using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Section.Common
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

        private readonly Subject<Unit> _activated = new();
        private readonly Subject<Unit> _defeated = new();
        private readonly ReactiveProperty<bool> _isAngry = new(false);

        private readonly Queue<float> _recentHitTimes = new Queue<float>();

        public BossParameters BossParameters => _bossParameters;
        public Observable<Unit> Activated => _activated;
        public Observable<Unit> Defeated => _defeated;
        public ReadOnlyReactiveProperty<bool> IsAngry => _isAngry;
        public bool IsActivated { get; private set; }

        public IState AngryState { get; private set; }
        public IState InvincibleState { get; private set; }

        public override bool CanStart => IsActivated;
        public override IState FirstState => PursueState;

        protected override bool IsInterruptible =>
            base.IsInterruptible && CurrentState != AngryState && CurrentState != InvincibleState;

        protected override void CreateStates()
        {
            AngryState = new AngryState(this, _angry);
            InvincibleState = new InvincibleState(this, _bossParameters, _invincible);
        }

        public void Activate()
        {
            if (IsActivated) return;

            IsActivated = true;
            _activated.OnNext(Unit.Default);
        }

        public void MarkAngry()
        {
            _isAngry.Value = true;
        }

        protected override void OnDamaged(int damage)
        {
            if (!IsInterruptible) return;

            if (!_isAngry.Value && CurrentHp.CurrentValue <= _bossParameters.AngryStateTargetHp)
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

        protected override void OnDied()
        {
            _defeated.OnNext(Unit.Default);
            _defeated.OnCompleted();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _activated.Dispose();
            _defeated.Dispose();
            _isAngry.Dispose();
        }
    }
}
