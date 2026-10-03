using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    // 火焔玉で燃える蔦。燃え尽きたら消えて、Burnt を流す。設計：doc/demo-game/10-level-design.md §4.4（灯火 3）・§7（ボス戦）
    //   道をふさぐ蔦：燃え尽きたら通れる
    //   瓦礫を吊る蔦：燃え尽きたら FallingRubble が落ちる
    // 燃やし方は2つ：火焔玉を当てる ／ 杖が灯っている状態で近くで E（杖を伸ばして火をつける。PlayerStaffReach）
    // 火焔玉に当たるよう、トリガーではないコライダーを付けておく。E で燃やせるようにするなら Layer Interactable にする
    public class BurnableVine : MonoBehaviour, IFlammable, IInteractable
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Tooltip("燃えてから燃え尽きるまでの秒数")]
        [SerializeField, Range(0.1f, 10f)]
        private float _burnSeconds = 2f;

        [Tooltip("燃え尽きて消えるまでの秒数（溶ける）")]
        [SerializeField, Range(0.1f, 5f)]
        private float _dissolveSeconds = 0.8f;

        [SerializeField, ColorUsage(false, true)]
        private Color _burningEmission = new Color(4f, 1.2f, 0.2f);

        [Tooltip("残っている（有効な）間は E で燃やせない物（まわりを囲む魔法の壁など）")]
        [SerializeField]
        private GameObject[] _blockers = new GameObject[0];

        [Tooltip("燃えている間の炎（FX_VineBurn。doc/demo-game/12-vfx.md #18）。形は蔦のメッシュに合わせる（実行時に設定する）")]
        [SerializeField]
        private GameObject _burnEffectPrefab;

        private GameObject _burnEffect;

        [Tooltip("燃え尽きたら落とす瓦礫。未設定なら落とさない")]
        [SerializeField]
        private FallingRubble[] _rubbles = new FallingRubble[0];

        private readonly Subject<Unit> _burnt = new();

        public bool IsBurnt { get; private set; }
        public bool IsBurning { get; private set; }
        public Observable<Unit> Burnt => _burnt;

        public string Prompt => "Burn";

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (IsBurning || IsBurnt || PlayerStaffReach.IsBusy(interactor)) return false;

            foreach (var blocker in _blockers)
            {
                if (blocker != null && blocker.activeInHierarchy) return false;
            }

            var staff = interactor.GetComponentInParent<Staff>();
            return staff != null && staff.IsLit;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;

            // 杖を伸ばす先は、蔦の表面のうち手（胸の高さ）にいちばん近い所
            Vector3 hand = interactor.transform.position + Vector3.up * 1.2f;
            var col = GetComponentInChildren<Collider>();
            Vector3 point = col != null ? col.ClosestPoint(hand) : transform.position;

            PlayerStaffReach.ReachOrRun(interactor, point, Ignite);
        }

        public void Ignite()
        {
            if (IsBurning || IsBurnt) return;

            IsBurning = true;
            BurnAsync(destroyCancellationToken).SuppressCancellationThrow().Forget();
        }

        // 燃える（エフェクト：doc/demo-game/12-vfx.md #18）→ 溶けて消える
        private async UniTask BurnAsync(CancellationToken ct)
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            PlayBurnEffect(renderers.Length > 0 ? renderers[0] : null);

            float elapsed = 0f;
            while (elapsed < _burnSeconds)
            {
                elapsed += Time.deltaTime;
                float flicker = 0.75f + 0.25f * Mathf.PerlinNoise(Time.time * 8f, 0f);
                SetEmission(renderers, _burningEmission * (flicker * Mathf.Clamp01(elapsed / 0.3f)));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // 燃え尽きた時点で、通れる・落ちる
            FxSpawner.Stop(_burnEffect);
            if (_burnEffect != null)
            {
                // 残った火の粉が消えたら片づける（蔦より先に消えないよう、蔦の子にはしていない）
                Destroy(_burnEffect, 3f);
            }

            IsBurning = false;
            IsBurnt = true;

            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            foreach (var rubble in _rubbles)
            {
                if (rubble != null) rubble.Drop();
            }

            _burnt.OnNext(Unit.Default);
            _burnt.OnCompleted();

            elapsed = 0f;
            while (elapsed < _dissolveSeconds)
            {
                elapsed += Time.deltaTime;
                DissolveBehaviour.SetDissolve(renderers, Mathf.Clamp01(elapsed / _dissolveSeconds));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            gameObject.SetActive(false);
        }

        private void PlayBurnEffect(MeshRenderer meshRenderer)
        {
            if (_burnEffectPrefab == null) return;

            // 形はワールド座標のメッシュで決まるので、蔦の子にしない（蔦の拡縮が炎の大きさに乗らないように）
            _burnEffect = Instantiate(_burnEffectPrefab, transform.position, Quaternion.identity);

            // 炎は蔦の表面から出す。子のパーティクルも同じ形に
            if (meshRenderer != null)
            {
                foreach (var ps in _burnEffect.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var shape = ps.shape;
                    shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                    shape.meshRenderer = meshRenderer;
                }
            }

            FxSpawner.Play(_burnEffect);
        }

        private static void SetEmission(Renderer[] renderers, Color color)
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                foreach (var material in renderer.materials)
                {
                    material.SetColor(EmissionColorId, color);
                }
            }
        }

        private void OnDestroy()
        {
            _burnt.Dispose();
        }
    }
}
