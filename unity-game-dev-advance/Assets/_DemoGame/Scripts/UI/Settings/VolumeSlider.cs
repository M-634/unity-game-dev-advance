using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame
{
    public class VolumeSlider : MonoBehaviour
    {
        [Tooltip("Min 0・Max 1。キーボード・ゲームパッドでは 0.1 刻みで動く")]
        [SerializeField]
        private Slider _slider;

        [SerializeField]
        private TMP_Text _valueText;

        private readonly Subject<float> _valueChanged = new();

        public Observable<float> ValueChanged => _valueChanged;

        public void SetValueWithoutNotify(float value)
        {
            if (_slider != null) _slider.SetValueWithoutNotify(value);

            Refresh(value);
        }

        private void Awake()
        {
            if (_slider != null) _slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(float value)
        {
            Refresh(value);
            _valueChanged.OnNext(value);
        }

        private void Refresh(float value)
        {
            if (_valueText == null) return;

            _valueText.text = Mathf.RoundToInt(value * 100f).ToString();
        }

        private void OnDestroy()
        {
            if (_slider != null) _slider.onValueChanged.RemoveListener(OnValueChanged);

            _valueChanged.Dispose();
        }
    }
}
