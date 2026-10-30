namespace Section2.AIState
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}
