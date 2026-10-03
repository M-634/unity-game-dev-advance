using System;
using UnityEngine;

namespace DemoGame
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private InputReader _input;

        [SerializeField]
        private CharacterController _controller;

        [Tooltip("移動の基準になるカメラ。未設定ならワールド座標基準で動く")]
        [SerializeField]
        private Transform _cameraTransform;

        [Tooltip("見た目（モデル）のルート。ここだけを差し替えればモデルを交換できる")]
        [SerializeField]
        private Transform _visualRoot;

        [Header("移動")]
        [SerializeField, Range(0f, 10f)]
        private float _walkSpeed = 2f;

        [SerializeField, Range(0f, 20f)]
        private float _sprintSpeed = 5f;

        [SerializeField, Range(0.01f, 1f)]
        private float _rotationSmoothTime = 0.1f;

        [Header("ジャンプ・重力")]
        [SerializeField]
        private float _jumpHeight = 1.2f;

        [SerializeField]
        private float _gravity = -15f;

        private float _verticalVelocity;
        private float _rotationVelocity;
        private bool _jumpRequested;

        public bool IsGrounded => _controller != null && _controller.isGrounded;

        public float CurrentSpeed { get; private set; }

        // 箱を押している間など、移動を他のコンポーネントに任せるときに true にする
        // ロック中は移動・回転・ジャンプをしない（重力だけは掛け続ける）
        public bool IsMovementLocked { get; set; }

        // 火焔玉をチャージしている間など、歩けるが走らせたくないときに true にする
        public bool IsSprintLocked { get; set; }

        public Transform VisualRoot => _visualRoot;

        // TPS / FPS。設定するのはカメラ側（CameraModeSwitcher）。プレイヤーはカメラを知らない
        public CameraMode ViewMode { get; set; } = CameraMode.Tps;

        // 着地した瞬間。引数は着地直前の落下速度（m/s、正の値）
        public event Action<float> Landed;

        private bool _wasGrounded = true;

        private void Awake()
        {
            if (_controller == null)
            {
                _controller = GetComponent<CharacterController>();
            }

            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.Jumped += OnJumped;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.Jumped -= OnJumped;
            }
        }

        private void Update()
        {
            if (_input == null) return;

            ApplyGravity();

            if (IsMovementLocked)
            {
                _jumpRequested = false;
                return;
            }

            MoveAndRotate();
        }

        // ロック中に移動を任された側が、アニメーション用の速度を伝える
        public void SetLockedSpeed(float speed)
        {
            CurrentSpeed = speed;
        }

        // 入力をカメラ基準のワールド方向にしたもの（長さ 0〜1）
        public Vector3 GetInputDirection()
        {
            return _input == null ? Vector3.zero : ToWorldDirection(_input.Move);
        }

        private void OnJumped()
        {
            _jumpRequested = true;
        }

        private void ApplyGravity()
        {
            bool grounded = IsGrounded;
            if (grounded && !_wasGrounded && _verticalVelocity < 0f)
            {
                Landed?.Invoke(-_verticalVelocity);
            }
            _wasGrounded = grounded;

            if (IsGrounded)
            {
                // 接地中はわずかに下向きの力をかけ続ける。0 にすると接地判定がちらつく
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_jumpRequested)
                {
                    // v = sqrt(2 * g * h) から必要な初速を求める
                    _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                }
            }

            _jumpRequested = false;
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        private void MoveAndRotate()
        {
            Vector2 rawInput = _input.Move;
            Vector3 direction = ToWorldDirection(rawInput);

            float targetSpeed = _input.IsSprinting && !IsSprintLocked ? _sprintSpeed : _walkSpeed;
            CurrentSpeed = direction.sqrMagnitude > 0.001f ? targetSpeed : 0f;

            if (ViewMode == CameraMode.Fps)
            {
                // FPS では体を常にカメラの水平方向へ向ける。横移動しても向きは変えない
                FaceCameraYaw();
            }
            else if (direction.sqrMagnitude > 0.001f)
            {
                RotateVisual(direction);
            }

            Vector3 velocity = direction * CurrentSpeed;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);
        }

        private Vector3 ToWorldDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.001f)
            {
                return Vector3.zero;
            }

            if (_cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y).normalized;
            }

            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;

            return (forward.normalized * input.y + right.normalized * input.x).normalized;
        }

        private void FaceCameraYaw()
        {
            if (_cameraTransform == null) return;

            _visualRoot.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
        }

        private void RotateVisual(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(_visualRoot.eulerAngles.y, targetAngle, ref _rotationVelocity, _rotationSmoothTime);

            _visualRoot.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
