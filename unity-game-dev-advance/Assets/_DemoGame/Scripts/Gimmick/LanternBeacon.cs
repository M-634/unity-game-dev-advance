using UnityEngine;

namespace DemoGame
{
    public class LanternBeacon : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private GameProgress _progress;

        [SerializeField]
        private Lantern _lantern;

        [Tooltip("何番目に点ける灯火か。この数 - 1 個が点いているとき、強く脈打つ")]
        [SerializeField, Range(1, GameProgress.RequiredLitCount)]
        private int _order = 1;

        [Tooltip("霧の向こうから見える細い光")]
        [SerializeField]
        private Renderer _beam;

        [SerializeField]
        private Color _color = new Color(1f, 0.8f, 0.45f);

        [Header("明るさ（不透明度）")]
        [SerializeField, Range(0f, 1f)]
        private float _faintAlpha = 0.08f;

        [SerializeField, Range(0f, 1f)]
        private float _pulseMinAlpha = 0.15f;

        [SerializeField, Range(0f, 1f)]
        private float _pulseMaxAlpha = 0.55f;

        [Tooltip("脈打つ速さ（1 秒あたりの回数）")]
        [SerializeField, Range(0.1f, 3f)]
        private float _pulsePerSecond = 0.8f;

        [SerializeField, Range(1f, 3f)]
        private float _pulseWidthScale = 1.6f;

        private MaterialPropertyBlock _block;
        private Vector3 _beamScale;

        public int Order => _order;

        private void Awake()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }

            if (_lantern == null)
            {
                _lantern = GetComponentInParent<Lantern>();
            }

            _block = new MaterialPropertyBlock();

            if (_beam != null)
            {
                _beamScale = _beam.transform.localScale;
            }
        }

        private void Update()
        {
            if (_beam == null) return;

            bool isLit = _lantern != null && _lantern.IsLit;
            _beam.enabled = !isLit;
            if (isLit) return;

            bool isNext = _progress != null && _progress.LitCount.CurrentValue == _order - 1;
            float alpha = _faintAlpha;
            float width = 1f;

            if (isNext)
            {
                float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * _pulsePerSecond * Mathf.PI * 2f);
                alpha = Mathf.Lerp(_pulseMinAlpha, _pulseMaxAlpha, wave);
                width = Mathf.Lerp(1f, _pulseWidthScale, wave);
            }

            _beam.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, new Color(_color.r, _color.g, _color.b, alpha));
            _beam.SetPropertyBlock(_block);
            _beam.transform.localScale = new Vector3(_beamScale.x * width, _beamScale.y, _beamScale.z * width);
        }
    }
}
