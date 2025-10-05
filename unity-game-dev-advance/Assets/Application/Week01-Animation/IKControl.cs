using UnityEngine;

namespace Week01
{
    [RequireComponent(typeof(Animator))]
    public class IKControl : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        private Transform _targetRightHandTransform;
        private Transform _targetLeftHandTransform;
        private Transform _targetLookTransform;
        
        private bool _isActive;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetTarget(Transform targetRightHandTransform = null, Transform targetLeftHandTransform = null,
            Transform targetLookTransform = null)
        {
            _targetRightHandTransform = targetRightHandTransform;
            _targetLeftHandTransform = targetLeftHandTransform;
            _targetLookTransform = targetLookTransform;
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            if (_animator == null) return;

            if (!_isActive)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                _animator.SetLookAtWeight(0);
                return;
            }

            if (_targetRightHandTransform != null)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                _animator.SetIKPosition(AvatarIKGoal.RightHand, _targetRightHandTransform.position);
                _animator.SetIKRotation(AvatarIKGoal.RightHand, _targetRightHandTransform.rotation);
            }

            if (_targetLeftHandTransform != null)
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                _animator.SetIKPosition(AvatarIKGoal.LeftHand, _targetLeftHandTransform.position);
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, _targetLeftHandTransform.rotation);
            }

            if (_targetLookTransform != null)
            {
                _animator.SetLookAtWeight(1);
                _animator.SetLookAtPosition(_targetLookTransform.position);
            }
        }
    }
}