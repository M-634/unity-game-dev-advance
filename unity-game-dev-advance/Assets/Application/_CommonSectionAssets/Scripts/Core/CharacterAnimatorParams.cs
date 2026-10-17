using UnityEngine;

namespace Section.Common
{
    public static class CharacterAnimatorParams
    {
        public static readonly int Speed = Animator.StringToHash("Speed");

        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        public static readonly int Attack = Animator.StringToHash("Attack");

        public static readonly int Hit = Animator.StringToHash("Hit");

        public static readonly int IsDead = Animator.StringToHash("IsDead");
        
        public static readonly int Roar = Animator.StringToHash("Roar");
    }
}
