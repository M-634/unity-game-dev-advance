using Section.Common;
using UnityEngine;

namespace Section2_AIState.CSharpSample
{
    public class SampleUsage : MonoBehaviour
    {
        [SerializeField] IDamageable _damageableField; // interfaceはデフォルトで[SerializeFiled]属性を付けられない。
        
        
        public void Foo(IDamageable damageable)
        {
            　// damageableの実体が、UnityEngine.Object(MonoBehaviourを継承したクラス)だった場合 fake nullでの判定ができないので注意
             damageable?.TakeDamage(10);
            
             if (damageable != null) // if(damageable is not null)と記載もできる
             {
                 damageable.TakeDamage(20);
             }
             
             if (damageable is not null)
             {
                 if (damageable is Door door)
                 {
                     door.TakeDamage(10);
                 }
                 else
                 {
                     damageable.TakeDamage(20);
                 }
             }
             
             
             Door doorUpperCast = (Door)damageable;
             doorUpperCast.TakeDamage(10);
             
             
        }
    }

    public class Door : IDamageable
    {
        public bool IsDead { get; }
        public int TakeDamage(int attackPower)
        {
            return 10;
        }
    }
}