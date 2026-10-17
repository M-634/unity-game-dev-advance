using R3;
using UnityEngine;

namespace DemoGame
{
    public enum PauseScreen
    {
        None,
        Menu,
        Settings,
    }

    public class PausePresenter : MonoBehaviour
    {
        [SerializeField]
        private PauseView _view;

        [SerializeField]
        private SettingsPresenter _settings;

        [SerializeField]
        private InputReader _input;

        [SerializeField]
        private InGameController _controller;

        private readonly CompositeDisposable _disposables = new();
        private readonly ReactiveProperty<PauseScreen> _screen = new(PauseScreen.None);
        private CursorLockMode _cursorLockBeforePause;
        private bool _cursorVisibleBeforePause;
        private int _settingsClosedFrame = -1;
        private bool _isTransitionRequested;

        private void Start()
        {
            if (_view == null || _settings == null || _input == null)
            {
                Debug.LogError($"{nameof(PausePresenter)}: 参照が設定されていません。", this);
                return;
            }

            _input.PausePressed += OnPausePressed;

            _view.ResumeClicked.Subscribe(_ => Resume()).AddTo(_disposables);
            _view.SettingsClicked.Subscribe(_ => _screen.Value = PauseScreen.Settings).AddTo(_disposables);
            _view.TitleClicked.Subscribe(_ => GoToTitle()).AddTo(_disposables);
            _settings.Closed.Subscribe(_ =>
            {
                _settingsClosedFrame = Time.frameCount;
                _screen.Value = PauseScreen.Menu;
            }).AddTo(_disposables);

            _screen.Pairwise().Subscribe(pair => ShowScreen(pair.Previous, pair.Current)).AddTo(_disposables);
        }

        private void OnPausePressed()
        {
            switch (_screen.Value)
            {
                case PauseScreen.None:
                    if (CanPause()) Pause();
                    break;
                case PauseScreen.Menu:
                    if (_settingsClosedFrame != Time.frameCount) Resume();
                    break;
            }
        }

        private bool CanPause()
        {
            if (_isTransitionRequested || _input.IsCutsceneMode) return false;

            return _controller == null || !_controller.IsFinished;
        }

        private void Pause()
        {
            _cursorLockBeforePause = Cursor.lockState;
            _cursorVisibleBeforePause = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Time.timeScale = 0f;
            AudioListener.pause = true;
            _input.SetPaused(true);

            _screen.Value = PauseScreen.Menu;
        }

        private void Resume()
        {
            if (_screen.Value != PauseScreen.Menu) return;

            _screen.Value = PauseScreen.None;

            Time.timeScale = 1f;
            AudioListener.pause = false;
            _input.SetPaused(false);

            Cursor.lockState = _cursorLockBeforePause;
            Cursor.visible = _cursorVisibleBeforePause;
        }

        private void GoToTitle()
        {
            if (_isTransitionRequested) return;

            _isTransitionRequested = true;
            _view.SetInteractable(false);

            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneLoader.LoadTitle();
        }

        private void ShowScreen(PauseScreen previous, PauseScreen current)
        {
            switch (current)
            {
                case PauseScreen.None:
                    _view.Hide();
                    break;
                case PauseScreen.Menu:
                    _view.Show(previous == PauseScreen.Settings);
                    break;
                case PauseScreen.Settings:
                    _view.Hide();
                    _settings.Open();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_input != null) _input.PausePressed -= OnPausePressed;

            if (_screen.Value != PauseScreen.None)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }

            _disposables.Dispose();
            _screen.Dispose();
        }
    }
}
