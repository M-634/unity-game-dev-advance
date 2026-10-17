using TMPro;
using UnityEngine;

namespace DemoGame
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;

        [SerializeField, Range(0.1f, 3f)]
        private float _lifetime = 0.8f;

        [Tooltip("消えるまでに浮かぶ高さ（m）")]
        [SerializeField, Range(0f, 3f)]
        private float _riseHeight = 0.8f;

        [Tooltip("出た瞬間の大きさ。すぐ 1 に戻る")]
        [SerializeField, Range(1f, 3f)]
        private float _popScale = 1.6f;

        [SerializeField]
        private Color _color = new Color(1f, 0.95f, 0.6f);

        [Tooltip("ダメージ 0（無敵）のときの色")]
        [SerializeField]
        private Color _noDamageColor = new Color(0.6f, 0.6f, 0.6f);

        private Vector3 _startPosition;
        private Vector3 _baseScale;
        private Color _currentColor;
        private float _elapsed;

        private void Awake()
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }

            _baseScale = transform.localScale;
        }

        public void Show(int damage)
        {
            _startPosition = transform.position;
            _elapsed = 0f;
            _currentColor = damage > 0 ? _color : _noDamageColor;

            if (_text != null)
            {
                _text.text = damage.ToString();
                _text.color = _currentColor;
            }

            Apply(0f);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifetime);

            Apply(t);

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void Apply(float t)
        {
            float rise = 1f - (1f - t) * (1f - t);
            transform.position = _startPosition + Vector3.up * (_riseHeight * rise);

            float pop = Mathf.Lerp(_popScale, 1f, Mathf.Clamp01(t / 0.15f));
            transform.localScale = _baseScale * pop;

            if (_text != null)
            {
                var color = _currentColor;
                color.a = 1f - Mathf.Clamp01((t - 0.5f) / 0.5f);
                _text.color = color;
            }
        }
    }
}
