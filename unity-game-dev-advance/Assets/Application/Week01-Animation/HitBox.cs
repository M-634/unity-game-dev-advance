using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Week01
{
    [RequireComponent(typeof(BoxCollider))]
    public class HitBox : MonoBehaviour, IDamageableObject
    {
        [SerializeField] 
        private Material _material;
        
        [SerializeField]
        private Color _onHitColor;

        [SerializeField, Range(0f, 1f)]
        private float _delayChangeColorTime = 0.1f;
        
        public bool IsDamaging { get; private set; }
        
        private void Start()
        {
            if (_material == null)
            {
                _material = GetComponent<Renderer>().material;
            }
            IsDamaging = false;
        }

        public void OnDamage(DamageDataContext damageDataContext)
        {
            if (IsDamaging)
            {
                return;
            }
            Debug.Log($"Damage Amount : {damageDataContext.DamageAmount}");
            OnDamagedAsync().Forget();
        }

        private async UniTask OnDamagedAsync()
        {
            IsDamaging = true;
            
            var currentMaterialColor = _material.color;
            _material.color = _onHitColor;
            await UniTask.Delay(TimeSpan.FromSeconds(_delayChangeColorTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            _material.color = currentMaterialColor;
            
            IsDamaging = false;
        }
    }
}
