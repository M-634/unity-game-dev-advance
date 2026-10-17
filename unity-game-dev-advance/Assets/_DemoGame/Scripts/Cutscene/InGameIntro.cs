using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace DemoGame
{
    public class InGameIntro : MonoBehaviour
    {
        [SerializeField]
        private CutscenePlayer _cutscenePlayer;

        [SerializeField]
        private PlayableDirector _director;

        [SerializeField]
        private InGameController _inGameController;

        [Tooltip("導入演出で拾う杖。演出が終わったら拾ったことにする")]
        [SerializeField]
        private StaffPickup _staffPickup;

        [Header("祭壇の火")]
        [Tooltip("導入演出のあとに火が上がる篝火")]
        [SerializeField]
        private Bonfire _altar;

        [Tooltip("祭壇を映すカメラの流れ。無ければその場で火が上がるだけ")]
        [SerializeField]
        private CameraShowcase _altarShowcase;

        [Tooltip("祭壇のカメラに切り替えてから火が上がるまでの時間（秒）")]
        [SerializeField, Range(0f, 5f)]
        private float _igniteDelay = 1.2f;

        private void Start()
        {
            PlayAsync().Forget();
        }

        private async UniTaskVoid PlayAsync()
        {
            var token = destroyCancellationToken;

            if (_cutscenePlayer != null && _director != null)
            {
                bool canceled = await _cutscenePlayer.PlayAsync(_director, skippable: true, token).SuppressCancellationThrow();
                if (canceled) return;
            }

            if (_staffPickup != null)
            {
                _staffPickup.PickUp();
            }

            bool igniteCanceled = await IgniteAltarAsync(token).SuppressCancellationThrow();
            if (igniteCanceled) return;

            if (_inGameController != null)
            {
                _inGameController.BeginTimer();
            }
        }

        private async UniTask IgniteAltarAsync(CancellationToken token)
        {
            if (_altar == null) return;

            if (_altarShowcase != null)
            {
                IgniteLaterAsync(token).SuppressCancellationThrow().Forget();
                await _altarShowcase.PlayAsync(token);
            }

            IgniteAltar();
        }

        private async UniTask IgniteLaterAsync(CancellationToken token)
        {
            await UniTask.Delay((int)(_igniteDelay * 1000f), cancellationToken: token);
            IgniteAltar();
        }

        private void IgniteAltar()
        {
            if (_altar == null || _altar.IsBurning) return;

            _altar.Reignite();
        }
    }
}
