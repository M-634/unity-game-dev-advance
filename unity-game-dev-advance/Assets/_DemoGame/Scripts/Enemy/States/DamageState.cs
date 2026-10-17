using UnityEngine;

namespace DemoGame
{
    public sealed class DamageState : IState
    {
        private readonly EnemyController _owner;
        private readonly DamageStateSettings _settings;

        private float _endTime;

        public DamageState(EnemyController owner, DamageStateSettings settings)
        {
            _owner = owner;
            _settings = settings;
        }

        public void OnEnter()
        {
            _endTime = Time.time + _settings.StaggerTime;

            _owner.StopMoving();
            _owner.SetTrigger(CharacterAnimatorParams.Hit);
        }

        public void OnUpdate()
        {
            if (Time.time < _endTime) return;

            _owner.ChangeState(_owner.PursueState);
        }

        public void OnExit()
        {
        }
    }
}
