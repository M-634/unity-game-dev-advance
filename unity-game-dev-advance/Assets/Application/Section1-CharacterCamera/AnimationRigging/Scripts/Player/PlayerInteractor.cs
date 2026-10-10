using R3;
using UnityEngine;

namespace Section1.AnimationRigging
{
    // 近くの IInteractable を選び、E キーで働きかける
    // トリガーの出入りは使わず、一定間隔で探し直す（取りこぼしで「選ばれたまま」にならないように）
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField]
        private InputReader _input;

        [Tooltip("探す範囲の基準になる向き。プレイヤーの Visual を指定する")]
        [SerializeField]
        private Transform _origin;

        [SerializeField, Range(0f, 3f)]
        private float _forwardOffset = 0.8f;

        [SerializeField, Range(0.1f, 3f)]
        private float _radius = 1f;

        [SerializeField, Range(0.02f, 1f)]
        private float _scanInterval = 0.1f;

        [Tooltip("探す対象のレイヤー（Interactable）")]
        [SerializeField]
        private LayerMask _layers = ~0;

        private readonly ReactiveProperty<IInteractable> _current = new(null);
        private readonly Collider[] _buffer = new Collider[16];
        private float _nextScanTime;

        // 箱を押している間など、選んでいる対象を固定したいときに使う
        private IInteractable _held;

        public ReadOnlyReactiveProperty<IInteractable> Current => _current;

        private void Awake()
        {
            if (_origin == null)
            {
                _origin = transform;
            }
        }

        private void OnEnable()
        {
            if (_input == null)
            {
                Debug.LogError($"{nameof(PlayerInteractor)}: InputReader が設定されていません。", this);
                return;
            }

            _input.Interacted += OnInteracted;
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.Interacted -= OnInteracted;
            }

            _current.Value = null;
        }

        private void Update()
        {
            if (Time.time < _nextScanTime) return;

            _nextScanTime = Time.time + _scanInterval;
            _current.Value = _held ?? FindNearest();
        }

        public void Hold(IInteractable target)
        {
            _held = target;
            _current.Value = target;
        }

        public void Release()
        {
            _held = null;
            _nextScanTime = 0f;
        }

        private IInteractable FindNearest()
        {
            Vector3 center = _origin.position + Vector3.up * 0.9f + _origin.forward * _forwardOffset;
            int count = Physics.OverlapSphereNonAlloc(center, _radius, _buffer, _layers, QueryTriggerInteraction.Collide);

            IInteractable nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var interactable = _buffer[i].GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(this)) continue;

                float distance = (_buffer[i].ClosestPoint(center) - center).sqrMagnitude;
                if (distance >= nearestDistance) continue;

                nearest = interactable;
                nearestDistance = distance;
            }

            return nearest;
        }

        private void OnInteracted()
        {
            var target = _current.Value;

            // 破棄された対象（UnityEngine.Object の == null）を弾く
            if (target == null || (target is Object unityObject && unityObject == null)) return;
            if (!target.CanInteract(this)) return;

            target.Interact(this);
            _nextScanTime = 0f;

            // 対象は同じでも文言が変わることがある（押す → 離す）ので、HUD に知らせ直す
            _current.ForceNotify();
        }

        private void OnDestroy()
        {
            _current.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            var origin = _origin != null ? _origin : transform;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(origin.position + Vector3.up * 0.9f + origin.forward * _forwardOffset, _radius);
        }
    }
}
