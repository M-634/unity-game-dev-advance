using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace DemoGame
{
    // 箱を押している間のプレイヤー。移動を PlayerController から預かり、箱と一緒に動かす
    // 両手は Animation Rigging（Two Bone IK）で箱の面に添える。Rig の Weight で FK ⇔ IK を補間する
    public class PlayerBoxPusher : MonoBehaviour
    {
        [SerializeField]
        private PlayerController _controller;

        [SerializeField]
        private CharacterController _characterController;

        [SerializeField]
        private PlayerAttack _attack;

        [Header("押す")]
        [SerializeField, Range(0.1f, 5f)]
        private float _pushSpeed = 1.5f;

        [Tooltip("押し始めたとき、面からどれだけ離れて立つか（CharacterController の半径に足す）")]
        [SerializeField, Range(0f, 1f)]
        private float _standOffset = 0.2f;

        [Tooltip("入力が押す向きと逆にこれ以上向いたら、離したとみなす（-1〜0）")]
        [SerializeField, Range(-1f, 0f)]
        private float _releaseDot = -0.5f;

        [Header("両手の IK（モデル側の Rig）")]
        [Tooltip("両手の Two Bone IK をまとめた Rig。普段は Weight 0")]
        [SerializeField]
        private Rig _handRig;

        [SerializeField]
        private Transform _leftHandTarget;

        [SerializeField]
        private Transform _rightHandTarget;

        [Tooltip("手を添える高さ（足元から）")]
        [SerializeField, Range(0.3f, 1.5f)]
        private float _handHeight = 0.85f;

        [Tooltip("面の中心から左右にどれだけ開くか")]
        [SerializeField, Range(0f, 0.6f)]
        private float _handSpread = 0.25f;

        [SerializeField, Range(0.01f, 1f)]
        private float _rigBlendTime = 0.2f;

        [Header("押している間は隠すもの")]
        [SerializeField]
        private GameObject _weapon;

        private PressBox _box;
        private PlayerInteractor _interactor;
        private Vector3 _pushDirection;
        private float _rigTargetWeight;

        public bool IsPushing => _box != null;

        private void Awake()
        {
            if (_controller == null) _controller = GetComponent<PlayerController>();
            if (_characterController == null) _characterController = GetComponent<CharacterController>();
            if (_attack == null) _attack = GetComponent<PlayerAttack>();

            if (_handRig != null)
            {
                _handRig.weight = 0f;
            }
        }

        public void Begin(PressBox box, PlayerInteractor interactor)
        {
            if (IsPushing || box == null) return;
            if (!_characterController.isGrounded) return;

            _box = box;
            _interactor = interactor;
            _box.SetPusher(this);
            _interactor.Hold(box);

            _box.GetPushFace(transform.position, out var faceCenter, out var normal, out _);
            _pushDirection = -normal;

            // 面の正面に立たせ、面の方を向かせる
            Vector3 standPosition = faceCenter + normal * (_characterController.radius + _standOffset);
            standPosition.y = transform.position.y;
            Teleport(standPosition);
            _controller.VisualRoot.rotation = Quaternion.LookRotation(_pushDirection, Vector3.up);

            _controller.IsMovementLocked = true;
            _attack.IsLocked = true;
            SetWeaponVisible(false);
            _rigTargetWeight = 1f;
        }

        public void End()
        {
            if (!IsPushing) return;

            _box.SetPusher(null);
            _interactor.Release();
            _box = null;
            _interactor = null;

            _controller.IsMovementLocked = false;
            _controller.SetLockedSpeed(0f);
            _attack.IsLocked = false;
            SetWeaponVisible(true);
            _rigTargetWeight = 0f;
        }

        private void Update()
        {
            BlendRig();

            if (!IsPushing) return;

            if (!_characterController.isGrounded)
            {
                End();
                return;
            }

            Vector3 input = _controller.GetInputDirection();
            float along = Vector3.Dot(input, _pushDirection);

            if (input.sqrMagnitude > 0.01f && along < _releaseDot)
            {
                End();
                return;
            }

            float moved = 0f;

            if (along > 0.1f)
            {
                moved = _box.Move(_pushDirection, _pushSpeed * along * Time.deltaTime);

                // 箱を動かした結果を物理側へ反映してから、プレイヤーを動かす（古い位置の箱にぶつからないように）
                Physics.SyncTransforms();
            }

            // 接地判定を保つため、わずかに下向きにも動かす
            _characterController.Move(_pushDirection * moved + Vector3.down * (2f * Time.deltaTime));
            _controller.SetLockedSpeed(moved / Mathf.Max(Time.deltaTime, 1e-5f));

            // Rig はアニメーションの評価中に解かれるので、ターゲットは Update のうちに置く
            PlaceHandTargets();
        }

        private void PlaceHandTargets()
        {
            if (_leftHandTarget == null || _rightHandTarget == null) return;

            _box.GetPushFace(transform.position, out var faceCenter, out var normal, out float halfWidth);

            Vector3 right = Vector3.Cross(Vector3.up, -normal).normalized;
            float spread = Mathf.Min(_handSpread, halfWidth * 0.9f);
            Vector3 height = Vector3.up * (transform.position.y + _handHeight - faceCenter.y);

            Quaternion rotation = Quaternion.LookRotation(-normal, Vector3.up);

            _leftHandTarget.SetPositionAndRotation(faceCenter + height - right * spread, rotation);
            _rightHandTarget.SetPositionAndRotation(faceCenter + height + right * spread, rotation);
        }

        private void BlendRig()
        {
            if (_handRig == null) return;

            _handRig.weight = Mathf.MoveTowards(_handRig.weight, _rigTargetWeight, Time.deltaTime / _rigBlendTime);
        }

        private void Teleport(Vector3 position)
        {
            // CharacterController は有効なまま位置を書き換えると戻されるので、いったん止める
            _characterController.enabled = false;
            transform.position = position;
            _characterController.enabled = true;
        }

        private void SetWeaponVisible(bool visible)
        {
            if (_weapon != null)
            {
                _weapon.SetActive(visible);
            }
        }

        private void OnDisable()
        {
            End();
        }
    }
}
