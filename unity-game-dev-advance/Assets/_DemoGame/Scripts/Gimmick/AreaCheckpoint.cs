using UnityEngine;

namespace DemoGame
{
    [RequireComponent(typeof(Collider))]
    public class AreaCheckpoint : MonoBehaviour
    {
        [SerializeField]
        private GameProgress _progress;

        [SerializeField]
        private AreaType _area;

        [Tooltip("再開する位置と向き。未設定ならこのオブジェクトの位置")]
        [SerializeField]
        private Transform _respawnPoint;

        public AreaType Area => _area;

        public Transform RespawnPoint => _respawnPoint != null ? _respawnPoint : transform;

        private void Awake()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_progress == null || !other.CompareTag("Player")) return;

            _progress.EnterArea(_area);
        }

        private void OnDrawGizmos()
        {
            Transform point = RespawnPoint;
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.8f);
            Gizmos.DrawWireSphere(point.position + Vector3.up, 0.5f);
            Gizmos.DrawLine(point.position + Vector3.up, point.position + Vector3.up + point.forward * 1.5f);
        }
    }
}
