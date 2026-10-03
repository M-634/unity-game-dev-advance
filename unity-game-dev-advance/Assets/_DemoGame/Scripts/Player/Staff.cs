using R3;
using UnityEngine;

namespace DemoGame
{
    // 杖の状態。設計：doc/demo-game/10-level-design.md §4.1
    //   火の量（Flame 0〜100）：火焔玉を撃つと減る。0 なら杖は消えている
    //   光の段（Glow 0〜3）  ：篝火で「点けた灯火の数」まで上がる。チャージできる上限。撃つと使った段だけ下がる
    // どちらも時間では減らない。見た目（StaffVisual）と明るさ（PlayerLumen）は、これを購読する側が決める
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

        // 篝火で光らせられる段。点けた灯火の数
        public int MaxGlow => _progress != null ? Mathf.Clamp(_progress.LitCount.CurrentValue, 0, MaxGlowLevel) : 0;

        // 火焔玉を出す位置。見た目が無ければ自分の位置
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

        // 導入演出で拾ったとき
        public void Acquire()
        {
            _isOwned.Value = true;
        }

        // 篝火で火を移す。火を満タンにし、光の段を glow まで上げる（下げない）。何か変わったら true
        public bool Refill(int glow)
        {
            if (!_isOwned.Value) return false;

            glow = Mathf.Clamp(glow, 0, MaxGlowLevel);
            bool changed = _flame.Value < MaxFlame || _glow.Value < glow;

            _flame.Value = MaxFlame;
            _glow.Value = Mathf.Max(_glow.Value, glow);
            return changed;
        }

        // 火焔玉を撃つときに呼ぶ。火が足りなければ残りを使い切る（撃てなくはしない）
        public void Use(float flameCost, int glowCost)
        {
            if (!IsLit) return;

            _flame.Value = Mathf.Max(0f, _flame.Value - flameCost);
            _glow.Value = Mathf.Max(0, _glow.Value - glowCost);

            // 火が消えたら光も消える
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
