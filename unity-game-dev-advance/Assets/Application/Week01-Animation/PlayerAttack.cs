using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Week01
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAttack : MonoBehaviour
    {
        private static readonly int AttackAnimatorHash = Animator.StringToHash("Attack");

        [SerializeField]
        private Animator _animator;
        
        [SerializeField]
        private GameObject _idleSword;
        
        [SerializeField]
        private GameObject _attackingSword;

        [SerializeField, Range(0f, 5f)]
        private float _waitTimeForChangingAttackToIdleState;

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
                Attack().Forget();
            }
        }
#endif

        private async UniTask Attack()
        {
            if (_isAttacking) return;

            SetActiveChangeSword(true);
            
            _animator.SetTrigger(AttackAnimatorHash);

            await UniTask.Delay(TimeSpan.FromSeconds(_waitTimeForChangingAttackToIdleState), cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            
            SetActiveChangeSword(false);
        }
    }
}
