namespace Section1.AnimationRigging
{
    // 火焔玉で燃える物（蔦など）。設計：doc/demo-game/10-level-design.md §5.4・§7
    public interface IFlammable
    {
        bool IsBurnt { get; }

        void Ignite();
    }
}
