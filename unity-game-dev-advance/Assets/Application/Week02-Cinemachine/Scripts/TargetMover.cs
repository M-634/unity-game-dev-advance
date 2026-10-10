using UnityEngine;

namespace Week02
{
    public class TargetMover : MonoBehaviour
    {
        [Tooltip("往復する方向（ワールド空間）")]
        [SerializeField]
        private Vector3 _direction = Vector3.right;

        [Tooltip("往復する幅（m）")]
        [SerializeField, Min(0f)]
        private float _travel = 10f;

        [Tooltip("移動の速さ（m/s）")]
        [SerializeField, Min(0f)]
        private float _speed = 4f;

        private Vector3 _origin;
        private float _moved;

        private void Awake()
        {
            _origin = transform.position;
        }

        private void Update()
        {
            _moved += _speed * Time.deltaTime;

            float offset = Mathf.PingPong(_moved, _travel) - _travel * 0.5f;
            transform.position = _origin + _direction.normalized * offset;
        }
    }
}
