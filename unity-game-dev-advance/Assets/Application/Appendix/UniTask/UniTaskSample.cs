using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Appendix
{
    public class UniTaskSample : MonoBehaviour
    {
        [Tooltip("押すと実行中の処理をキャンセルする")]
        [SerializeField]
        private Button _cancelButton;

        [SerializeField]
        private GameObject _cube;

        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(Cancel);
            }

            // 試したいものだけコメントを外して使う
            // BasicWaitAsync().Forget();
            // WaitUntilAsync().Forget();
            LoopAsync().Forget();
        }

        private async UniTask BasicWaitAsync()
        {
            // 1 フレーム待つ
            await UniTask.Yield();
            Debug.Log("1 フレーム経過");

            // 10 秒待つ
            await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: _cancellationTokenSource.Token);
            Debug.Log("10 秒経過");
        }

        private async UniTask WaitUntilAsync()
        {
            Debug.Log("Cube が表示されるまで待機します");

            await UniTask.WaitUntil(
                () => _cube != null && _cube.activeSelf,
                cancellationToken: _cancellationTokenSource.Token,
                cancelImmediately: true);

            Debug.Log("Cube が表示されました");
        }

        private async UniTask LoopAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _cancellationTokenSource.Token);
                Debug.Log("1 秒経過");
            }

            Debug.Log("キャンセルされたので止まりました");
        }

        private void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void OnDestroy()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(Cancel);
            }

            // 始めた処理を、オブジェクトが消えるときに必ず止める
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
