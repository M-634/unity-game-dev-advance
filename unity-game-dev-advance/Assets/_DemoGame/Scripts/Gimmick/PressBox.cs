using UnityEngine;

namespace DemoGame
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public class PressBox : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private BoxCollider _collider;

        [Tooltip("動かすときに、ぶつかる相手として調べるレイヤー")]
        [SerializeField]
        private LayerMask _blockingLayers = ~0;

        [Tooltip("壁との間に残す隙間")]
        [SerializeField, Range(0.001f, 0.1f)]
        private float _skin = 0.02f;

        private readonly RaycastHit[] _hits = new RaycastHit[8];
        private PlayerBoxPusher _pusher;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        public string Prompt => _pusher != null ? "Release" : "Push";

        public bool IsBeingPushed => _pusher != null;

        private void Awake()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
            if (_collider == null) _collider = GetComponent<BoxCollider>();

            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (!interactor.TryGetComponent(out PlayerBoxPusher pusher)) return false;

            if (_pusher != null) return _pusher == pusher;

            return interactor.transform.position.y < _collider.bounds.max.y - 0.1f;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!interactor.TryGetComponent(out PlayerBoxPusher pusher)) return;

            if (_pusher == pusher)
            {
                pusher.End();
                return;
            }

            pusher.Begin(this, interactor);
        }

        internal void SetPusher(PlayerBoxPusher pusher)
        {
            _pusher = pusher;
        }

        public void GetPushFace(Vector3 playerPosition, out Vector3 faceCenter, out Vector3 normal, out float halfWidth)
        {
            Vector3 local = transform.InverseTransformPoint(playerPosition);
            Vector3 half = Vector3.Scale(_collider.size, transform.lossyScale) * 0.5f;
            Vector3 center = _collider.bounds.center;

            bool useX = Mathf.Abs(local.x / _collider.size.x) > Mathf.Abs(local.z / _collider.size.z);

            if (useX)
            {
                normal = transform.right * Mathf.Sign(local.x);
                faceCenter = center + normal * half.x;
                halfWidth = half.z;
            }
            else
            {
                normal = transform.forward * Mathf.Sign(local.z);
                faceCenter = center + normal * half.z;
                halfWidth = half.x;
            }

            faceCenter.y = _collider.bounds.min.y;
        }

        public float Move(Vector3 direction, float distance)
        {
            if (distance <= 0f) return 0f;

            var bounds = _collider.bounds;

            Vector3 halfExtents = bounds.extents - new Vector3(0.01f, 0.05f, 0.01f);
            Vector3 center = bounds.center + Vector3.up * 0.05f;

            int count = Physics.BoxCastNonAlloc(center, halfExtents, direction, _hits, transform.rotation,
                distance + _skin, _blockingLayers, QueryTriggerInteraction.Ignore);

            float allowed = distance;

            for (int i = 0; i < count; i++)
            {
                var hit = _hits[i];
                if (hit.collider == _collider) continue;
                if (hit.collider.CompareTag("Player")) continue;

                allowed = Mathf.Min(allowed, Mathf.Max(0f, hit.distance - _skin));
            }

            if (allowed <= 0f) return 0f;

            transform.position += direction * allowed;
            return allowed;
        }

        public void ResetPosition()
        {
            if (_pusher != null)
            {
                _pusher.End();
            }

            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
        }
    }
}
