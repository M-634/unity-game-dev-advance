using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Week01
{
    [RequireComponent(typeof(Rigidbody))]
    public class PressBox : MonoBehaviour, IInteractiveObject
    {
        [SerializeField] 
        private Transform _rightHandTargetTransform;
        
        [SerializeField] 
        private Transform _leftHandTargetTransform;
        
        [SerializeField] 
        private Rigidbody _rigidbody;
 
        private bool _isMoving;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public int GetObjectID()
        {
            return gameObject.GetInstanceID();
        }

        public void OnExecute(Transform player)
        {
            if (_isMoving) return;
            _isMoving = true;


            if (player.TryGetComponent(out IKControl ikControl))
            {
                ikControl.SetTarget(_rightHandTargetTransform, _leftHandTargetTransform);
                ikControl.SetActive(true);
            }
            
            OnPressAsync(player, 5f).Forget();   
        }

        public void OnExit(Transform player)
        {
            if (player.TryGetComponent(out IKControl ikControl))
            {
                ikControl.SetTarget();
                ikControl.SetActive(false);
            }
        }
        
        private async UniTask OnPressAsync(Transform playerTransform, float power)
        {
            // var vel = playerTransform.forward * power;
            // _rigidbody.AddForce(vel, ForceMode.Impulse);
            //
            // await UniTask.WaitUntil(() => _rigidbody.linearVelocity.magnitude >= 0.01f);
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            _isMoving = false;
        }

    }
}