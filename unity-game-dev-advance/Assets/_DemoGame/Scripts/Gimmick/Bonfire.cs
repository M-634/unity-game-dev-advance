using System.Collections.Generic;
using R3;
using UnityEngine;

namespace DemoGame
{
    // 篝火。近くで E を押すと、杖を炎へ伸ばして（PlayerStaffReach）火を移す。設計：doc/demo-game/10-level-design.md §4.2・§7
    //   ステージの篝火：いつも燃えている。杖の光の段は「点けた灯火の数」まで
    //   ボス戦の篝火  ：いつでもフルチャージ。ボスの地面叩きつけ（WeakenAll）で1段ずつ弱くなり、0 で消える
    // PlayerInteractor に見つけてもらうため、この GameObject は Layer Interactable にし、コライダー（トリガー）を付けておく
    public class Bonfire : MonoBehaviour, IInteractable
    {
        private static readonly List<Bonfire> Weakenable = new List<Bonfire>();

        [SerializeField]
        private GameProgress _progress;

        [Header("ボス戦の篝火")]
        [Tooltip("叩きつけで弱くなるか（ボス戦の篝火）")]
        [SerializeField]
        private bool _canWeaken;

        [Tooltip("杖を近づけると、灯火の数に関係なく光の段を最大にするか（ボス戦の篝火）")]
        [SerializeField]
        private bool _fullCharge;

        [SerializeField, Range(1, 5)]
        private int _maxStrength = 3;

        [Tooltip("杖を伸ばす先（炎の中）。未設定なら炎の位置")]
        [SerializeField]
        private Transform _reachPoint;

        [Tooltip("杖に火が移ったときに、杖を伸ばした先に出すエフェクト（doc/demo-game/12-vfx.md #3）")]
        [SerializeField]
        private GameObject _kindleEffect;

        [Header("見た目")]
        [Tooltip("炎のエフェクト（FX_Fire）。_flame の子に付ける（強さで _flame ごと小さくなる）")]
        [SerializeField]
        private GameObject _fireEffectPrefab;

        [Tooltip("炎（強さで大きさを変える）。エフェクトができたら Particle System の親にする")]
        [SerializeField]
        private Transform _flame;

        [SerializeField]
        private Light _light;

        [Tooltip("いちばん弱い段（1）のときの、炎とライトの大きさの割合")]
        [SerializeField, Range(0.1f, 1f)]
        private float _weakestScale = 0.4f;

        [Tooltip("ライトの揺らぎ（強さに対する割合）")]
        [SerializeField, Range(0f, 0.5f)]
        private float _flickerAmount = 0.12f;

        [SerializeField, Range(0.1f, 20f)]
        private float _flickerSpeed = 6f;

        private readonly ReactiveProperty<int> _strength = new(0);
        private readonly Subject<Staff> _refilled = new();

        private Vector3 _flameScale = Vector3.one;
        private float _lightIntensity;
        private float _lightRange;
        private float _noiseOffset;

        public ReadOnlyReactiveProperty<int> Strength => _strength;

        // 杖に火を移したとき（火が移るエフェクト。doc/demo-game/12-vfx.md #3）
        public Observable<Staff> Refilled => _refilled;

        public bool IsBurning => _strength.Value > 0;

        // ボスの地面叩きつけで呼ぶ。部屋の篝火がすべて1段弱くなる
        public static void WeakenAll()
        {
            for (int i = Weakenable.Count - 1; i >= 0; i--)
            {
                Weakenable[i].Weaken();
            }
        }

        private void Awake()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }

            if (_flame != null)
            {
                _flameScale = _flame.localScale;
                FxSpawner.Attach(_fireEffectPrefab, _flame);
            }

            if (_light != null)
            {
                _lightIntensity = _light.intensity;
                _lightRange = _light.range;
            }

            _noiseOffset = Random.value * 100f;
            _strength.Value = _maxStrength;
            Apply();
        }

        private void OnEnable()
        {
            if (_canWeaken)
            {
                Weakenable.Add(this);
            }
        }

        private void OnDisable()
        {
            Weakenable.Remove(this);
        }

        public string Prompt => "Kindle";

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (!IsBurning || PlayerStaffReach.IsBusy(interactor)) return false;

            var staff = interactor.GetComponentInParent<Staff>();
            if (staff == null || !staff.IsOwned.CurrentValue) return false;

            // 火が満タンで、もう光らせられないなら出さない（E を押しても何も起きないので）
            return staff.Flame.CurrentValue < Staff.MaxFlame || staff.Glow.CurrentValue < GlowFor(staff);
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;

            var staff = interactor.GetComponentInParent<Staff>();
            Vector3 point = ReachPosition();
            PlayerStaffReach.ReachOrRun(interactor, point, () =>
            {
                // 腕を伸ばしている間に消えていたら移らない
                if (IsBurning && staff.Refill(GlowFor(staff)))
                {
                    FxSpawner.Spawn(_kindleEffect, point);
                    _refilled.OnNext(staff);
                }
            });
        }

        private int GlowFor(Staff staff)
        {
            return _fullCharge ? Staff.MaxGlowLevel : staff.MaxGlow;
        }

        private Vector3 ReachPosition()
        {
            if (_reachPoint != null) return _reachPoint.position;
            if (_flame != null) return _flame.position;
            return transform.position + Vector3.up;
        }

        public void Weaken()
        {
            if (!_canWeaken || !IsBurning) return;

            _strength.Value--;
            Apply();
        }

        // ボス戦で篝火が出てくるとき（BossBonfireSpawner から）
        public void Reignite()
        {
            _strength.Value = _maxStrength;
            Apply();
        }

        private void Update()
        {
            if (_light == null || !IsBurning) return;

            float noise = Mathf.PerlinNoise(_noiseOffset, Time.time * _flickerSpeed) * 2f - 1f;
            _light.intensity = _lightIntensity * StrengthScale() * (1f + noise * _flickerAmount);
        }

        private float StrengthScale()
        {
            if (_maxStrength <= 1) return 1f;

            float t = (_strength.Value - 1) / (float)(_maxStrength - 1);
            return Mathf.Lerp(_weakestScale, 1f, t);
        }

        private void Apply()
        {
            bool burning = IsBurning;
            float scale = burning ? StrengthScale() : 0f;

            if (_flame != null)
            {
                _flame.gameObject.SetActive(burning);
                _flame.localScale = _flameScale * scale;
            }

            if (_light != null)
            {
                _light.enabled = burning;
                _light.intensity = _lightIntensity * scale;
                _light.range = _lightRange * Mathf.Lerp(1f, scale, 0.5f);
            }
        }

        private void OnDestroy()
        {
            _strength.Dispose();
            _refilled.Dispose();
        }
    }
}
