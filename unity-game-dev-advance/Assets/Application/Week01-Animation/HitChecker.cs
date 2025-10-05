using UnityEngine;

namespace Week01
{
    [RequireComponent(typeof(Collider))]
    public class HitChecker : MonoBehaviour
    {
        [SerializeField]
        private Collider _collider;
        
        private void Start()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
            }
            _collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IDamageableObject damageableObject))
            {
                damageableObject.OnDamage(new DamageDataContext(10));
            }
        }
    }
}