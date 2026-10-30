using System.Collections.Generic;
using UnityEngine;

namespace Section2.AIState
{
    public class BossController : EnemyController
    {
        [Header("ボス")]
        [SerializeField]
        private BossParameters _bossParameters = new BossParameters();

        [SerializeField]
        private AngryStateSettings _angry = new AngryStateSettings();

        [SerializeField]
        private InvincibleStateSettings _invincible = new InvincibleStateSettings();

        private readonly Queue<float> _recentHitTimes = new Queue<float>();

        private bool _isAngry;

        public IState AngryState { get; private set; }
        public IState InvincibleState { get; private set; }

        protected override bool IsInterruptible =>
            base.IsInterruptible && CurrentState != AngryState && CurrentState != InvincibleState;

        protected override void CreateStates()
        {
            AngryState = new AngryState(this, _angry);
            InvincibleState = new InvincibleState(this, _bossParameters, _invincible);
        }

        public void MarkAngry()
        {
            _isAngry = true;
        }

        protected override void OnDamaged(int damage)
        {
            if (!IsInterruptible) return;

            if (!_isAngry && CurrentHp <= _bossParameters.AngryStateTargetHp)
            {
                _recentHitTimes.Clear();
                ChangeState(AngryState);
                return;
            }

            if (CountRecentHit() >= _invincible.HitCount)
            {
                _recentHitTimes.Clear();
                ChangeState(InvincibleState);
                return;
            }

            base.OnDamaged(damage);
        }

        private int CountRecentHit()
        {
            float now = Time.time;
            _recentHitTimes.Enqueue(now);

            while (_recentHitTimes.Count > 0 && now - _recentHitTimes.Peek() > _invincible.HitWindow)
            {
                _recentHitTimes.Dequeue();
            }

            return _recentHitTimes.Count;
        }
    }
}
