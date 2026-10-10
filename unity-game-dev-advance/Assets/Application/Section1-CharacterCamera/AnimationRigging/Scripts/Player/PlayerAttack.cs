using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Pool;

namespace Section1.AnimationRigging
{
    // 攻撃が当たったときの情報。ダメージ数の表示などに使う
    public readonly struct AttackHit
    {
        public readonly IDamageable Target;

        // 相手のコライダーの表面のうち、攻撃の中心にいちばん近い点
        public readonly Vector3 Point;

        // 実際に減った HP（防御・無敵で 0 のこともある）
        public readonly int Damage;

        public AttackHit(IDamageable target, Vector3 point, int damage)
        {
            Target = target;
            Point = point;
            Damage = damage;
        }
    }

    // 火焔玉の段ごとの数値。設計：doc/demo-game/10-level-design.md §5.2・§5.3
    [Serializable]
    public struct FireballStage
    {
        [Tooltip("この段になるまでに押し続ける時間（秒）")]
        public float HoldTime;

        public int Damage;

        [Tooltip("撃ったときに減る杖の火")]
        public float FlameCost;

        [Tooltip("弾の大きさ（チャージなし = 1）")]
        public float Scale;

        [Tooltip("弾けたときにまわりにもダメージを与える半径。0 なら当たった相手だけ")]
        public float ExplosionRadius;

        public FireballStage(float holdTime, int damage, float flameCost, float scale, float explosionRadius)
        {
            HoldTime = holdTime;
            Damage = damage;
            FlameCost = flameCost;
            Scale = scale;
            ExplosionRadius = explosionRadius;
        }
    }

    // 攻撃。杖が消えていれば殴る。灯っていれば火焔玉（押してすぐ離すとチャージなし、押し続けると杖の光の段までチャージ）
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField]
        private InputReader _input;

        [Tooltip("杖。未設定なら自分から探す。無ければ殴るだけ")]
        [SerializeField]
        private Staff _staff;

        [Tooltip("チャージ中に走らせないため・撃つ向きに体を向けるため")]
        [SerializeField]
        private PlayerController _controller;

        [Tooltip("判定の基準になる向き。未設定なら自分の transform を使う")]
        [SerializeField]
        private Transform _origin;

        [Header("判定")]
        [SerializeField, Range(0.1f, 5f)]
        private float _radius = 1.3f;

        [Tooltip("基準から前方にどれだけ離した位置に判定を出すか")]
        [SerializeField, Range(0f, 5f)]
        private float _forwardOffset = 1.2f;

        [Tooltip("基準からどれだけ上に判定を出すか。基準（Visual）は足元なので、0 だと球の中心が地面になる")]
        [SerializeField, Range(0f, 3f)]
        private float _heightOffset = 0.9f;

        [Tooltip("殴りのダメージ。火焔玉より弱くする")]
        [SerializeField]
        private int _damage = 5;

        [SerializeField, Range(0.05f, 3f)]
        private float _cooldown = 0.45f;

        [Tooltip("攻撃が当たる対象のレイヤー")]
        [SerializeField]
        private LayerMask _targetLayers = ~0;

        [Tooltip("殴りが当たったときのエフェクト（doc/demo-game/12-vfx.md #7）")]
        [SerializeField]
        private GameObject _meleeHitEffect;

        [Header("火焔玉")]
        [SerializeField]
        private Fireball _fireballPrefab;

        [Tooltip("狙いの基準になるカメラ。未設定なら Main Camera")]
        [SerializeField]
        private Transform _aimCamera;

        [Tooltip("画面の中央の先、どこまでを狙いの点として探すか（m）")]
        [SerializeField, Range(5f, 200f)]
        private float _aimDistance = 60f;

        [Tooltip("0：チャージなし ／ 1〜3：チャージの段。使える段は杖の光の段（Glow）まで")]
        [SerializeField]
        private FireballStage[] _fireballStages =
        {
            new FireballStage(0f, 15, 10f, 1f, 0f),
            new FireballStage(0.4f, 22, 15f, 1.3f, 0f),
            new FireballStage(0.8f, 30, 20f, 1.6f, 0f),
            new FireballStage(1.2f, 60, 35f, 2.4f, 2f),
        };

        [Header("デバッグ")]
        [Tooltip("攻撃の判定を描く（Editor と APP_DEBUG のビルドだけ）")]
        [SerializeField]
        private bool _drawGizmo = true;

        [Tooltip("攻撃したあと、判定を何秒描いておくか")]
        [SerializeField, Range(0f, 2f)]
        private float _gizmoDuration = 0.3f;

        public event Action Swung;

        // 箱を押している間など、攻撃させたくないときに true にする
        public bool IsLocked { get; set; }

        public event Action<AttackHit> Hit;

        // 撃った瞬間（段）。着弾ではない
        public event Action<int> FireballFired;

        // チャージ中の段。-1 はチャージしていない（画面の縁の炎・杖の見た目が購読する）
        public ReadOnlyReactiveProperty<int> ChargeLevel => _chargeLevel;

        private readonly ReactiveProperty<int> _chargeLevel = new(-1);
        private ObjectPool<Fireball> _fireballPool;
        private readonly RaycastHit[] _aimBuffer = new RaycastHit[8];
        private bool _isCharging;
        private float _chargeStartTime;

        private float _nextAttackTime;
        private readonly HashSet<IDamageable> _hitCache = new HashSet<IDamageable>();

        // Gizmos 用。最後に攻撃した位置と、当たったか
        private float _lastSwingTime = float.NegativeInfinity;
        private Vector3 _lastSwingCenter;
        private bool _lastSwingHit;

        private void Awake()
        {
            if (_origin == null)
            {
                _origin = transform;
            }

            if (_staff == null)
            {
                _staff = GetComponent<Staff>();
            }

            if (_controller == null)
            {
                _controller = GetComponent<PlayerController>();
            }

            if (_fireballPrefab != null)
            {
                _fireballPool = new ObjectPool<Fireball>(
                    createFunc: () => Instantiate(_fireballPrefab),
                    actionOnGet: fireball => fireball.gameObject.SetActive(true),
                    actionOnRelease: fireball => fireball.gameObject.SetActive(false),
                    actionOnDestroy: fireball =>
                    {
                        if (fireball != null) Destroy(fireball.gameObject);
                    },
                    defaultCapacity: 4,
                    maxSize: 16);
            }
        }

        private void OnEnable()
        {
            if (_input == null)
            {
                Debug.LogError($"{nameof(PlayerAttack)}: InputReader が設定されていません。", this);
                return;
            }

            _input.Attacked += OnAttacked;
            _input.AttackReleased += OnAttackReleased;
        }

        private void OnDisable()
        {
            if (_input == null) return;

            _input.Attacked -= OnAttacked;
            _input.AttackReleased -= OnAttackReleased;

            CancelCharge();
        }

        private void OnAttacked()
        {
            if (IsLocked) return;
            if (Time.time < _nextAttackTime) return;

            // 杖を拾うまでは攻撃できない
            if (_staff != null && !_staff.IsOwned.CurrentValue) return;

            if (_staff != null && _staff.IsLit && _fireballPrefab != null)
            {
                BeginCharge();
                return;
            }

            _nextAttackTime = Time.time + _cooldown;
            Swung?.Invoke();

            ApplyDamage();
        }

        private void OnAttackReleased()
        {
            if (!_isCharging) return;

            int level = Mathf.Max(0, _chargeLevel.Value);
            CancelCharge();
            Fire(level);
        }

        private void BeginCharge()
        {
            _isCharging = true;
            _chargeStartTime = Time.time;
            SetChargeLevel(0);

            if (_controller != null)
            {
                _controller.IsSprintLocked = true;
            }
        }

        private void CancelCharge()
        {
            if (!_isCharging) return;

            _isCharging = false;
            SetChargeLevel(-1);

            if (_controller != null)
            {
                _controller.IsSprintLocked = false;
            }
        }

        private void UpdateCharge()
        {
            if (!_isCharging) return;

            // 箱を押し始めた・杖の火が消えたなどで撃てなくなったら、撃たずにやめる
            if (IsLocked || _staff == null || !_staff.IsLit)
            {
                CancelCharge();
                return;
            }

            float held = Time.time - _chargeStartTime;
            int maxLevel = Mathf.Min(_staff.Glow.CurrentValue, _fireballStages.Length - 1);

            int level = 0;
            for (int i = 1; i <= maxLevel; i++)
            {
                // 杖の火が足りない段には上がらない
                if (held < _fireballStages[i].HoldTime) break;
                if (_staff.Flame.CurrentValue < _fireballStages[i].FlameCost) break;

                level = i;
            }

            SetChargeLevel(level);
        }

        private void SetChargeLevel(int level)
        {
            if (_chargeLevel.Value == level) return;

            _chargeLevel.Value = level;

            if (_staff != null && _staff.Visual != null)
            {
                _staff.Visual.SetCharge(level);
            }
        }

        private void Fire(int level)
        {
            if (_staff == null || !_staff.IsLit || _fireballPrefab == null) return;

            var stage = _fireballStages[Mathf.Clamp(level, 0, _fireballStages.Length - 1)];

            _nextAttackTime = Time.time + _cooldown;
            Swung?.Invoke();

            Vector3 direction = GetAimDirection();
            FaceTowards(direction);

            // 体の向きを変えてから位置を取る（杖の先は体と一緒に回る）
            var muzzle = _staff.Muzzle;
            direction = GetAimDirectionFrom(muzzle.position, direction);

            var fireball = _fireballPool.Get();
            fireball.transform.SetPositionAndRotation(muzzle.position, Quaternion.LookRotation(direction));
            fireball.Launch(direction, stage.Damage, stage.Scale, stage.ExplosionRadius, transform.root, OnFireballHit, ReleaseFireball);

            _staff.Use(stage.FlameCost, level);
            FireballFired?.Invoke(level);
        }

        private void OnFireballHit(AttackHit hit)
        {
            Hit?.Invoke(hit);
        }

        private void ReleaseFireball(Fireball fireball)
        {
            // プレイヤーが先に消えていたら、弾も消す
            if (_fireballPool == null)
            {
                Destroy(fireball.gameObject);
                return;
            }

            _fireballPool.Release(fireball);
        }

        // 画面の中央（カメラの正面）。TPS でも FPS でも同じ
        private Vector3 GetAimDirection()
        {
            var cam = GetAimCamera();
            if (cam != null) return cam.forward;

            var origin = _origin != null ? _origin : transform;
            return origin.forward;
        }

        // カメラの正面の先にある物を狙いの点にして、杖の先からそこへ向ける
        private Vector3 GetAimDirectionFrom(Vector3 muzzlePosition, Vector3 fallback)
        {
            var cam = GetAimCamera();
            if (cam == null) return fallback;

            Vector3 target = cam.position + cam.forward * _aimDistance;

            int count = Physics.RaycastNonAlloc(cam.position, cam.forward, _aimBuffer, _aimDistance, _targetLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var hit = _aimBuffer[i];
                if (hit.collider.transform.root == transform.root) continue;
                if (hit.distance >= nearest) continue;

                nearest = hit.distance;
                target = hit.point;
            }

            Vector3 direction = target - muzzlePosition;

            // 狙いの点が杖の先より手前（すぐ目の前の壁など）なら、カメラの向きのまま撃つ
            if (direction.sqrMagnitude < 0.25f || Vector3.Dot(direction, cam.forward) <= 0f)
            {
                return cam.forward;
            }

            return direction.normalized;
        }

        private Transform GetAimCamera()
        {
            if (_aimCamera != null) return _aimCamera;

            var main = Camera.main;
            return main != null ? main.transform : null;
        }

        // TPS では撃つ向きへ体を向ける（FPS はもともとカメラの向き）
        private void FaceTowards(Vector3 direction)
        {
            if (_controller == null || _controller.ViewMode != CameraMode.Tps) return;

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;

            _controller.VisualRoot.rotation = Quaternion.LookRotation(direction);
        }

        private void ApplyDamage()
        {
            _hitCache.Clear();

            Vector3 center = GetAttackCenter();
            var colliders = Physics.OverlapSphere(center, _radius, _targetLayers, QueryTriggerInteraction.Ignore);

            _lastSwingTime = Time.time;
            _lastSwingCenter = center;
            _lastSwingHit = false;

            foreach (var col in colliders)
            {
                // 自分自身も IDamageable を持っているので、まず除外する
                if (col.transform.root == transform.root) continue;

                // 1つの相手が複数のコライダーを持っていても、ダメージは1回だけ
                if (!col.TryGetComponent(out IDamageable target)) continue;
                if (target.IsDead) continue;
                if (!_hitCache.Add(target)) continue;

                int damage = target.TakeDamage(_damage);
                _lastSwingHit = true;
                Vector3 point = col.ClosestPoint(center);
                FxSpawner.Spawn(_meleeHitEffect, point);
                Hit?.Invoke(new AttackHit(target, point, damage));
            }
        }

        private Vector3 GetAttackCenter()
        {
            var origin = _origin != null ? _origin : transform;
            return origin.position + origin.forward * _forwardOffset + Vector3.up * _heightOffset;
        }

        // 判定の位置を描く（DebugShapes。Scene ビューと Game ビューの両方に出る）
        //   薄い橙：いま攻撃したら出る位置 ／ 攻撃した直後：当たったら赤、外れたら白
        private void Update()
        {
            UpdateCharge();

            if (!_drawGizmo) return;

            DebugShapes.WireSphere(GetAttackCenter(), _radius, new Color(1f, 0.5f, 0.2f, 0.35f));

            if (Time.time - _lastSwingTime > _gizmoDuration) return;

            var color = _lastSwingHit ? new Color(1f, 0.1f, 0.1f, 1f) : new Color(1f, 1f, 1f, 0.9f);
            DebugShapes.WireSphere(_lastSwingCenter, _radius, color);
            DebugShapes.WireSphere(_lastSwingCenter, _radius * 0.98f, color);
        }

        private void OnDestroy()
        {
            _chargeLevel.Dispose();
            _fireballPool?.Clear();
            _fireballPool = null;
        }

#if UNITY_EDITOR
        // 再生していないときに、選ぶと判定の位置を確かめられる
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying || !_drawGizmo) return;

            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(GetAttackCenter(), _radius);
        }
#endif
    }
}
