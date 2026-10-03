using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    // ボス戦の篝火を出す・弱くする。設計：doc/demo-game/10-level-design.md §7
    //   ボスが動き出したら・怒ったら：床から篝火が出てくる（消えていた物も点き直す）
    //   ボスの攻撃が当たる瞬間（地面叩きつけ）：部屋の篝火がすべて1段弱くなる（3回で消える）
    //   全部消えたら、しばらくして出てくる
    public class BossBonfireSpawner : MonoBehaviour
    {
        [SerializeField]
        private BossController _boss;

        [Tooltip("ボス戦の篝火（Bonfire_Boss）。最初は床の下に隠れている")]
        [SerializeField]
        private Bonfire[] _bonfires = new Bonfire[0];

        [Tooltip("出てくるときにせり上がる高さ（m）")]
        [SerializeField, Range(0f, 5f)]
        private float _riseHeight = 1.5f;

        [SerializeField, Range(0.1f, 5f)]
        private float _riseSeconds = 1f;

        [Tooltip("せり上がるときに足元に出す砂煙（doc/demo-game/12-vfx.md #20）")]
        [SerializeField]
        private GameObject _riseEffect;

        [Tooltip("全部消えてから、また出てくるまでの秒数")]
        [SerializeField, Range(1f, 60f)]
        private float _respawnDelay = 8f;

        private readonly CompositeDisposable _disposables = new();
        private Vector3[] _raisedPositions = new Vector3[0];
        private bool _isRespawnWaiting;

        private void Awake()
        {
            _raisedPositions = new Vector3[_bonfires.Length];
            for (int i = 0; i < _bonfires.Length; i++)
            {
                if (_bonfires[i] == null) continue;

                _raisedPositions[i] = _bonfires[i].transform.position;
                _bonfires[i].gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (_boss == null)
            {
                Debug.LogError($"{nameof(BossBonfireSpawner)}: ボスが設定されていません。", this);
                return;
            }

            _boss.Activated.Subscribe(_ => RaiseAll()).AddTo(_disposables);
            _boss.IsAngry.Where(angry => angry).Subscribe(_ => RaiseAll()).AddTo(_disposables);
            _boss.AttackImpact.Subscribe(_ => OnSlam()).AddTo(_disposables);
        }

        private void OnSlam()
        {
            Bonfire.WeakenAll();

            if (!_isRespawnWaiting && !AnyBurning())
            {
                RespawnAsync(destroyCancellationToken).SuppressCancellationThrow().Forget();
            }
        }

        private bool AnyBurning()
        {
            foreach (var bonfire in _bonfires)
            {
                if (bonfire != null && bonfire.gameObject.activeSelf && bonfire.IsBurning) return true;
            }

            return false;
        }

        private async UniTask RespawnAsync(CancellationToken ct)
        {
            _isRespawnWaiting = true;
            await UniTask.Delay(System.TimeSpan.FromSeconds(_respawnDelay), cancellationToken: ct);
            _isRespawnWaiting = false;

            if (_boss != null && !_boss.IsDead)
            {
                RaiseAll();
            }
        }

        private void RaiseAll()
        {
            for (int i = 0; i < _bonfires.Length; i++)
            {
                var bonfire = _bonfires[i];
                if (bonfire == null || (bonfire.gameObject.activeSelf && bonfire.IsBurning)) continue;

                RaiseAsync(bonfire, _raisedPositions[i], destroyCancellationToken).SuppressCancellationThrow().Forget();
            }
        }

        // 床の下からせり上がって、点く（エフェクト：doc/demo-game/12-vfx.md #20）
        private async UniTask RaiseAsync(Bonfire bonfire, Vector3 raised, CancellationToken ct)
        {
            var t = bonfire.transform;
            Vector3 lowered = raised + Vector3.down * _riseHeight;

            t.position = lowered;
            FxSpawner.Spawn(_riseEffect, raised);
            bonfire.gameObject.SetActive(true);
            bonfire.Reignite();

            float elapsed = 0f;
            while (elapsed < _riseSeconds)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, elapsed / _riseSeconds);
                t.position = Vector3.Lerp(lowered, raised, k);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            t.position = raised;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
