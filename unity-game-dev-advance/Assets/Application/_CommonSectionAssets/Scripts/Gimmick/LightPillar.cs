using UnityEngine;

namespace Section.Common
{
    public class LightPillar : MonoBehaviour
    {
        [Tooltip("縦に伸ばすもの（柱のメッシュ。原点が下端）")]
        [SerializeField]
        private Transform _beam;

        [SerializeField]
        private Light _light;

        [SerializeField, Range(0.1f, 10f)]
        private float _riseSeconds = 2f;

        [Tooltip("柱の中を昇る光の粒（FX_PillarMotes）")]
        [SerializeField]
        private GameObject _motesPrefab;

        private Vector3 _beamScale = Vector3.one;
        private float _lightIntensity;
        private float _elapsed;
        private bool _isInitialized;

        private void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            if (_beam != null)
            {
                _beamScale = _beam.localScale;
            }

            if (_light != null)
            {
                _lightIntensity = _light.intensity;
            }

            FxSpawner.Attach(_motesPrefab, transform);
        }

        private void OnEnable()
        {
            Initialize();
            _elapsed = 0f;
            Apply(0f);
        }

        private void Update()
        {
            if (_elapsed >= _riseSeconds) return;

            _elapsed += Time.deltaTime;
            Apply(Mathf.Clamp01(_elapsed / _riseSeconds));
        }

        private void Apply(float t)
        {
            // 速く伸び始めて、ゆっくり止まる
            float eased = 1f - (1f - t) * (1f - t);

            if (_beam != null)
            {
                _beam.localScale = new Vector3(_beamScale.x, _beamScale.y * Mathf.Max(0.001f, eased), _beamScale.z);
            }

            if (_light != null)
            {
                _light.intensity = _lightIntensity * eased;
            }
        }
    }
}
