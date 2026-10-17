using UnityEngine;

namespace DemoGame
{
    public sealed class AttackState : IState
    {
        private readonly EnemyController _owner;
        private readonly AttackStateSettings _settings;

        private float _elapsed;
        private bool _hasHit;
        private bool _hasImpacted;

        public AttackState(EnemyController owner, AttackStateSettings settings)
        {
            _owner = owner;
            _settings = settings;
        }

        public bool IsHitActive => !_hasHit && _elapsed >= _settings.HitStartTime && _elapsed <= _settings.HitEndTime;

        public void OnEnter()
        {
            _elapsed = 0f;
            _hasHit = false;
            _hasImpacted = false;

            _owner.StopMoving();
            _owner.BeginAttackCooldown();
            _owner.SetTrigger(CharacterAnimatorParams.Attack);
        }

        public void OnUpdate()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed < _settings.HitStartTime)
            {
                _owner.FaceTarget();
            }
            else
            {
                if (!_hasImpacted)
                {
                    _hasImpacted = true;
                    _owner.NotifyAttackImpact();
                }

                if (IsHitActive && _owner.TryHitTarget(_settings))
                {
                    _hasHit = true;
                }
            }

            if (_elapsed >= _settings.Duration)
            {
                _owner.ChangeState(_owner.PursueState);
            }
        }

        public void OnExit()
        {
        }
    }
}
