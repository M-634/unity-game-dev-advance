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

        public bool IsMovementLocked { get; set; }

        public bool IsSprintLocked { get; set; }

        public Transform VisualRoot => _visualRoot;

        public CameraMode ViewMode { get; set; } = CameraMode.Tps;

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

        public void SetLockedSpeed(float speed)
        {
            CurrentSpeed = speed;
        }

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
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_jumpRequested)
                {
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
