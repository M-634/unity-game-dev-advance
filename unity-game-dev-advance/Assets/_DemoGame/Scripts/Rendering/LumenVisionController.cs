using R3;
using UnityEngine;
using UnityEngine.Rendering;

namespace DemoGame
{
    public class LumenVisionController : MonoBehaviour
    {
        [SerializeField]
        private PlayerLumen _lumen;

        [SerializeField]
        private PlayerAttack _attack;

        [SerializeField]
        private Volume _darkVolume;

        [SerializeField]
        private Volume _chargeVolume;

        [Tooltip("チャージの重みが目標に追いつくまでの秒数")]
        [SerializeField, Range(0.01f, 1f)]
        private float _chargeSmoothTime = 0.15f;

        private readonly CompositeDisposable _disposables = new();
        private float _chargeTarget;
        private float _chargeVelocity;

        private void Start()
        {
            if (_lumen == null)
            {
                _lumen = FindFirstObjectByType<PlayerLumen>();
            }

            if (_attack == null)
            {
                _attack = FindFirstObjectByType<PlayerAttack>();
            }

            if (_lumen != null && _darkVolume != null)
            {
                _lumen.Lumen.Subscribe(value => _darkVolume.weight = 1f - Mathf.Clamp01(value / PlayerLumen.MaxLumen)).AddTo(_disposables);
            }

            if (_attack != null)
            {
                _attack.ChargeLevel.Subscribe(level => _chargeTarget = Mathf.Clamp01(level / (float)Staff.MaxGlowLevel)).AddTo(_disposables);
            }

            if (_chargeVolume != null)
            {
                _chargeVolume.weight = 0f;
            }
        }

        private void Update()
        {
            if (_chargeVolume == null) return;

            _chargeVolume.weight = Mathf.SmoothDamp(_chargeVolume.weight, _chargeTarget, ref _chargeVelocity, _chargeSmoothTime);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
