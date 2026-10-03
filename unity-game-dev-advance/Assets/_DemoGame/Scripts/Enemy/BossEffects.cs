using R3;
using UnityEngine;

namespace DemoGame
{
    // ボスのエフェクト。ボスの状態を見て出すだけで、ボスの動きには関わらない。設計：doc/demo-game/12-vfx.md #14・#15
    //   攻撃の瞬間（AttackImpact）：前の地面に衝撃波・砂煙・岩の破片
    //   怒ったら：体の表面から炎のオーラ（形はボスの SkinnedMeshRenderer に合わせる）
    public class BossEffects : MonoBehaviour
    {
        [SerializeField]
        private BossController _boss;

        [Tooltip("地面叩きつけのエフェクト")]
        [SerializeField]
        private GameObject _slamEffect;

        [Tooltip("ボスの足元から、どれだけ前に出すか（m）。叩きつける手が地面に着く所")]
        [SerializeField, Range(0f, 10f)]
        private float _slamForward = 3f;

        [Tooltip("怒りのオーラ（ループ）")]
        [SerializeField]
        private GameObject _angerAuraPrefab;

        private readonly CompositeDisposable _disposables = new();
        private GameObject _aura;

        private void Start()
        {
            if (_boss == null)
            {
                Debug.LogError($"{nameof(BossEffects)}: ボスが設定されていません。", this);
                return;
            }

            _boss.AttackImpact.Subscribe(_ => SpawnSlam()).AddTo(_disposables);
            _boss.IsAngry.Where(angry => angry).Take(1).Subscribe(_ => StartAura()).AddTo(_disposables);
            _boss.Died.Subscribe(_ => StopAura()).AddTo(_disposables);
        }

        private void SpawnSlam()
        {
            if (_boss == null) return;

            var t = _boss.transform;
            Vector3 forward = t.forward;
            forward.y = 0f;
            Vector3 point = t.position + forward.normalized * _slamForward;
            FxSpawner.Spawn(_slamEffect, point);
        }

        private void StartAura()
        {
            if (_angerAuraPrefab == null || _boss == null || _aura != null) return;

            _aura = Instantiate(_angerAuraPrefab, _boss.transform);
            _aura.transform.localPosition = Vector3.zero;

            // 炎は体の表面から出す（モデルを差し替えても追従するよう、実行時に探す）
            var body = _boss.GetComponentInChildren<SkinnedMeshRenderer>();
            if (body == null) return;

            foreach (var ps in _aura.GetComponentsInChildren<ParticleSystem>(true))
            {
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                shape.skinnedMeshRenderer = body;
            }
        }

        private void StopAura()
        {
            if (_aura == null) return;

            foreach (var ps in _aura.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
