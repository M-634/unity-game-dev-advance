using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DemoGame
{
    // 設定画面の部品を持つだけ。値が何を意味するか、どこに保存するかは知らない。
    public class SettingsView : MonoBehaviour
    {
        [Tooltip("画面全体。表示・非表示と操作の可否を切り替える")]
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [Header("Graphics")]
        [SerializeField]
        private OptionSelector _resolution;

        [SerializeField]
        private OptionSelector _screenMode;

        [SerializeField]
        private OptionSelector _quality;

        [SerializeField]
        private OptionSelector _vSync;

        [SerializeField]
        private OptionSelector _frameRate;

        [Header("Sound")]
        [SerializeField]
        private VolumeSlider _masterVolume;

        [SerializeField]
        private VolumeSlider _bgmVolume;

        [SerializeField]
        private VolumeSlider _seVolume;

        [Header("Buttons")]
        [SerializeField]
        private Button _resetButton;

        [SerializeField]
        private Button _backButton;

        [Tooltip("戻る入力（Esc・ゲームパッドの B）。EventSystem と同じ UI/Cancel を指定する")]
        [SerializeField]
        private InputActionReference _cancelAction;

        private readonly Subject<Unit> _resetClicked = new();
        private readonly Subject<Unit> _backRequested = new();
        private bool _isShown;

        public OptionSelector Resolution => _resolution;
        public OptionSelector ScreenMode => _screenMode;
        public OptionSelector Quality => _quality;
        public OptionSelector VSync => _vSync;
        public OptionSelector FrameRate => _frameRate;
        public VolumeSlider MasterVolume => _masterVolume;
        public VolumeSlider BgmVolume => _bgmVolume;
        public VolumeSlider SeVolume => _seVolume;

        public Observable<Unit> ResetClicked => _resetClicked;

        // BACK ボタンとキャンセル入力の両方
        public Observable<Unit> BackRequested => _backRequested;

        public void Show()
        {
            SetShown(true);

            if (EventSystem.current != null && _resolution != null)
            {
                EventSystem.current.SetSelectedGameObject(_resolution.gameObject);
            }
        }

        public void Hide()
        {
            SetShown(false);
        }

        private void SetShown(bool shown)
        {
            _isShown = shown;

            if (_canvasGroup == null) return;

            _canvasGroup.alpha = shown ? 1f : 0f;
            _canvasGroup.interactable = shown;
            _canvasGroup.blocksRaycasts = shown;
        }

        private void Awake()
        {
            if (_resetButton != null) _resetButton.onClick.AddListener(OnResetClicked);
            if (_backButton != null) _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            if (_cancelAction != null) _cancelAction.action.performed += OnCancel;
        }

        private void OnDisable()
        {
            if (_cancelAction != null) _cancelAction.action.performed -= OnCancel;
        }

        private void OnResetClicked()
        {
            _resetClicked.OnNext(Unit.Default);
        }

        private void OnBackClicked()
        {
            _backRequested.OnNext(Unit.Default);
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            if (!_isShown) return;

            _backRequested.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            if (_resetButton != null) _resetButton.onClick.RemoveListener(OnResetClicked);
            if (_backButton != null) _backButton.onClick.RemoveListener(OnBackClicked);

            _resetClicked.Dispose();
            _backRequested.Dispose();
        }
    }
}
