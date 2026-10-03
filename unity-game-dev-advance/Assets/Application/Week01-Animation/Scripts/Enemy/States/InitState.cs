namespace Week01
{
    // 初期化。NavMesh に乗せ、追う相手を探し、動き出してよくなるまで待つ
    public sealed class InitState : IState
    {
        private readonly EnemyController _owner;

        public InitState(EnemyController owner)
        {
            _owner = owner;
        }

        public void OnEnter()
        {
            _owner.FindTarget();
            _owner.EnsureOnNavMesh();
            _owner.StopMoving();
        }

        public void OnUpdate()
        {
            // NavMesh が焼けるのが遅れたときに備えて、乗れるまで試す
            if (!_owner.EnsureOnNavMesh()) return;
            if (!_owner.CanStart) return;

            _owner.ChangeState(_owner.FirstState);
        }

        public void OnExit()
        {
        }
    }
}
