namespace Week01
{
    // 開発用。棒立ちになり、どんな攻撃でも一撃で倒れる
    public sealed class DebugState : IState
    {
        private readonly EnemyController _owner;

        public DebugState(EnemyController owner)
        {
            _owner = owner;
        }

        public void OnEnter()
        {
            _owner.StopMoving();
            _owner.IsOneHitKill = true;
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
            _owner.IsOneHitKill = false;
        }
    }
}
