namespace Section.Common
{
    public sealed class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void Change(IState next, bool reenter = false)
        {
            if (CurrentState == next && !reenter) return;

            CurrentState?.OnExit();
            CurrentState = next;
            CurrentState?.OnEnter();
        }

        public void Update()
        {
            CurrentState?.OnUpdate();
        }
    }
}