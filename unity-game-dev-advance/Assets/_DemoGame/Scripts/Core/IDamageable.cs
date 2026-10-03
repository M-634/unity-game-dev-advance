namespace DemoGame
{
    public interface IDamageable
    {
        bool IsDead { get; }

        // 攻撃力を渡す。実際に減った HP を返す（防御・無敵で減らないこともある）
        int TakeDamage(int attackPower);
    }
}
