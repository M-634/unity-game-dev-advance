using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DemoGame
{
    // 「< 値 >」の選択部品。左右キー・決定・矢印のクリックで値が変わる。
    // 選ばれた番号を流すだけで、番号が何を意味するかは知らない。
    public class OptionSelector : Selectable, ISubmitHandler
    {
        [SerializeField]
        private TMP_Text _valueText;

        [Tooltip("矢印のボタン。Navigation は None にしておく（上下の移動で止まらないように）")]
        [SerializeField]
        private Button _previousButton;

        [SerializeField]
        private Button _nextButton;

        [Tooltip("選べないときに行ごと薄くする")]
        [SerializeField]
        private CanvasGroup _canvasGroup;

        private readonly Subject<int> _indexChanged = new();
        private IReadOnlyList<string> _options = Array.Empty<string>();
        private int _index;

        public Observable<int> IndexChanged => _indexChanged;

        public void SetOptions(IReadOnlyList<string> options)
        {
            _options = options ?? Array.Empty<string>();
            _index = Mathf.Clamp(_index, 0, Mathf.Max(0, _options.Count - 1));
            Refresh();
        }

        public void SetIndexWithoutNotify(int index)
        {
            if (_options.Count == 0) return;

            _index = Mathf.Clamp(index, 0, _options.Count - 1);
            Refresh();
        }

        public void SetInteractable(bool value)
        {
            interactable = value;

            if (_previousButton != null) _previousButton.interactable = value;
            if (_nextButton != null) _nextButton.interactable = value;
            if (_canvasGroup != null) _canvasGroup.alpha = value ? 1f : 0.35f;
        }

        protected override void Awake()
        {
            base.Awake();

            // Selectable は Edit Mode でも動くので、再生中だけつなぐ
            if (!Application.isPlaying) return;

            if (_previousButton != null) _previousButton.onClick.AddListener(OnPreviousClicked);
            if (_nextButton != null) _nextButton.onClick.AddListener(OnNextClicked);
        }

        public override void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    Step(-1);
                    break;
                case MoveDirection.Right:
                    Step(1);
                    break;
                default:
                    base.OnMove(eventData);
                    break;
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Step(1);
        }

        private void OnPreviousClicked()
        {
            Step(-1);
            Select();
        }

        private void OnNextClicked()
        {
            Step(1);
            Select();
        }

        // 端まで行ったら反対側へ回る
        private void Step(int delta)
        {
            if (!IsInteractable() || _options.Count <= 1) return;

            _index = (_index + delta + _options.Count) % _options.Count;
            Refresh();
            _indexChanged.OnNext(_index);
        }

        private void Refresh()
        {
            if (_valueText == null) return;

            _valueText.text = _options.Count > 0 ? _options[_index] : string.Empty;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!Application.isPlaying) return;

            if (_previousButton != null) _previousButton.onClick.RemoveListener(OnPreviousClicked);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(OnNextClicked);

            _indexChanged.Dispose();
        }
    }
}
