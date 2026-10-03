using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace DemoGame
{
    // InGame に入った直後の導入演出。終わったら（スキップでも）経過時間を数え始める
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

            if (_inGameController != null)
            {
                _inGameController.BeginTimer();
            }
        }
    }
}
