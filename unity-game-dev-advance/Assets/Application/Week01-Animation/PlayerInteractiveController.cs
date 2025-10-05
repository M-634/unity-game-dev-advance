using JetBrains.Annotations;
using Unity_Game_Dev_Advanced;
using UnityEngine;

namespace Week01
{
    [RequireComponent(typeof(Collider))]
    public class PlayerInteractiveController : MonoBehaviour
    {
        [SerializeField]
        private StarterAssetsInputsRapper _input;
        
        [SerializeField]
        private Transform _playerTransform;
        
        [SerializeField]
        private Collider _collider;
        
        public bool _isInteracting;
        
        [CanBeNull]
        private IInteractiveObject _currentInteractiveObject;

        
        private void Start()
        {
            if (_playerTransform == null)
            {
                _playerTransform = transform;
            }
            
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
            }
            _collider.isTrigger = true;
        }

        public void Update()
        {
            if (_input.interactive)
            {
                _input.interactive = false;
                
                if(!_isInteracting) return;
                
                if (_currentInteractiveObject != null)
                {
                   _currentInteractiveObject.OnExecute(_playerTransform);   
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IInteractiveObject interactiveObject))
            {
               _currentInteractiveObject = interactiveObject;   
               _isInteracting = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IInteractiveObject interactiveObject))
            {
                if (_currentInteractiveObject == null ||
                    interactiveObject.GetObjectID() != _currentInteractiveObject.GetObjectID())
                {
                    return;
                }
                
                _currentInteractiveObject.OnExit(_playerTransform);
                _currentInteractiveObject = null;
               _isInteracting = false;  
            }
        }
    }
}