using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Week01
{
    // 杖を持った右腕を、対象（篝火の炎・灯火・蔦）へ伸ばすアクション。設計：doc/demo-game/10-level-design.md §4.2・§4.3
    // 右腕だけの Rig（Rig_Reach。Two Bone IK）の Weight を 0 → 1 → 0 と動かし、手が届いた瞬間に onReached を呼ぶ
    // 箱押しの Rig_Hands とは別の Rig にしている（あちらは毎フレーム自分の Weight を書くため）
    public class PlayerStaffReach : MonoBehaviour
    {
        [SerializeField]
        private PlayerController _controller;

        [SerializeField]
        private PlayerAttack _attack;

        [Tooltip("右腕だけの Rig（モデル側）。普段は Weight 0")]
        [SerializeField]
        private Rig _rig;

        [Tooltip("右手の IK のターゲット")]
        [SerializeField]
        private Transform _handTarget;

        [Tooltip("腕を伸ばしきるまで（秒）")]
        [SerializeField, Range(0.05f, 1f)]
        private float _reachSeconds = 0.3f;

        [Tooltip("伸ばしきったまま止める時間（秒）。この始めに火が移る")]
        [SerializeField, Range(0f, 1f)]
        private float _holdSeconds = 0.25f;

        [Tooltip("腕を戻すまで（秒）")]
        [SerializeField, Range(0.05f, 1f)]
        private float _returnSeconds = 0.3f;

        public bool IsReaching { get; private set; }

        private void Awake()
        {
            if (_controller == null) _controller = GetComponent<PlayerController>();
            if (_attack == null) _attack = GetComponent<PlayerAttack>();

            if (_rig != null)
            {
                _rig.weight = 0f;
            }
        }

        // point へ腕を伸ばし、届いたら onReached。Rig が無ければすぐ onReached
        public void Reach(Vector3 point, Action onReached)
        {
            if (IsReaching) return;

            if (_rig == null || _handTarget == null)
            {
                onReached?.Invoke();
                return;
            }

            ReachAsync(point, onReached, destroyCancellationToken).SuppressCancellationThrow().Forget();
        }

        private async UniTask ReachAsync(Vector3 point, Action onReached, CancellationToken ct)
        {
            IsReaching = true;
            Lock(true);

            try
            {
                // 対象の方を向く（水平だけ）
                if (_controller != null)
                {
                    Vector3 toPoint = point - _controller.VisualRoot.position;
                    toPoint.y = 0f;
                    if (toPoint.sqrMagnitude > 0.001f)
                    {
                        _controller.VisualRoot.rotation = Quaternion.LookRotation(toPoint);
                    }
                }

                _handTarget.position = point;

                await BlendAsync(0f, 1f, _reachSeconds, ct);
                onReached?.Invoke();

                if (_holdSeconds > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_holdSeconds), cancellationToken: ct);
                }

                await BlendAsync(1f, 0f, _returnSeconds, ct);
            }
            finally
            {
                if (_rig != null)
                {
                    _rig.weight = 0f;
                }

                Lock(false);
                IsReaching = false;
            }
        }

        private async UniTask BlendAsync(float from, float to, float seconds, CancellationToken ct)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                _rig.weight = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / seconds));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            _rig.weight = to;
        }

        // 腕を伸ばしている間は、動かない・攻撃しない
        private void Lock(bool isLocked)
        {
            if (_controller != null)
            {
                _controller.IsMovementLocked = isLocked;
                _controller.SetLockedSpeed(0f);
            }

            if (_attack != null)
            {
                _attack.IsLocked = isLocked;
            }
        }

        // 働きかける側（篝火・灯火・蔦）が使う。腕を伸ばせないプレイヤーなら、すぐ onReached
        public static void ReachOrRun(PlayerInteractor interactor, Vector3 point, Action onReached)
        {
            var reach = interactor != null ? interactor.GetComponentInParent<PlayerStaffReach>() : null;
            if (reach == null)
            {
                onReached?.Invoke();
                return;
            }

            reach.Reach(point, onReached);
        }

        public static bool IsBusy(PlayerInteractor interactor)
        {
            var reach = interactor != null ? interactor.GetComponentInParent<PlayerStaffReach>() : null;
            return reach != null && reach.IsReaching;
        }
    }
}
