using UnityEngine;

namespace DemoGame
{
    // プレイヤー・雑魚敵・ボスで共通の Animator パラメータ名
    // Animator Controller はモデルごとに作るが、パラメータ名だけはここに揃える
    public static class CharacterAnimatorParams
    {
        // 移動速度（m/s）。Blend Tree の閾値もこの単位で作る
        public static readonly int Speed = Animator.StringToHash("Speed");

        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        public static readonly int Attack = Animator.StringToHash("Attack");

        public static readonly int Hit = Animator.StringToHash("Hit");

        public static readonly int IsDead = Animator.StringToHash("IsDead");

        // ボスの咆哮（怒り）。持っていないモデルでは何も起きない
        public static readonly int Roar = Animator.StringToHash("Roar");
    }
}
