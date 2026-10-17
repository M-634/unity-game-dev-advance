using R3;
using UnityEngine;
using UnityEngine.AI;

namespace Section.Common
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        public static bool DebugModeForAll { get; set; }

        [SerializeField]
        private EnemyParameters _parameters = new EnemyParameters();

        [SerializeField]
        private NavMeshAgent _agent;

        [Tooltip("見た目の層。空なら子から探す")]
        [SerializeField]
        private CharacterVisual _visual;

        [Tooltip("追う相手。空ならプレイヤーを探す")]
        [SerializeField]
        private Transform _target;

        [Header("状態ごとの設定")]
        [SerializeField]
        private SearchStateSettings _search = new SearchStateSettings();

        [SerializeField]
        private PursueStateSettings _pursue = new PursueStateSettings();

        [SerializeField]
        private AttackStateSettings _attack = new AttackStateSettings();

        [SerializeField]
        private DamageStateSettings _damage = new DamageStateSettings();

        [SerializeField]
        private DieStateSettings _die = new DieStateSettings();

        [Header("開発用")]
        [Tooltip("ON にすると棒立ちになり、どんな攻撃でも一撃で倒れる（DebugState）")]
        [SerializeField]
        private bool _debugMode;

        [Tooltip("状態が切り替わる距離と、攻撃の当たる範囲を描く（Editor と APP_DEBUG のビルドだけ）")]
        [SerializeField]
        private bool _drawDebug = true;

        private readonly ReactiveProperty<int> _currentHp = new(0);
        private readonly Subject<Unit> _died = new();
        private readonly StateMachine _machine = new StateMachine();

        private IDamageable _targetDamageable;

        private Transform[] _hitBones = System.Array.Empty<Transform>();
        private readonly Collider[] _overlapBuffer = new Collider[16];

        private float _lastHitTime = float.NegativeInfinity;
        private Vector3 _lastHitPoint;
        private float _nextAttackTime;
        private float _nextStaggerTime;

        public EnemyParameters Parameters => _parameters;
        public int MaxHp => _parameters.MaxHp;
        public ReadOnlyReactiveProperty<int> CurrentHp => _currentHp;
        public bool IsDead => _currentHp.Value <= 0;

        public Observable<Unit> Died => _died;

        public Observable<Unit> AttackImpact => _attackImpact;

        private readonly Subject<Unit> _attackImpact = new();

        public void NotifyAttackImpact()
        {
            _attackImpact.OnNext(Unit.Default);
        }

        [Header("エフェクト")]
        [Tooltip("溶けて消え始めたときに出す（灰と火の粉）")]
        [SerializeField]
        private GameObject _deathEffect;

        [SerializeField, Range(0.1f, 5f)]
        private float _deathEffectScale = 1f;

        public void SpawnDeathEffect()
        {
            if (_deathEffect == null) return;

            Vector3 center = transform.position + Vector3.up;
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                center = bounds.center;
            }

            FxSpawner.Spawn(_deathEffect, center, _deathEffectScale);
        }

        public int AttackPowerBonus { get; set; }
        public int DefencePowerBonus { get; set; }
        public int AttackPower => _parameters.AttackPower + AttackPowerBonus;
        public int DefencePower => _parameters.DefencePower + DefencePowerBonus;

        public bool IsInvincible { get; set; }
        public bool IsOneHitKill { get; set; }

        public bool IsDebugMode => _debugMode || DebugModeForAll;
        public IState CurrentState => _machine.CurrentState;

        public CharacterVisual Visual => _visual;
        public Transform Target => _target;
        public Vector3 HomePosition { get; private set; }

        public IState InitState { get; private set; }
        public IState SearchState { get; private set; }
        public IState PursueState { get; private set; }
        public IState AttackState { get; private set; }
        public IState DamageState { get; private set; }
        public IState DieState { get; private set; }
        public IState DebugState { get; private set; }

        public virtual bool CanStart => true;
        public virtual IState FirstState => SearchState;

        public float DistanceToTarget =>
            _target == null ? float.MaxValue : Vector3.Distance(transform.position, _target.position);

        public bool CanAttack => Time.time >= _nextAttackTime;

        public bool HasArrived =>
            _agent.isActiveAndEnabled && _agent.isOnNavMesh && !_agent.pathPending
            && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            DebugModeForAll = false;
        }

        protected virtual void Awake()
        {
            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }

            if (_visual == null)
            {
                _visual = GetComponentInChildren<CharacterVisual>();
            }

            _currentHp.Value = _parameters.MaxHp;
            HomePosition = transform.position;
            _hitBones = FindHitBones();

            InitState = new InitState(this);
            SearchState = new SearchState(this, _search);
            PursueState = new PursueState(this, _pursue);
            AttackState = new AttackState(this, _attack);
            DamageState = new DamageState(this, _damage);
            DieState = new DieState(this, _die);
            DebugState = new DebugState(this);

            CreateStates();
        }

        protected virtual void CreateStates()
        {
        }

        protected virtual void Start()
        {
            _machine.Change(InitState);
        }

        protected virtual void Update()
        {
            UpdateDebugMode();
            _machine.Update();
            DrawDebug();

            if (_visual != null)
            {
                float speed = _agent.isActiveAndEnabled ? _agent.velocity.magnitude : 0f;
                _visual.SetFloat(CharacterAnimatorParams.Speed, speed);
            }
        }

        private void UpdateDebugMode()
        {
            if (IsDead) return;

            if (IsDebugMode && CurrentState != DebugState)
            {
                ChangeState(DebugState);
            }
            else if (!IsDebugMode && CurrentState == DebugState)
            {
                ChangeState(InitState);
            }
        }

        public void ChangeState(IState next, bool reenter = false)
        {
            _machine.Change(next, reenter);
        }

        public int TakeDamage(int attackPower)
        {
            if (IsDead || attackPower <= 0) return 0;
            if (IsInvincible) return 0;

            int damage = IsOneHitKill ? _currentHp.Value : Mathf.Max(1, attackPower - DefencePower);
            damage = Mathf.Min(damage, _currentHp.Value);
            _currentHp.Value -= damage;

            if (_visual != null)
            {
                _visual.Flash(_damage.FlashColor, _damage.FlashDuration);
            }

            if (IsDead)
            {
                ChangeState(DieState);
                _died.OnNext(Unit.Default);
                _died.OnCompleted();
                OnDied();
                return damage;
            }

            OnDamaged(damage);
            return damage;
        }

        protected virtual void OnDamaged(int damage)
        {
            TryStagger();
        }

        protected virtual void OnDied()
        {
        }

        protected virtual bool IsInterruptible =>
            CurrentState != DieState && CurrentState != DebugState && CurrentState != InitState;

        protected bool TryStagger()
        {
            if (!IsInterruptible) return false;
            if (Time.time < _nextStaggerTime) return false;

            _nextStaggerTime = Time.time + _damage.StaggerCooldown;

            ChangeState(DamageState, reenter: true);
            return true;
        }

        public bool EnsureOnNavMesh()
        {
            if (!_agent.isActiveAndEnabled) return false;
            if (_agent.isOnNavMesh) return true;

            if (!NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas)) return false;

            _agent.Warp(hit.position);
            return _agent.isOnNavMesh;
        }

        public void FindTarget()
        {
            if (_target == null)
            {
                var player = FindFirstObjectByType<PlayerHealth>();
                if (player != null)
                {
                    _target = player.transform;
                }
            }

            _targetDamageable = _target != null ? _target.GetComponent<IDamageable>() : null;
        }

        public void MoveTo(Vector3 destination, float speed)
        {
            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            _agent.speed = speed;
            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void StopMoving()
        {
            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            _agent.isStopped = true;
            _agent.ResetPath();
        }

        public void FaceTarget(float turnSpeed = 8f)
        {
            if (_target == null) return;

            Vector3 direction = _target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f) return;

            var look = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        public void BeginAttackCooldown()
        {
            _nextAttackTime = Time.time + _parameters.AttackIntervalTime;
        }

        public bool IsTargetInReach(float range, float angle)
        {
            if (_target == null) return false;

            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > range) return false;
            if (toTarget.sqrMagnitude < 0.0001f) return true;

            return Vector3.Angle(transform.forward, toTarget) <= angle;
        }

        public bool TryHitTarget(AttackStateSettings settings)
        {
            if (_target == null) return false;

            bool isHit = false;
            Vector3 point = _target.position + Vector3.up;

            if (_hitBones.Length > 0)
            {
                foreach (var bone in _hitBones)
                {
                    if (!OverlapsTarget(bone.position, settings.HitRadius)) continue;

                    isHit = true;
                    point = bone.position;
                    break;
                }
            }
            else
            {
                isHit = IsTargetInReach(settings.HitRange, settings.HitAngle);
            }

            if (!isHit) return false;

            _lastHitTime = Time.time;
            _lastHitPoint = point;

            if (_targetDamageable != null && !_targetDamageable.IsDead)
            {
                _targetDamageable.TakeDamage(AttackPower);
            }

            return true;
        }

        private bool OverlapsTarget(Vector3 center, float radius)
        {
            int count = Physics.OverlapSphereNonAlloc(center, radius, _overlapBuffer, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var hit = _overlapBuffer[i].transform;
                if (hit == _target || hit.IsChildOf(_target)) return true;
            }

            return false;
        }

        private Transform[] FindHitBones()
        {
            if (_visual == null || _attack.HitBoneNames == null) return System.Array.Empty<Transform>();

            var bones = new System.Collections.Generic.List<Transform>();
            var children = _visual.GetComponentsInChildren<Transform>(true);

            foreach (var boneName in _attack.HitBoneNames)
            {
                if (string.IsNullOrEmpty(boneName)) continue;

                var bone = System.Array.Find(children, t => t.name == boneName);
                if (bone != null)
                {
                    bones.Add(bone);
                }
                else
                {
                    Debug.LogWarning($"{nameof(EnemyController)}: 当たり判定のボーン「{boneName}」が見つかりません。足元の扇形で判定します。", this);
                }
            }

            return bones.ToArray();
        }

        public void SetTrigger(int id)
        {
            if (_visual == null) return;

            _visual.SetTrigger(id);
        }

        public void SetBool(int id, bool value)
        {
            if (_visual == null) return;

            _visual.SetBool(id, value);
        }

        public void DisableBody()
        {
            StopMoving();
            _agent.enabled = false;

            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            _currentHp.Dispose();
            _died.Dispose();
            _attackImpact.Dispose();
        }

        private static readonly Color DetectColor = new Color(1f, 0.9f, 0.1f);
        private static readonly Color LoseColor = new Color(1f, 0.45f, 0.1f);
        private static readonly Color AttackRangeColor = new Color(1f, 0.15f, 0.15f);
        private static readonly Color PatrolColor = new Color(0.3f, 0.8f, 1f);
        private const float HitResultDuration = 0.3f;

        private void DrawDebug()
        {
            if (!_drawDebug || IsDead) return;

            bool isSearching = CurrentState == SearchState;
            bool isPursuing = CurrentState == PursueState || CurrentState == AttackState;
            Vector3 ground = transform.position + Vector3.up * 0.05f;

            DebugShapes.Circle(ground, _search.DetectRange, WithAlpha(DetectColor, isSearching ? 1f : 0.25f));
            DebugShapes.Circle(ground, _pursue.LoseRange, WithAlpha(LoseColor, isPursuing ? 1f : 0.25f));
            DebugShapes.Circle(ground, _pursue.AttackRange, WithAlpha(AttackRangeColor, isPursuing ? 1f : 0.25f));

            if (isSearching && (_search.PatrolPoints == null || _search.PatrolPoints.Length == 0))
            {
                DebugShapes.Circle(HomePosition + Vector3.up * 0.05f, _search.PatrolRadius, WithAlpha(PatrolColor, 0.5f));
            }

            DrawAttackDebug(ground);
        }

        private void DrawAttackDebug(Vector3 ground)
        {
            var attack = AttackState as AttackState;
            bool isAttacking = CurrentState == AttackState;
            bool isHitActive = isAttacking && attack != null && attack.IsHitActive;
            var activeColor = new Color(1f, 0.55f, 0.1f, 1f);

            if (_hitBones.Length > 0)
            {
                if (isAttacking)
                {
                    var color = isHitActive ? activeColor : new Color(1f, 0.55f, 0.1f, 0.3f);
                    foreach (var bone in _hitBones)
                    {
                        DebugShapes.WireSphere(bone.position, _attack.HitRadius, color);
                    }
                }
            }
            else
            {
                var color = isHitActive ? activeColor : new Color(1f, 0.3f, 0.3f, 0.3f);
                DebugShapes.Sector(ground, transform.forward, _attack.HitRange, _attack.HitAngle, color, isHitActive ? 12 : 0);
            }

            if (Time.time - _lastHitTime <= HitResultDuration)
            {
                var hitColor = new Color(1f, 0.1f, 0.1f, 1f);
                DebugShapes.WireSphere(_lastHitPoint, Mathf.Max(_attack.HitRadius, 0.3f), hitColor);
                DebugShapes.WireSphere(_lastHitPoint, Mathf.Max(_attack.HitRadius, 0.3f) * 0.95f, hitColor);
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawDebug || !Application.isPlaying || CurrentState == null) return;

            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f, CurrentState.GetType().Name);
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            DrawGizmoCircle(transform.position, _search.DetectRange, DetectColor);
            DrawGizmoCircle(transform.position, _pursue.LoseRange, LoseColor);
            DrawGizmoCircle(transform.position, _pursue.AttackRange, AttackRangeColor);

            if (_search.PatrolPoints == null || _search.PatrolPoints.Length == 0)
            {
                DrawGizmoCircle(transform.position, _search.PatrolRadius, PatrolColor);
            }
        }

        private static void DrawGizmoCircle(Vector3 center, float radius, Color color)
        {
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawWireDisc(center + Vector3.up * 0.05f, Vector3.up, radius);
        }
#endif
    }
}
