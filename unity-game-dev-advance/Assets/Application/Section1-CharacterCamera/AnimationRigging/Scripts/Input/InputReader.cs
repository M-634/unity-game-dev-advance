using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Section1.AnimationRigging
{
    public class InputReader : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string CutsceneMapName = "Cutscene";

        [SerializeField]
        private InputActionAsset _inputActions;

        public Vector2 Move { get; private set; }

        public Vector2 Look { get; private set; }

        public bool IsSprinting { get; private set; }

        public event Action Jumped;

        // 押した瞬間。チャージは押してから離すまでの時間で数えるので、離した瞬間も流す
        public event Action Attacked;

        public event Action AttackReleased;

        public bool IsAttackHeld => _attackAction != null && _attackAction.IsPressed();

        public event Action Interacted;

        public event Action SwitchViewPressed;

        // 演出中だけ有効（Cutscene マップ）
        public event Action SkipPressed;

        public bool IsSkipHeld => _skipAction != null && _skipAction.IsPressed();

        // Cinemachine の InputProvider に渡すため、Look アクションそのものを公開する
        public InputAction LookAction => _lookAction;

        private InputActionMap _playerMap;
        private InputActionMap _cutsceneMap;
        private InputAction _skipAction;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _attackAction;
        private InputAction _interactAction;
        private InputAction _switchViewAction;

        private void Awake()
        {
            if (_inputActions == null)
            {
                Debug.LogError($"{nameof(InputReader)}: InputActionAsset が設定されていません。", this);
                return;
            }

            _playerMap = _inputActions.FindActionMap(PlayerMapName, throwIfNotFound: true);

            _moveAction = _playerMap.FindAction("Move", throwIfNotFound: true);
            _lookAction = _playerMap.FindAction("Look", throwIfNotFound: true);
            _jumpAction = _playerMap.FindAction("Jump", throwIfNotFound: true);
            _sprintAction = _playerMap.FindAction("Sprint", throwIfNotFound: true);
            _attackAction = _playerMap.FindAction("Attack", throwIfNotFound: true);
            _interactAction = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _switchViewAction = _playerMap.FindAction("SwitchView", throwIfNotFound: true);

            _cutsceneMap = _inputActions.FindActionMap(CutsceneMapName, throwIfNotFound: true);
            _skipAction = _cutsceneMap.FindAction("Skip", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            if (_playerMap == null) return;

            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLook;
            _sprintAction.performed += OnSprint;
            _sprintAction.canceled += OnSprint;

            _jumpAction.performed += OnJump;
            _attackAction.performed += OnAttack;
            _attackAction.canceled += OnAttackReleased;
            _interactAction.performed += OnInteract;
            _switchViewAction.performed += OnSwitchView;
            _skipAction.performed += OnSkip;

            _playerMap.Enable();
        }

        private void OnDisable()
        {
            if (_playerMap == null) return;

            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
            _lookAction.performed -= OnLook;
            _lookAction.canceled -= OnLook;
            _sprintAction.performed -= OnSprint;
            _sprintAction.canceled -= OnSprint;

            _jumpAction.performed -= OnJump;
            _attackAction.performed -= OnAttack;
            _attackAction.canceled -= OnAttackReleased;
            _interactAction.performed -= OnInteract;
            _switchViewAction.performed -= OnSwitchView;
            _skipAction.performed -= OnSkip;

            _cutsceneMap.Disable();

            _playerMap.Disable();

            // 押しっぱなしのまま無効化されると入力が残り続けるため、ここで消す
            Move = Vector2.zero;
            Look = Vector2.zero;
            IsSprinting = false;
        }

        public void SetInputEnabled(bool isEnabled)
        {
            if (_playerMap == null) return;

            if (isEnabled)
            {
                _playerMap.Enable();
            }
            else
            {
                _playerMap.Disable();
                Move = Vector2.zero;
                Look = Vector2.zero;
                IsSprinting = false;
            }
        }

        // 演出中は Player マップを止め、Cutscene マップ（スキップ）だけを有効にする
        public void SetCutsceneMode(bool isCutscene)
        {
            if (_cutsceneMap == null) return;

            if (isCutscene)
            {
                SetInputEnabled(false);
                _cutsceneMap.Enable();
            }
            else
            {
                _cutsceneMap.Disable();
                SetInputEnabled(true);
            }
        }

        private void OnMove(InputAction.CallbackContext context) => Move = context.ReadValue<Vector2>();
        private void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();
        private void OnSprint(InputAction.CallbackContext context) => IsSprinting = context.ReadValueAsButton();

        private void OnJump(InputAction.CallbackContext context) => Jumped?.Invoke();
        private void OnAttack(InputAction.CallbackContext context) => Attacked?.Invoke();
        private void OnAttackReleased(InputAction.CallbackContext context) => AttackReleased?.Invoke();
        private void OnInteract(InputAction.CallbackContext context) => Interacted?.Invoke();
        private void OnSwitchView(InputAction.CallbackContext context) => SwitchViewPressed?.Invoke();
        private void OnSkip(InputAction.CallbackContext context) => SkipPressed?.Invoke();
    }
}
