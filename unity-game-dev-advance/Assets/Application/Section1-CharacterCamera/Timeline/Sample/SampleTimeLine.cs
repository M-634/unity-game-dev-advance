using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Section.One.Timeline
{
    public class SampleTimeLine : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        
        private void Start()
        {
            _director.stopped += OnStopped;
            _director.Play();
            
            
            
            // SampleUniTaskByExtensions().Forget();
        }
        
        private static void OnStopped(PlayableDirector obj) => Debug.Log($"{obj.playableAsset.name} is stopped");

        private async UniTaskVoid SampleUniTaskByExtensions()
        {
            await _director.PlayAsync(this.GetCancellationTokenOnDestroy());
            Debug.Log("finished");
        }
    }

    public static class TimelineExtensions
    {
        public static UniTask PlayAsync(this PlayableDirector director, CancellationToken cancellationToken)
        {
            var tcs = new UniTaskCompletionSource();

            director.stopped += OnStopped;
            director.Play();
            
            cancellationToken.Register(() =>
            {
                director.stopped -= OnStopped;
                tcs.TrySetCanceled(cancellationToken);
            });

            return tcs.Task;

            void OnStopped(PlayableDirector obj)
            {
                director.stopped -= OnStopped;
                tcs.TrySetResult();
            }
        }
    }
}
