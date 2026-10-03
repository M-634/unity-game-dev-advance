using UnityEngine;

namespace DemoGame
{
    // 蔦で吊られた瓦礫。蔦が燃え尽きると落ち、落ちている間に当たった相手（ボス）にダメージ。設計：doc/demo-game/10-level-design.md §7
    // Rigidbody は最初 Kinematic（吊られている）。Drop() で重力を受ける
    [RequireComponent(typeof(Rigidbody))]
    public class FallingRubble : MonoBehaviour
    {
        [SerializeField]
        private int _damage = 50;

        [Tooltip("この速さ（m/s）より速く落ちているときだけダメージ")]
        [SerializeField, Range(0f, 20f)]
        private float _minSpeed = 3f;

        [Tooltip("地面や相手に当たったときの砂煙（doc/demo-game/12-vfx.md #19）")]
        [SerializeField]
        private GameObject _landEffect;

        [Tooltip("落ちてから消えるまでの秒数。0 なら残す")]
        [SerializeField, Range(0f, 30f)]
        private float _lifetimeAfterDrop = 6f;

        private Rigidbody _rigidbody;
        private bool _isDropped;
        private bool _hasHit;
        private bool _hasLanded;

        public bool IsDropped => _isDropped;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
        }

        public void Drop()
        {
            if (_isDropped) return;

            _isDropped = true;
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;

            if (_lifetimeAfterDrop > 0f)
            {
                Destroy(gameObject, _lifetimeAfterDrop);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isDropped || _hasHit) return;

            if (!_hasLanded)
            {
                _hasLanded = true;
                FxSpawner.Spawn(_landEffect, collision.contactCount > 0 ? collision.GetContact(0).point : transform.position);
            }

            if (collision.relativeVelocity.magnitude < _minSpeed)
            {
                _hasHit = true;
                return;
            }

            var target = collision.collider.GetComponentInParent<IDamageable>();
            if (target == null || target.IsDead) return;

            // プレイヤーには当てない（ボスを誘い込む仕掛け）
            if (collision.collider.transform.root.CompareTag("Player")) return;

            _hasHit = true;
            target.TakeDamage(_damage);
        }
    }
}
