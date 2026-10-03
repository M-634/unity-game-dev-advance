using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DemoGame
{
    // ボタンが押されたことを流すだけ。押されたら何が起きるかは知らない。
    public class TitleView : MonoBehaviour
    {
        [Tooltip("タイトル画面全体。表示・非表示と操作の可否を切り替える")]
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private Button _startButton;

        [SerializeField]
        private Button _settingsButton;

        [SerializeField]
        private Button _quitButton;

        private readonly Subject<Unit> _startClicked = new();
        private readonly Subject<Unit> _settingsClicked = new();
        private readonly Subject<Unit> _quitClicked = new();
        private GameObject _lastSelected;

        public Observable<Unit> StartClicked => _startClicked;
        public Observable<Unit> SettingsClicked => _settingsClicked;
        public Observable<Unit> QuitClicked => _quitClicked;

        // 表示して、前に選んでいたボタン（初回は START）を選び直す
        public void Show()
        {
            SetShown(true);

            if (EventSystem.current == null) return;

            GameObject target = _lastSelected != null ? _lastSelected : _startButton != null ? _startButton.gameObject : null;
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

        // 開始・終了を受け付けたあと、ボタンを押せなくする（見た目はそのまま）
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
            if (_startButton != null) _startButton.onClick.AddListener(OnStartClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnStartClicked()
        {
            _startClicked.OnNext(Unit.Default);
        }

        private void OnSettingsClicked()
        {
            _settingsClicked.OnNext(Unit.Default);
        }

        private void OnQuitClicked()
        {
            _quitClicked.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            if (_startButton != null) _startButton.onClick.RemoveListener(OnStartClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(OnQuitClicked);

            _startClicked.Dispose();
            _settingsClicked.Dispose();
            _quitClicked.Dispose();
        }
    }
}
