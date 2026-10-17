using UnityEngine;

namespace DemoGame
{
    public sealed class InvincibleState : IState
    {
        private readonly BossController _owner;
        private readonly BossParameters _parameters;
        private readonly InvincibleStateSettings _settings;

        private float _endTime;

        public InvincibleState(BossController owner, BossParameters parameters, InvincibleStateSettings settings)
        {
            _owner = owner;
            _parameters = parameters;
            _settings = settings;
        }

        public void OnEnter()
        {
            _endTime = Time.time + _parameters.InvincibleTime;

            _owner.StopMoving();
            _owner.IsInvincible = true;

            if (_owner.Visual != null)
            {
                _owner.Visual.SetOverlay(_settings.GuardColor, _settings.GuardAmount);
            }
        }

        public void OnUpdate()
        {
            _owner.FaceTarget();

            if (Time.time < _endTime) return;

            _owner.ChangeState(_owner.PursueState);
        }

        public void OnExit()
        {
            _owner.IsInvincible = false;

            if (_owner.Visual != null)
            {
                _owner.Visual.SetOverlay(Color.white, 0f);
            }
        }
    }
}
