using TMPro;
using UnityEngine;

namespace DemoGame
{
    // 表示だけを担当する。何を出すかは Presenter が決める。
    public class HudView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _hpText;

        [SerializeField]
        private TMP_Text _litCountText;

        [Tooltip("「[E] Light」などのインタラクト表示")]
        [SerializeField]
        private TMP_Text _interactText;

        [Header("ボスの HP バー")]
        [Tooltip("ボスの HP バー全体。表示・非表示を切り替える対象")]
        [SerializeField]
        private GameObject _bossHpRoot;

        [Tooltip("中身。横幅（localScale.x）を残り HP の割合にする。Pivot は左端（x = 0）にしておく")]
        [SerializeField]
        private RectTransform _bossHpFill;

        public void SetHp(int current, int max)
        {
            if (_hpText == null) return;

            _hpText.text = $"HP  {current} / {max}";
        }

        public void SetLitCount(int current, int required)
        {
            if (_litCountText == null) return;

            _litCountText.text = $"{current} / {required}";
        }

        public void SetBossHpVisible(bool visible)
        {
            if (_bossHpRoot == null) return;

            _bossHpRoot.SetActive(visible);
        }

        public void SetBossHp(int current, int max)
        {
            if (_bossHpFill == null || max <= 0) return;

            var scale = _bossHpFill.localScale;
            scale.x = Mathf.Clamp01((float)current / max);
            _bossHpFill.localScale = scale;
        }

        // null なら表示を消す
        public void SetInteractPrompt(string prompt)
        {
            if (_interactText == null) return;

            bool visible = !string.IsNullOrEmpty(prompt);
            _interactText.gameObject.SetActive(visible);

            if (visible)
            {
                _interactText.text = $"[E] {prompt}";
            }
        }
    }
}
