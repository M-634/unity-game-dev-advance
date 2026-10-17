using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    public class CameraShowcase : MonoBehaviour
    {
        [Serializable]
        public class Shot
        {
            public CinemachineVirtualCameraBase Camera;

            [Tooltip("このカメラに切り替えてから次へ進むまでの時間（秒）。ブレンドの時間を含む")]
            [Range(0.1f, 10f)]
            public float Seconds = 1.5f;
        }

        [SerializeField]
        private CutscenePlayer _cutscenePlayer;

        [SerializeField]
        private InputReader _input;

        [SerializeField]
        private Shot[] _shots = new Shot[0];

        [SerializeField]
        private int _livePriority = 100;

        [SerializeField]
        private int _standbyPriority = 0;

        [SerializeField]
        private bool _skippable = true;

        private readonly Subject<int> _shotStarted = new();

        public Observable<int> ShotStarted => _shotStarted;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            SetLive(-1);
        }

        public async UniTask PlayAsync(CancellationToken ct)
        {
            if (IsPlaying || _shots.Length == 0) return;

            IsPlaying = true;
            using var scope = _cutscenePlayer != null ? _cutscenePlayer.Lock() : null;

            bool skipRequested = false;
            void OnSkip() => skipRequested = true;
            if (_skippable && _input != null) _input.SkipPressed += OnSkip;

            try
            {
                for (int i = 0; i < _shots.Length; i++)
                {
                    SetLive(i);
                    _shotStarted.OnNext(i);

                    float end = Time.time + _shots[i].Seconds;
                    while (Time.time < end && !skipRequested)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, ct);
                    }

                    if (skipRequested) break;
                }
            }
            finally
            {
                if (_input != null) _input.SkipPressed -= OnSkip;

                SetLive(-1);
                IsPlaying = false;
            }
        }

        private void SetLive(int index)
        {
            for (int i = 0; i < _shots.Length; i++)
            {
                if (_shots[i]?.Camera == null) continue;

                _shots[i].Camera.Priority = i == index ? _livePriority : _standbyPriority;
            }
        }

        private void OnDestroy()
        {
            _shotStarted.Dispose();
        }
    }
}
