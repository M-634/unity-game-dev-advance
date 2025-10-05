namespace Week01
{
    public interface IDamageableObject
    {
        bool IsDamaging { get; }
        void OnDamage(DamageDataContext damageDataContext);
    }

    public struct DamageDataContext
    {
        public int DamageAmount { get; private set; }
        
        public DamageDataContext(int damageAmount)
        {
            DamageAmount = damageAmount;
        }
    }
}