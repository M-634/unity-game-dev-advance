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

        private void Start()
        {
            if (_view == null || _controller == null)
            {
                Debug.LogError($"{nameof(ResultPresenter)}: 参照が設定されていません。", this);
                return;
            }

            _view.Hide();

            _controller.Finished
                .Subscribe(result => _view.Show(result.IsCleared, result.ElapsedTime))
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
