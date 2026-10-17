using UnityEngine;

namespace DemoGame
{
    public sealed class PursueState : IState
    {
        private readonly EnemyController _owner;
        private readonly PursueStateSettings _settings;

        private float _nextRepathTime;

        public PursueState(EnemyController owner, PursueStateSettings settings)
        {
            _owner = owner;
            _settings = settings;
        }

        public void OnEnter()
        {
            _nextRepathTime = 0f;
        }

        public void OnUpdate()
        {
            float distance = _owner.DistanceToTarget;

            if (distance > _settings.LoseRange)
            {
                _owner.ChangeState(_owner.SearchState);
                return;
            }

            if (distance <= _settings.AttackRange)
            {
                _owner.StopMoving();
                _owner.FaceTarget();
                _nextRepathTime = 0f;

                if (_owner.CanAttack)
                {
                    _owner.ChangeState(_owner.AttackState);
                }

                return;
            }

            if (Time.time < _nextRepathTime) return;

            _nextRepathTime = Time.time + _settings.RepathInterval;
            _owner.MoveTo(_owner.Target.position, _owner.Parameters.PursueSpeed);
        }

        public void OnExit()
        {
            _owner.StopMoving();
        }
    }
}
