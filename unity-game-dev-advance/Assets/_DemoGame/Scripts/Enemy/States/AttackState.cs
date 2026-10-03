using UnityEngine;

namespace DemoGame
{
    // 振りかぶり → 当たり判定を出す時間帯 → 戻り。Animation Event は使わず、時間で区切る
    // 判定はボーン（手・頭など）に付いた球なので、位置はモーションに追従する。合わせるのは時間帯だけ
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

        // 判定を出している最中か（開発用の表示に使う）
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

            // 判定を出すまでは相手の方へ向き直す。出し始めたら向きを固定する（避けられるように）
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
