using UnityEngine;

namespace DemoGame
{
    public sealed class DieState : IState
    {
        private readonly EnemyController _owner;
        private readonly DieStateSettings _settings;

        private float _elapsed;
        private bool _hasSpawnedEffect;

        public DieState(EnemyController owner, DieStateSettings settings)
        {
            _owner = owner;
            _settings = settings;
        }

        public void OnEnter()
        {
            _elapsed = 0f;
            _hasSpawnedEffect = false;

            _owner.DisableBody();
            _owner.SetBool(CharacterAnimatorParams.IsDead, true);
        }

        public void OnUpdate()
        {
            _elapsed += Time.deltaTime;

            float t = _settings.DissolveDuration > 0f
                ? (_elapsed - _settings.DissolveDelay) / _settings.DissolveDuration
                : _elapsed - _settings.DissolveDelay;

            if (t <= 0f) return;

            if (!_hasSpawnedEffect)
            {
                _hasSpawnedEffect = true;
                _owner.SpawnDeathEffect();
            }

            if (_owner.Visual != null)
            {
                _owner.Visual.SetDissolve(Mathf.Clamp01(t));
            }

            if (t >= 1f)
            {
                _owner.DestroySelf();
            }
        }

        public void OnExit()
        {
        }
    }
}
