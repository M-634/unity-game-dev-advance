using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    public class PlayerRespawner : MonoBehaviour
    {
        [SerializeField]
        private PlayerHealth _health;

        [SerializeField]
        private PlayerController _controller;

        [SerializeField]
        private CharacterController _characterController;

        [SerializeField]
        private InputReader _input;

        [SerializeField]
        private GameProgress _progress;

        [SerializeField]
        private InGameController _inGameController;

        [Tooltip("エリアごとの再開地点。空ならシーンから探す。目覚めの地は最初の位置から再開する")]
        [SerializeField]
        private AreaCheckpoint[] _checkpoints;

        [Header("演出")]
        [Tooltip("倒れてから再開するまでの時間（秒）")]
        [SerializeField, Range(0f, 10f)]
        private float _respawnDelay = 2.5f;

        [Tooltip("画面を暗くする CanvasGroup。無ければ暗転しない")]
        [SerializeField]
        private CanvasGroup _fade;

        [SerializeField, Range(0.05f, 3f)]
        private float _fadeSeconds = 0.5f;

        private readonly CompositeDisposable _disposables = new();
        private bool _isRespawning;
        private Vector3 _startPosition;
        private float _startYaw;

        private void Start()
        {
            if (_health == null || _controller == null)
            {
                Debug.LogError($"{nameof(PlayerRespawner)}: 参照が設定されていません。", this);
                return;
            }

            if (_characterController == null)
            {
                _characterController = _controller.GetComponent<CharacterController>();
            }

            if (_checkpoints == null || _checkpoints.Length == 0)
            {
                _checkpoints = FindObjectsByType<AreaCheckpoint>(FindObjectsSortMode.None);
            }

            _startPosition = _controller.transform.position;
            _startYaw = _controller.VisualRoot.eulerAngles.y;

            SetFade(0f);

            _health.Died
                .Subscribe(_ => RespawnAsync(destroyCancellationToken).SuppressCancellationThrow().Forget())
                .AddTo(_disposables);
        }

        private async UniTask RespawnAsync(CancellationToken ct)
        {
            if (_isRespawning || IsFinished) return;

            _isRespawning = true;
            _controller.IsMovementLocked = true;
            if (_input != null) _input.SetInputEnabled(false);

            float waitBeforeFade = Mathf.Max(0f, _respawnDelay - _fadeSeconds);
            await UniTask.Delay((int)(waitBeforeFade * 1000f), cancellationToken: ct);
            await FadeAsync(0f, 1f, ct);

            if (IsFinished)
            {
                _isRespawning = false;
                return;
            }

            AreaCheckpoint checkpoint = FindCheckpoint();
            if (checkpoint != null)
            {
                Teleport(checkpoint.RespawnPoint.position, checkpoint.RespawnPoint.eulerAngles.y);
            }
            else
            {
                Teleport(_startPosition, _startYaw);
            }
            _health.Revive();

            await FadeAsync(1f, 0f, ct);

            _controller.IsMovementLocked = false;
            if (_input != null && !IsFinished) _input.SetInputEnabled(true);
            _isRespawning = false;
        }

        private bool IsFinished => _inGameController != null && _inGameController.IsFinished;

        private AreaCheckpoint FindCheckpoint()
        {
            AreaType area = _progress != null ? _progress.CurrentArea.CurrentValue : AreaType.Awakening;
            AreaCheckpoint best = null;

            foreach (var checkpoint in _checkpoints)
            {
                if (checkpoint == null || checkpoint.Area > area) continue;
                if (best != null && checkpoint.Area <= best.Area) continue;

                best = checkpoint;
            }

            return best;
        }

        private void Teleport(Vector3 position, float yaw)
        {
            Transform target = _controller.transform;
            Vector3 delta = position - target.position;

            if (_characterController != null) _characterController.enabled = false;
            target.position = position;
            _controller.VisualRoot.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (_characterController != null) _characterController.enabled = true;

            Physics.SyncTransforms();

            int count = CinemachineCore.Instance.VirtualCameraCount;
            for (int i = 0; i < count; i++)
            {
                CinemachineVirtualCameraBase vcam = CinemachineCore.Instance.GetVirtualCamera(i);
                Transform follow = vcam.Follow;
                if (follow == null || !follow.IsChildOf(target)) continue;

                vcam.OnTargetObjectWarped(follow, delta);
            }
        }

        private async UniTask FadeAsync(float from, float to, CancellationToken ct)
        {
            if (_fade == null) return;

            float elapsed = 0f;
            while (elapsed < _fadeSeconds)
            {
                elapsed += Time.deltaTime;
                SetFade(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / _fadeSeconds)));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            SetFade(to);
        }

        private void SetFade(float alpha)
        {
            if (_fade == null) return;

            _fade.alpha = alpha;
            _fade.blocksRaycasts = alpha > 0f;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
