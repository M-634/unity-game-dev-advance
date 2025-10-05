using System;
using Cysharp.Threading.Tasks;
using Unity_Game_Dev_Advanced;
using UnityEngine;

namespace Week01
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAttack : MonoBehaviour
    {
        private static readonly int AttackAnimatorHash = Animator.StringToHash("Attack");

        [SerializeField]
        private StarterAssetsInputsRapper _input;

        [SerializeField]
        private Animator _animator;
        
        [SerializeField]
        private GameObject _idleSword;
        
        [SerializeField]
        private GameObject _attackingSword;

        [SerializeField, Range(0f, 5f)]
        private float _waitTimeForChangingAttackToIdleState;

        [SerializeField]
        private GameObject _hitCheckerObject;
        
        [SerializeField, Range(0f, 5f)]
        private float _showCollisionObjectTime = 0.01f;

        private bool _isAttacking = false;

        private void Start()
        {
            if (_input == null)
            {
                _input = GetComponent<StarterAssetsInputsRapper>();
            }
            
            SetActiveChangeSword(false);
            _hitCheckerObject.SetActive(false);
        }

        private void SetActiveChangeSword(bool isAttacking)
        {
            _idleSword.SetActive(!isAttacking);
            _attackingSword.SetActive(isAttacking);
            _isAttacking = isAttacking;
        }

        private void Update()
        {
            if (_input.attack)
            {
                Attack().Forget();
            }
        }

        private async UniTask Attack()
        {
            if (_isAttacking) return;

            SetActiveChangeSword(true);
            
            _animator.SetTrigger(AttackAnimatorHash);

            await UniTask.Delay(TimeSpan.FromSeconds(_waitTimeForChangingAttackToIdleState), cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            
            SetActiveChangeSword(false);
            _input.attack = false;
        }

        /// <summary>
        /// Invoke Animation Event 
        /// </summary>
        public void ShowAttackCollisionObject()
        {
            if (!_isAttacking)
            {
                throw new InvalidOperationException("not attacking state");
            }
            
            if (_hitCheckerObject != null)
            {
                ShowCollisionAsync().Forget();
            }
        }

        private async UniTask ShowCollisionAsync()
        {
            _hitCheckerObject.SetActive(true);
            
            await UniTask.Delay(TimeSpan.FromSeconds(_showCollisionObjectTime), cancellationToken: gameObject.GetCancellationTokenOnDestroy());
            
            _hitCheckerObject.SetActive(false);
        }
    }
}
