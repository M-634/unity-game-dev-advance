namespace DemoGame
{
    public sealed class StateMachine
    {
        public IState CurrentState { get; private set; }

        // 同じ状態への切り替えは無視する。入り直したいとき（被弾の怯みを延ばすなど）は reenter を true にする
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
