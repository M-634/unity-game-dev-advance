using System;
using System.Collections.Generic;
using UnityEngine;

namespace DemoGame
{
    // 火焔玉の弾。まっすぐ飛び、最初に当たった物で弾ける。設計：doc/demo-game/10-level-design.md §5
    // 当たりは Physics の接触ではなく、毎フレーム「前の位置から今の位置まで」の SphereCast で調べる（速くてもすり抜けない）
    // 使い回す（Object Pool）。弾けたら・時間切れになったら Launch で渡された返し先へ戻る
    public class Fireball : MonoBehaviour
    {
        [SerializeField, Range(1f, 60f)]
        private float _speed = 18f;

        [Tooltip("大きさ 1 のときの当たりの半径")]
        [SerializeField, Range(0.05f, 2f)]
        private float _radius = 0.25f;

        [SerializeField, Range(0.5f, 10f)]
        private float _lifetime = 3f;

        [SerializeField]
        private LayerMask _hitLayers = ~0;

        [Tooltip("段で大きさを変えるもの（見た目のルート）")]
        [SerializeField]
        private Transform _visual;

        [SerializeField]
        private Light _light;

        [Tooltip("弾のパーティクル（FX_Fireball：芯・軌跡・火の粉）。_visual の子に付ける。使い回すときに消してから出し直す")]
        [SerializeField]
        private GameObject _particlesPrefab;

        [Tooltip("弾けたときのエフェクト（doc/demo-game/12-vfx.md #6）。段の大きさに合わせて大きくする")]
        [SerializeField]
        private GameObject _impactEffect;

        private readonly RaycastHit[] _castBuffer = new RaycastHit[8];
        private readonly Collider[] _overlapBuffer = new Collider[16];
        private readonly HashSet<IDamageable> _damaged = new HashSet<IDamageable>();

        private Vector3 _direction;
        private int _damage;
        private float _scale = 1f;
        private float _explosionRadius;
        private Transform _ownerRoot;
        private Action<AttackHit> _onHit;
        private float _age;
        private bool _isLaunched;
        private Action<Fireball> _release;
        private Vector3 _visualScale = Vector3.one;
        private float _lightRange;
        private GameObject _particles;

        private void Awake()
        {
            _particles = FxSpawner.Attach(_particlesPrefab, _visual != null ? _visual : transform);

            if (_visual != null)
            {
                _visualScale = _visual.localScale;
            }

            if (_light != null)
            {
                _lightRange = _light.range;
            }
        }

        private float Radius => _radius * _scale;

        // explosionRadius が 0 なら当たった相手だけ。0 より大きければ、弾けた位置のまわりにもダメージ
        // release が null なら、終わったら Destroy する
        public void Launch(Vector3 direction, int damage, float scale, float explosionRadius, Transform ownerRoot, Action<AttackHit> onHit, Action<Fireball> release = null)
        {
            _release = release;
            _age = 0f;
            _direction = direction.normalized;
            _damage = damage;
            _scale = scale;
            _explosionRadius = explosionRadius;
            _ownerRoot = ownerRoot;
            _onHit = onHit;
            _isLaunched = true;

            transform.rotation = Quaternion.LookRotation(_direction);

            FxSpawner.Play(_particles, clear: true);

            if (_visual != null)
            {
                _visual.localScale = _visualScale * scale;
            }

            if (_light != null)
            {
                _light.range = _lightRange * Mathf.Sqrt(scale);
            }

            // 撃った位置で、すでに何かに重なっていたらその場で弾ける（目の前の敵）
            int count = Physics.OverlapSphereNonAlloc(transform.position, Radius, _overlapBuffer, _hitLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (IsOwner(_overlapBuffer[i])) continue;

                Explode(transform.position, _overlapBuffer[i]);
                return;
            }
        }

        private void Update()
        {
            if (!_isLaunched) return;

            _age += Time.deltaTime;
            if (_age >= _lifetime)
            {
                Despawn();
                return;
            }

            float distance = _speed * Time.deltaTime;
            Vector3 origin = transform.position;

            int count = Physics.SphereCastNonAlloc(origin, Radius, _direction, _castBuffer, distance, _hitLayers, QueryTriggerInteraction.Ignore);

            int nearest = -1;
            for (int i = 0; i < count; i++)
            {
                if (IsOwner(_castBuffer[i].collider)) continue;
                if (nearest < 0 || _castBuffer[i].distance < _castBuffer[nearest].distance)
                {
                    nearest = i;
                }
            }

            if (nearest >= 0)
            {
                var hit = _castBuffer[nearest];
                // distance が 0 のときは point が入らないので、今の位置で弾けさせる
                Vector3 point = hit.distance > 0f ? hit.point : origin;
                transform.position = origin + _direction * hit.distance;
                Explode(point, hit.collider);
                return;
            }

            transform.position = origin + _direction * distance;
        }

        private bool IsOwner(Collider col)
        {
            return _ownerRoot != null && col.transform.root == _ownerRoot;
        }

        private void Explode(Vector3 point, Collider directHit)
        {
            _isLaunched = false;
            _damaged.Clear();

            Affect(directHit, point);

            if (_explosionRadius > 0f)
            {
                int count = Physics.OverlapSphereNonAlloc(point, _explosionRadius, _overlapBuffer, _hitLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    if (IsOwner(_overlapBuffer[i])) continue;

                    Affect(_overlapBuffer[i], point);
                }
            }

            FxSpawner.Spawn(_impactEffect, point, Quaternion.LookRotation(-_direction), _scale);
            Despawn();
        }

        private void Despawn()
        {
            _isLaunched = false;
            _onHit = null;

            if (_release != null)
            {
                var release = _release;
                _release = null;
                release(this);
                return;
            }

            Destroy(gameObject);
        }

        private void Affect(Collider col, Vector3 point)
        {
            if (col == null) return;

            var flammable = col.GetComponentInParent<IFlammable>();
            if (flammable != null && !flammable.IsBurnt)
            {
                flammable.Ignite();
            }

            var target = col.GetComponentInParent<IDamageable>();
            if (target == null || target.IsDead) return;

            // 1つの相手が複数のコライダーを持っていても、ダメージは1回だけ
            if (!_damaged.Add(target)) return;

            int damage = target.TakeDamage(_damage);
            _onHit?.Invoke(new AttackHit(target, col.ClosestPoint(point), damage));
        }
    }
}
