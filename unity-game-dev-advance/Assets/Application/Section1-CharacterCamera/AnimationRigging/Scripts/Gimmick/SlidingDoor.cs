using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Section1.AnimationRigging
{
    // 決まった距離だけ滑って開く扉。開ける／閉めるだけで、いつ開くかは知らない（TimedDoor・EncounterRoom が決める）
    // Scene には「閉じた位置」で置く
    public class SlidingDoor : MonoBehaviour
    {
        [Tooltip("動かす対象。未設定なら自分を動かす")]
        [SerializeField]
        private Transform _door;

        [Tooltip("閉じた位置から開いた位置までのずれ（ワールド座標）")]
        [SerializeField]
        private Vector3 _openOffset = new Vector3(0f, 3f, 0f);

        [SerializeField, Range(0.1f, 5f)]
        private float _duration = 1f;

        [SerializeField]
        private bool _startsOpen;

        [Tooltip("閉じるとき、扉の範囲にプレイヤーがいる間は止まって待つ（挟まない）")]
        [SerializeField]
        private bool _waitWhilePlayerInside = true;

        [Tooltip("挟み判定に使う範囲。未設定なら扉のコライダー")]
        [SerializeField]
        private Collider _blockArea;

        private CancellationTokenSource _moveCts;
        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private readonly Collider[] _overlapBuffer = new Collider[8];

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (_door == null)
            {
                _door = transform;
            }

            if (_blockArea == null)
            {
                _blockArea = _door.GetComponent<Collider>();
            }

            _closedPosition = _door.position;
            _openPosition = _closedPosition + _openOffset;

            if (_startsOpen)
            {
                _door.position = _openPosition;
                IsOpen = true;
            }
        }

        public UniTask OpenAsync(CancellationToken token = default)
        {
            IsOpen = true;
            return MoveAsync(_openPosition, false, token);
        }

        public UniTask CloseAsync(CancellationToken token = default)
        {
            IsOpen = false;
            return MoveAsync(_closedPosition, _waitWhilePlayerInside, token);
        }

        private async UniTask MoveAsync(Vector3 target, bool waitForPlayer, CancellationToken token)
        {
            // 前の動き（開く途中に閉じる、など）は止めてから動かす
            _moveCts?.Cancel();
            _moveCts?.Dispose();
            _moveCts = CancellationTokenSource.CreateLinkedTokenSource(token, destroyCancellationToken);
            var ct = _moveCts.Token;

            float speed = Vector3.Distance(_closedPosition, _openPosition) / _duration;

            while ((_door.position - target).sqrMagnitude > 0.0001f)
            {
                if (waitForPlayer && IsPlayerInside())
                {
                    // 閉じかけでもいったん止まり、プレイヤーが抜けるのを待つ
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                    continue;
                }

                _door.position = Vector3.MoveTowards(_door.position, target, speed * Time.deltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            _door.position = target;
        }

        private void OnDestroy()
        {
            _moveCts?.Cancel();
            _moveCts?.Dispose();
        }

        // 閉じきった状態での扉の範囲にプレイヤーがいるか
        private bool IsPlayerInside()
        {
            if (_blockArea == null) return false;

            var bounds = _blockArea.bounds;
            Vector3 center = bounds.center - _door.position + _closedPosition;
            int count = Physics.OverlapBoxNonAlloc(center, bounds.extents, _overlapBuffer, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                if (_overlapBuffer[i].CompareTag("Player")) return true;
            }

            return false;
        }
    }
}
