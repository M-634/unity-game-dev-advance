using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DemoGame
{
    [RequireComponent(typeof(Collider))]
    public class SealOverlookTrigger : MonoBehaviour
    {
        [SerializeField]
        private GameProgress _progress;

        [Tooltip("封印の前で振り返って、灯火の光を順に映す")]
        [SerializeField]
        private CameraShowcase _showcase;

        private bool _isPlayed;

        private void Awake()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isPlayed || _showcase == null || !other.CompareTag("Player")) return;
            if (_progress != null && _progress.IsAllLit) return;

            _isPlayed = true;
            _showcase.PlayAsync(destroyCancellationToken).SuppressCancellationThrow().Forget();
        }
    }
}
