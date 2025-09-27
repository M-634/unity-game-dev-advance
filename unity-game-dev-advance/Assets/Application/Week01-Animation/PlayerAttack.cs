using UnityEngine;
using UnityEngine.InputSystem;

namespace Week01
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField]
        private GameObject _idleSword;
        
        [SerializeField]
        private GameObject _attackingSword;

        private bool _isAttacking = false;

        private void Start()
        {
            SetActiveChangeSword(false);
        }

        private void SetActiveChangeSword(bool isAttacking)
        {
            _idleSword.SetActive(!isAttacking);
            _attackingSword.SetActive(isAttacking);
            _isAttacking = isAttacking;
        }

#if ENABLE_INPUT_SYSTEM
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                Attack();
            }
        }
#endif

        private void Attack()
        {
            if (_isAttacking) return;
            
            SetActiveChangeSword(true);
        }
    }
}
