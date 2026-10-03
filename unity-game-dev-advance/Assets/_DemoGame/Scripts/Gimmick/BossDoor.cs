using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    // 扉は封印（BossDoorSeal）を購読しているだけ。灯の側は扉の存在を知らない。
    // 封印が無いときは、GameProgress の「灯が全部点いた」で開く
    public class BossDoor : MonoBehaviour
    {
        [Tooltip("紋様が全部消えたら開く")]
        [SerializeField]
        private BossDoorSeal _seal;

        [Tooltip("封印が無いときに使う")]
        [SerializeField]
        private GameProgress _progress;

        [Tooltip("動かす対象。未設定なら自分を動かす")]
        [SerializeField]
        private Transform _doorTransform;

        [SerializeField]
        private Vector3 _openOffset = new Vector3(0f, -5.2f, 0f);

        [SerializeField, Range(0.1f, 10f)]
        private float _openDuration = 1.6f;

        private readonly Subject<Unit> _opened = new();
        private readonly CompositeDisposable _disposables = new();

        public Observable<Unit> Opened => _opened;
        public bool IsOpen { get; private set; }

        // 開ききった
        public bool IsFullyOpen { get; private set; }

        private Vector3 _closedPosition;

        private void Awake()
        {
            if (_doorTransform == null)
            {
                _doorTransform = transform;
            }

            _closedPosition = _doorTransform.position;
        }

        private void OnEnable()
        {
            if (_seal != null)
            {
                _seal.Broken.Subscribe(_ => Open()).AddTo(_disposables);
                return;
            }

            if (_progress == null)
            {
                Debug.LogError($"{nameof(BossDoor)}: BossDoorSeal も GameProgress も設定されていません。", this);
                return;
            }

            _progress.AllLit.Subscribe(_ => Open()).AddTo(_disposables);
        }

        private void OnDisable()
        {
            _disposables.Clear();
        }

        public void Open()
        {
            if (IsOpen) return;

            IsOpen = true;
            OpenAsync().Forget();
        }

        private async UniTask OpenAsync()
        {
            Vector3 from = _closedPosition;
            Vector3 to = _closedPosition + _openOffset;
            float elapsed = 0f;

            var token = this.GetCancellationTokenOnDestroy();

            while (elapsed < _openDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _openDuration);
                _doorTransform.position = Vector3.Lerp(from, to, t * t * (3f - 2f * t));

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _doorTransform.position = to;
            IsFullyOpen = true;
            _opened.OnNext(Unit.Default);
            _opened.OnCompleted();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _opened.Dispose();
        }
    }
}
