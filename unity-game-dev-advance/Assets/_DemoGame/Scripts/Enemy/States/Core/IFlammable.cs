namespace DemoGame
{
    public interface IFlammable
    {
        bool IsBurnt { get; }

        void Ignite();
    }
}
