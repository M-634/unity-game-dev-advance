namespace Week01
{
    public interface IState
    {
        void OnEnter();

        void OnUpdate();

        void OnExit();
    }
}
