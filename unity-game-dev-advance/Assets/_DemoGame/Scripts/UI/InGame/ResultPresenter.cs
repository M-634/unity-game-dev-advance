using System;
using R3;
using UnityEngine;

namespace DemoGame
{
    public class ResultPresenter : MonoBehaviour
    {
        [SerializeField]
        private ResultView _view;

        [SerializeField]
        private InGameController _controller;

        private readonly CompositeDisposable _disposables = new();
        private bool _isTransitionRequested;

        private void Start()
        {
            if (_view == null || _controller == null)
            {
                Debug.LogError($"{nameof(ResultPresenter)}: 参照が設定されていません。", this);
                return;
            }

            _view.Hide();

            _controller.Finished
                .Subscribe(result =>
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    _view.Show(result.IsCleared, result.ElapsedTime);
                })
                .AddTo(_disposables);

            _view.RetryClicked.Subscribe(_ => RequestTransition(SceneLoader.ReloadCurrent)).AddTo(_disposables);
            _view.TitleClicked.Subscribe(_ => RequestTransition(SceneLoader.LoadTitle)).AddTo(_disposables);
        }

        private void RequestTransition(Action load)
        {
            if (_isTransitionRequested || !_controller.IsFinished) return;

            _isTransitionRequested = true;
            _view.SetInteractable(false);
            load();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
