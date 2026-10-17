using R3;
using UnityEngine;

namespace DemoGame
{
    public class Staff : MonoBehaviour
    {
        public const float MaxFlame = 100f;
        public const int MaxGlowLevel = 3;

        [SerializeField]
        private GameProgress _progress;

        [Tooltip("杖の見た目（WeaponSocket の下の杖の Prefab）。未設定なら子から探す")]
        [SerializeField]
        private StaffVisual _visual;

        [Tooltip("最初から杖を持っているか。導入演出で拾うようになったら OFF にする")]
        [SerializeField]
        private bool _ownedOnStart = true;

        [Tooltip("最初から杖が灯っているか（確認用）")]
        [SerializeField]
        private bool _litOnStart;

        private readonly ReactiveProperty<bool> _isOwned = new(false);
        private readonly ReactiveProperty<float> _flame = new(0f);
        private readonly ReactiveProperty<int> _glow = new(0);
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReadOnlyReactiveProperty<bool> IsOwned => _isOwned;
        public ReadOnlyReactiveProperty<float> Flame => _flame;
        public ReadOnlyReactiveProperty<int> Glow => _glow;

        public bool IsLit => _isOwned.Value && _flame.Value > 0f;

        public int MaxGlow => _progress != null ? Mathf.Clamp(_progress.LitCount.CurrentValue, 0, MaxGlowLevel) : 0;

        public Transform Muzzle => _visual != null && _visual.Muzzle != null ? _visual.Muzzle : transform;

        public StaffVisual Visual => _visual;

        private void Awake()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }

            if (_visual == null)
            {
                _visual = GetComponentInChildren<StaffVisual>(true);
            }

            _isOwned.Value = _ownedOnStart;
            if (_ownedOnStart && _litOnStart)
            {
                _flame.Value = MaxFlame;
            }
        }

        private void Start()
        {
            if (_visual == null) return;

            _isOwned.Subscribe(_visual.SetOwned).AddTo(_disposables);
            _flame.CombineLatest(_glow, (flame, glow) => (flame, glow))
                .Subscribe(state => _visual.SetFlame(state.flame / MaxFlame, state.glow))
                .AddTo(_disposables);
        }

        public void Acquire()
        {
            _isOwned.Value = true;
        }

        public bool Refill(int glow)
        {
            if (!_isOwned.Value) return false;

            glow = Mathf.Clamp(glow, 0, MaxGlowLevel);
            bool changed = _flame.Value < MaxFlame || _glow.Value < glow;

            _flame.Value = MaxFlame;
            _glow.Value = Mathf.Max(_glow.Value, glow);
            return changed;
        }

        public void Use(float flameCost, int glowCost)
        {
            if (!IsLit) return;

            _flame.Value = Mathf.Max(0f, _flame.Value - flameCost);
            _glow.Value = Mathf.Max(0, _glow.Value - glowCost);

            if (_flame.Value <= 0f)
            {
                _glow.Value = 0;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _isOwned.Dispose();
            _flame.Dispose();
            _glow.Dispose();
        }
    }
}
