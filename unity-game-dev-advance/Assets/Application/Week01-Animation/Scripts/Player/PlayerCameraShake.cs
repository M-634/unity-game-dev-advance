using Cinemachine;
using R3;
using UnityEngine;

namespace Week01
{
    // 被弾と、高いところからの着地で画面を揺らす（Cinemachine Impulse）
    // 揺れを受け取るのは各カメラの CinemachineImpulseListener
    public class PlayerCameraShake : MonoBehaviour
    {
        [SerializeField]
        private CinemachineImpulseSource _source;

        [SerializeField]
        private PlayerHealth _health;

        [SerializeField]
        private PlayerController _controller;

        [Header("被弾")]
        [SerializeField, Range(0f, 2f)]
        private float _hitForce = 0.3f;

        [Header("着地")]
        [Tooltip("この落下速度（m/s）を超えて着地したら揺らす")]
        [SerializeField, Range(0f, 30f)]
        private float _landSpeedThreshold = 6f;

        [SerializeField, Range(0f, 1f)]
        private float _landForcePerSpeed = 0.05f;

        [SerializeField, Range(0f, 2f)]
        private float _landForceMax = 0.6f;

        private readonly CompositeDisposable _disposables = new();
        private int _lastHp;

        private void Awake()
        {
            if (_source == null) _source = GetComponent<CinemachineImpulseSource>();
            if (_health == null) _health = GetComponent<PlayerHealth>();
            if (_controller == null) _controller = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _lastHp = _health.CurrentHp.CurrentValue;
                _health.CurrentHp.Subscribe(OnHpChanged).AddTo(_disposables);
            }

            if (_controller != null)
            {
                _controller.Landed += OnLanded;
            }
        }

        private void OnDisable()
        {
            _disposables.Clear();

            if (_controller != null)
            {
                _controller.Landed -= OnLanded;
            }
        }

        private void OnHpChanged(int hp)
        {
            if (hp < _lastHp)
            {
                Shake(_hitForce);
            }

            _lastHp = hp;
        }

        private void OnLanded(float fallSpeed)
        {
            if (fallSpeed < _landSpeedThreshold) return;

            Shake(Mathf.Min(_landForceMax, fallSpeed * _landForcePerSpeed));
        }

        private void Shake(float force)
        {
            if (_source == null || force <= 0f) return;

            _source.GenerateImpulseWithForce(force);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
