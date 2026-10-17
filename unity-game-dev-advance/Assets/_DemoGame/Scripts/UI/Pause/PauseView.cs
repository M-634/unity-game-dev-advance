using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DemoGame
{
    public class PauseView : MonoBehaviour
    {
        [Tooltip("ポーズ画面全体。表示・非表示と操作の可否を切り替える")]
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private Button _resumeButton;

        [SerializeField]
        private Button _settingsButton;

        [SerializeField]
        private Button _titleButton;

        private readonly Subject<Unit> _resumeClicked = new();
        private readonly Subject<Unit> _settingsClicked = new();
        private readonly Subject<Unit> _titleClicked = new();
        private GameObject _lastSelected;

        public Observable<Unit> ResumeClicked => _resumeClicked;
        public Observable<Unit> SettingsClicked => _settingsClicked;
        public Observable<Unit> TitleClicked => _titleClicked;

        public void Show(bool restoreSelection)
        {
            SetShown(true);

            if (EventSystem.current == null) return;

            GameObject first = _resumeButton != null ? _resumeButton.gameObject : null;
            GameObject target = restoreSelection && _lastSelected != null ? _lastSelected : first;
            EventSystem.current.SetSelectedGameObject(target);
        }

        public void Hide()
        {
            if (EventSystem.current != null)
            {
                _lastSelected = EventSystem.current.currentSelectedGameObject;
            }

            SetShown(false);
        }

        public void SetInteractable(bool interactable)
        {
            if (_canvasGroup != null) _canvasGroup.interactable = interactable;
        }

        private void SetShown(bool shown)
        {
            if (_canvasGroup == null) return;

            _canvasGroup.alpha = shown ? 1f : 0f;
            _canvasGroup.interactable = shown;
            _canvasGroup.blocksRaycasts = shown;
        }

        private void Awake()
        {
            SetShown(false);

            if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResumeClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_titleButton != null) _titleButton.onClick.AddListener(OnTitleClicked);
        }

        private void OnResumeClicked()
        {
            _resumeClicked.OnNext(Unit.Default);
        }

        private void OnSettingsClicked()
        {
            _settingsClicked.OnNext(Unit.Default);
        }

        private void OnTitleClicked()
        {
            _titleClicked.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            if (_resumeButton != null) _resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (_titleButton != null) _titleButton.onClick.RemoveListener(OnTitleClicked);

            _resumeClicked.Dispose();
            _settingsClicked.Dispose();
            _titleClicked.Dispose();
        }
    }
}
