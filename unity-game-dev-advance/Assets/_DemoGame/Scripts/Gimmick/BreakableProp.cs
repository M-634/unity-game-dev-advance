using R3;
using UnityEngine;

namespace DemoGame
{
    public class BreakableProp : MonoBehaviour, IDamageable
    {
        [SerializeField, Range(1, 100)]
        private int _maxHp = 10;

        [Tooltip("壊れたときに出すエフェクト")]
        [SerializeField]
        private GameObject _breakEffect;

        [Header("当たったときの揺れ")]
        [SerializeField, Range(0f, 0.5f)]
        private float _shakeScale = 0.12f;

        [SerializeField, Range(0.01f, 1f)]
        private float _shakeSeconds = 0.15f;

        private readonly Subject<Unit> _broken = new();
        private int _hp;
        private Vector3 _baseScale;
        private float _shakeUntil;

        public Observable<Unit> Broken => _broken;

        public bool IsDead => _hp <= 0;

        private void Awake()
        {
            _hp = _maxHp;
            _baseScale = transform.localScale;
        }

        public int TakeDamage(int attackPower)
        {
            if (IsDead || attackPower <= 0) return 0;

            int damage = Mathf.Min(attackPower, _hp);
            _hp -= damage;
            _shakeUntil = Time.time + _shakeSeconds;

            if (IsDead)
            {
                Break();
            }

            return damage;
        }

        private void Update()
        {
            if (Time.time >= _shakeUntil)
            {
                transform.localScale = _baseScale;
                return;
            }

            float t = (_shakeUntil - Time.time) / _shakeSeconds;
            transform.localScale = _baseScale * (1f + Mathf.Sin(t * Mathf.PI * 4f) * _shakeScale * t);
        }

        private void Break()
        {
            FxSpawner.Spawn(_breakEffect, transform.position + Vector3.up * 0.5f);
            _broken.OnNext(Unit.Default);
            _broken.OnCompleted();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            _broken.Dispose();
        }
    }
}
