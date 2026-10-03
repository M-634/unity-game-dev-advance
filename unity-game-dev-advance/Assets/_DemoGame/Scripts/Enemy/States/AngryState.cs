using UnityEngine;

namespace DemoGame
{
    // ボス。咆哮して、攻撃力と防御力を上げる。上がった値は倒されるまで続く
    public sealed class AngryState : IState
    {
        private readonly BossController _owner;
        private readonly AngryStateSettings _settings;

        private float _endTime;

        public AngryState(BossController owner, AngryStateSettings settings)
        {
            _owner = owner;
            _settings = settings;
        }

        public void OnEnter()
        {
            _endTime = Time.time + _settings.RoarDuration;

            _owner.StopMoving();
            _owner.AttackPowerBonus = _settings.AttackPowerBonus;
            _owner.DefencePowerBonus = _settings.DefencePowerBonus;
            _owner.MarkAngry();
            _owner.SetTrigger(CharacterAnimatorParams.Roar);

            if (_owner.Visual != null)
            {
                _owner.Visual.SetEmission(_settings.EmissionColor);
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
        }
    }
}
