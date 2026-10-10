using Cinemachine;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Section1.AnimationRigging
{
    // TPS（FreeLook）と FPS（POV）を Priority の入れ替えで切り替える
    // プレイヤーはカメラを知らない。ここが PlayerController.ViewMode を設定する
    public class CameraModeSwitcher : MonoBehaviour
    {
        [SerializeField]
        private InputReader _input;

        [SerializeField]
        private PlayerController _player;

        [SerializeField]
        private CinemachineFreeLook _tpsCamera;

        [SerializeField]
        private CinemachineVirtualCamera _fpsCamera;

        [Tooltip("Look を受け取る InputProvider。XY Axis が空なら InputReader の Look をつなぐ")]
        [SerializeField]
        private CinemachineInputProvider[] _inputProviders;

        [SerializeField]
        private int _livePriority = 20;

        [SerializeField]
        private int _standbyPriority = 0;

        [Tooltip("FPS で見上げ・見下ろしできる角度")]
        [SerializeField, Range(10f, 89f)]
        private float _fpsPitchLimit = 70f;

        [Tooltip("TPS に戻したときのカメラ位置（追従先からの相対。中段の Orbit に合わせる）")]
        [SerializeField]
        private Vector3 _tpsReturnOffset = new Vector3(0f, 2.5f, -3f);

        private readonly ReactiveProperty<CameraMode> _mode = new(CameraMode.Tps);
        private CinemachinePOV _pov;
        private Transform _outputCamera;

        public ReadOnlyReactiveProperty<CameraMode> Mode => _mode;

        private void Awake()
        {
            if (_fpsCamera != null)
            {
                _pov = _fpsCamera.GetCinemachineComponent<CinemachinePOV>();
            }

            if (_pov != null)
            {
                _pov.m_VerticalAxis.m_MinValue = -_fpsPitchLimit;
                _pov.m_VerticalAxis.m_MaxValue = _fpsPitchLimit;
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.SwitchViewPressed += OnSwitchViewPressed;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.SwitchViewPressed -= OnSwitchViewPressed;
            }
        }

        private void Start()
        {
            var main = Camera.main;
            _outputCamera = main != null ? main.transform : null;

            ConnectInputProviders();
            Apply(CameraMode.Tps, syncDirection: false);
        }

        private void Update()
        {
            // 箱を押し始めたら TPS に戻す（押している間は FPS にしない）
            if (_mode.Value == CameraMode.Fps && _player != null && _player.IsMovementLocked)
            {
                Apply(CameraMode.Tps, syncDirection: true);
            }
        }

        private void OnSwitchViewPressed()
        {
            if (_player != null && _player.IsMovementLocked) return;

            Apply(_mode.Value == CameraMode.Tps ? CameraMode.Fps : CameraMode.Tps, syncDirection: true);
        }

        private void ConnectInputProviders()
        {
            if (_input == null || _input.LookAction == null || _inputProviders == null) return;

            foreach (var provider in _inputProviders)
            {
                if (provider == null || provider.XYAxis != null) continue;

                provider.XYAxis = InputActionReference.Create(_input.LookAction);
            }
        }

        private void Apply(CameraMode mode, bool syncDirection)
        {
            if (syncDirection && _outputCamera != null)
            {
                // 切り替え先のカメラに、今の向きを写してから入れ替える（視線が飛ばないように）
                float yaw = _outputCamera.eulerAngles.y;
                float pitch = Mathf.DeltaAngle(0f, _outputCamera.eulerAngles.x);

                if (mode == CameraMode.Fps && _pov != null)
                {
                    _pov.m_HorizontalAxis.Value = yaw;
                    _pov.m_VerticalAxis.Value = Mathf.Clamp(pitch, -_fpsPitchLimit, _fpsPitchLimit);
                }
                else if (mode == CameraMode.Tps && _tpsCamera != null && _tpsCamera.Follow != null)
                {
                    Vector3 target = _tpsCamera.Follow.position;
                    Vector3 position = target + Quaternion.Euler(0f, yaw, 0f) * _tpsReturnOffset;
                    _tpsCamera.m_YAxis.Value = 0.5f;
                    _tpsCamera.ForceCameraPosition(position, Quaternion.LookRotation(target - position));
                }
            }

            if (_tpsCamera != null) _tpsCamera.Priority = mode == CameraMode.Tps ? _livePriority : _standbyPriority;
            if (_fpsCamera != null) _fpsCamera.Priority = mode == CameraMode.Fps ? _livePriority : _standbyPriority;

            if (_player != null)
            {
                _player.ViewMode = mode;
            }

            _mode.Value = mode;
        }

        private void OnDestroy()
        {
            _mode.Dispose();
        }
    }
}
