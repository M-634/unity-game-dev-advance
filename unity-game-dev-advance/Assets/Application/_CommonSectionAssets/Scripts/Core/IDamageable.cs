namespace Section.Common
{
    public interface IDamageable
    {
        bool IsDead { get; }
        
        int TakeDamage(int attackPower);
    }
}
