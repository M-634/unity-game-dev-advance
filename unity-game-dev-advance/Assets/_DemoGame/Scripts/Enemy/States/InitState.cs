namespace DemoGame
{
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
            if (!_owner.EnsureOnNavMesh()) return;
            if (!_owner.CanStart) return;

            _owner.ChangeState(_owner.FirstState);
        }

        public void OnExit()
        {
        }
    }
}
