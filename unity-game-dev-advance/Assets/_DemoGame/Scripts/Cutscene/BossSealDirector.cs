using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DemoGame
{
    public class BossSealDirector : MonoBehaviour
    {
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        [SerializeField]
        private CutscenePlayer _cutscenePlayer;

        [SerializeField]
        private PlayableDirector _director;

        [SerializeField]
        private BossDoorSeal _seal;

        [SerializeField]
        private BossDoor _door;

        [Tooltip("最後の紋様のあと、扉が開ききるまで映しておくカメラ（演出の Timeline で使っているもの）")]
        [SerializeField]
        private CinemachineVirtualCameraBase _holdCamera;

        [SerializeField]
        private int _holdPriority = 100;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_seal == null)
            {
                Debug.LogError($"{nameof(BossSealDirector)}: BossDoorSeal が設定されていません。", this);
                return;
            }

            _seal.BreakRequested
                .Subscribe(emblem => PlayAsync(emblem, destroyCancellationToken).SuppressCancellationThrow().Forget())
                .AddTo(_disposables);
        }

        private async UniTask PlayAsync(BossDoorSeal.Emblem emblem, CancellationToken token)
        {
            bool isLast = _seal.IsLastRemaining(emblem);

            using var scope = isLast && _cutscenePlayer != null ? _cutscenePlayer.Lock() : null;

            if (_cutscenePlayer != null && _director != null)
            {
                BindEmblem(emblem.Renderer != null ? emblem.Renderer.gameObject : null);
                await _cutscenePlayer.PlayAsync(_director, skippable: false, token);
            }

            HideEmblem(emblem.Renderer);
            _seal.MarkBroken(emblem);

            if (!isLast || _door == null) return;

            if (_holdCamera != null) _holdCamera.Priority = _holdPriority;

            try
            {
                await UniTask.WaitUntil(() => _door.IsFullyOpen, cancellationToken: token);
                await UniTask.Delay(500, cancellationToken: token);
            }
            finally
            {
                if (_holdCamera != null) _holdCamera.Priority = 0;
            }
        }

        private void BindEmblem(GameObject emblem)
        {
            if (!(_director.playableAsset is TimelineAsset timeline)) return;

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is DissolveTrack)
                {
                    _director.SetGenericBinding(track, emblem);
                }
            }
        }

        private static void HideEmblem(Renderer emblem)
        {
            if (emblem == null) return;

            emblem.material.SetFloat(DissolveId, 1f);
            emblem.enabled = false;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
