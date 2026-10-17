using R3;
using UnityEngine;

namespace DemoGame
{
    public class TitlePresenter : MonoBehaviour
    {
        [SerializeField]
        private TitleView _view;

        [SerializeField]
        private SettingsPresenter _settings;

        private readonly CompositeDisposable _disposables = new();
        private TitleModel _model;

        private void Start()
        {
            if (_view == null || _settings == null)
            {
                Debug.LogError($"{nameof(TitlePresenter)}: 参照が設定されていません。", this);
                return;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _model = new TitleModel();

            _view.StartClicked.Subscribe(_ => _model.RequestStart()).AddTo(_disposables);
            _view.SettingsClicked.Subscribe(_ => _model.OpenSettings()).AddTo(_disposables);
            _view.QuitClicked.Subscribe(_ => _model.RequestQuit()).AddTo(_disposables);
            _settings.Closed.Subscribe(_ => _model.CloseSettings()).AddTo(_disposables);

            _model.Screen.Subscribe(ShowScreen).AddTo(_disposables);

            _model.IsStartRequested.Where(x => x).Subscribe(_ =>
            {
                _view.SetInteractable(false);
                SceneLoader.LoadInGame();
            }).AddTo(_disposables);

            _model.IsQuitRequested.Where(x => x).Subscribe(_ =>
            {
                _view.SetInteractable(false);
                Quit();
            }).AddTo(_disposables);
        }

        private void ShowScreen(TitleScreen screen)
        {
            switch (screen)
            {
                case TitleScreen.Menu:
                    _view.Show();
                    break;
                case TitleScreen.Settings:
                    _view.Hide();
                    _settings.Open();
                    break;
            }
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _model?.Dispose();
        }
    }
}
