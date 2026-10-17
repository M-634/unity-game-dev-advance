using R3;
using UnityEngine;

namespace Section.Common
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField]
        private int _maxHp = 100;

        [Tooltip("被弾後、次のダメージを受け付けない時間（秒）")]
        [SerializeField, Range(0f, 5f)]
        private float _invincibleTime = 1f;
        
        [Header("デバック用の無敵モード")]
        [SerializeField]
        public bool _debugInvincibleMode;

        private readonly ReactiveProperty<int> _currentHp = new(0);
        private readonly Subject<Unit> _died = new();

        public int MaxHp => _maxHp;
        public ReadOnlyReactiveProperty<int> CurrentHp => _currentHp;
        public bool IsDead => _currentHp.Value <= 0;

        public Observable<Unit> Died => _died;

        private float _invincibleUntil;

        private void Awake()
        {
            _currentHp.Value = _maxHp;
        }
        
        public int TakeDamage(int attackPower)
        {
            if(_debugInvincibleMode) return 0;
            if (IsDead || attackPower <= 0) return 0;
            if (Time.time < _invincibleUntil) return 0;

            int damage = Mathf.Min(attackPower, _currentHp.Value);
            _invincibleUntil = Time.time + _invincibleTime;
            _currentHp.Value -= damage;

            if (!IsDead) return damage;

            _died.OnNext(Unit.Default);
            _died.OnCompleted();
            return damage;
        }

        public void ResetHp()
        {
            _invincibleUntil = 0f;
            _currentHp.Value = _maxHp;
        }

        private void OnDestroy()
        {
            _currentHp.Dispose();
            _died.Dispose();
        }
    }
}
