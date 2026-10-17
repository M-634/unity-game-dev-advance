using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    public class AreaGate : MonoBehaviour
    {
        [SerializeField]
        private GameProgress _progress;

        [Tooltip("この数の灯火が点いたら開く")]
        [SerializeField, Range(1, GameProgress.RequiredLitCount)]
        private int _requiredLitCount = 1;

        [SerializeField]
        private SlidingDoor _door;

        [Tooltip("扉が無いときに消すもの")]
        [SerializeField]
        private GameObject _blocker;

        [SerializeField, Range(0.1f, 5f)]
        private float _dissolveSeconds = 1.5f;

        private readonly CompositeDisposable _disposables = new();

        public bool IsOpen { get; private set; }

        private void Start()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }

            if (_progress == null)
            {
                Debug.LogError($"{nameof(AreaGate)}: GameProgress が見つかりません。", this);
                return;
            }

            _progress.LitCount
                .Where(count => count >= _requiredLitCount)
                .Take(1)
                .Subscribe(_ => OpenAsync(destroyCancellationToken).SuppressCancellationThrow().Forget())
                .AddTo(_disposables);
        }

        private async UniTask OpenAsync(CancellationToken ct)
        {
            if (IsOpen) return;

            IsOpen = true;

            if (_door != null)
            {
                await _door.OpenAsync(ct);
                return;
            }

            if (_blocker == null) return;

            foreach (var col in _blocker.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            var renderers = _blocker.GetComponentsInChildren<Renderer>();
            float elapsed = 0f;
            while (elapsed < _dissolveSeconds)
            {
                elapsed += Time.deltaTime;
                DissolveBehaviour.SetDissolve(renderers, Mathf.Clamp01(elapsed / _dissolveSeconds));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            _blocker.SetActive(false);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
