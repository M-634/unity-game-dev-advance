using R3;
using UnityEngine;

namespace Section.Common
{
    public class PlayerLumen : MonoBehaviour
    {
        public const float MaxLumen = 100f;
        
        [SerializeField]
        private Staff _staff;

        [Tooltip("点けた灯火の数（0〜3）ごとの、ふだんの明るさ")]
        [SerializeField]
        private float[] _baseByLitCount = { 20f, 35f, 50f, 65f };

        [Tooltip("杖の火が満タンのときに足す明るさ")]
        [SerializeField, Range(0f, 50f)]
        private float _flameAmount = 20f;

        [Tooltip("杖の光の段 1 つごとに足す明るさ")]
        [SerializeField, Range(0f, 20f)]
        private float _glowAmountPerLevel = 5f;

        [Tooltip("目標の値に追いつくまでの時間（秒）。急に明るさが跳ねないように")]
        [SerializeField, Range(0.01f, 3f)]
        private float _smoothTime = 0.5f;

        private readonly ReactiveProperty<float> _lumen = new(0f);
        private float _velocity;

        public ReadOnlyReactiveProperty<float> Lumen => _lumen;

        // 0〜1 にしたもの
        public float Normalized => _lumen.Value / MaxLumen;

        private void Awake()
        {
            if (_staff == null)
            {
                _staff = GetComponent<Staff>();
            }

            _lumen.Value = CalculateTarget();
        }

        private void Update()
        {
            float target = CalculateTarget();
            float value = Mathf.SmoothDamp(_lumen.Value, target, ref _velocity, _smoothTime);

            // 細かい揺れで購読側を毎フレーム起こさない
            if (Mathf.Abs(value - _lumen.Value) > 0.01f)
            {
                _lumen.Value = value;
            }
        }

        private float CalculateTarget()
        {
            float value = 0f;
            
            if (_staff != null && _staff.IsLit)
            {
                value += _flameAmount * _staff.Flame.CurrentValue / Staff.MaxFlame;
                value += _glowAmountPerLevel * _staff.Glow.CurrentValue;
            }

            return Mathf.Clamp(value, 0f, MaxLumen);
        }

        private void OnDestroy()
        {
            _lumen.Dispose();
        }
    }
}
