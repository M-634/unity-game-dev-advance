using R3;
using UnityEngine;

namespace DemoGame
{
    public readonly struct GameResult
    {
        public readonly bool IsCleared;
        public readonly float ElapsedTime;

        public GameResult(bool isCleared, float elapsedTime)
        {
            IsCleared = isCleared;
            ElapsedTime = elapsedTime;
        }
    }

    public class InGameController : MonoBehaviour
    {
        [SerializeField]
        private InputReader _input;

        [Tooltip("クリアの条件。ClearBeacon があれば「光に触れたら」、無ければ「ボスを倒したら」")]
        [SerializeField]
        private BossController _boss;

        [SerializeField]
        private ClearBeacon _clearBeacon;

        [Tooltip("オフなら、BeginTimer() が呼ばれるまで経過時間を数えない（導入演出のあとから数える）")]
        [SerializeField]
        private bool _beginTimerOnStart = true;

        private readonly Subject<GameResult> _finished = new();
        private bool _isTimerRunning;
        private readonly CompositeDisposable _disposables = new();

        public Observable<GameResult> Finished => _finished;
        public bool IsFinished { get; private set; }
        public float ElapsedTime { get; private set; }

        private void OnEnable()
        {
            if (_clearBeacon != null)
            {
                _clearBeacon.Touched.Subscribe(_ => Finish(true)).AddTo(_disposables);
            }
            else if (_boss != null)
            {
                _boss.Defeated.Subscribe(_ => Finish(true)).AddTo(_disposables);
            }
        }

        private void OnDisable()
        {
            _disposables.Clear();
        }

        private void Start()
        {
            if (_beginTimerOnStart)
            {
                BeginTimer();
            }
        }

        public void BeginTimer()
        {
            _isTimerRunning = true;
        }

        private void Update()
        {
            if (IsFinished || !_isTimerRunning) return;

            ElapsedTime += Time.deltaTime;
        }

        private void Finish(bool isCleared)
        {
            if (IsFinished) return;

            IsFinished = true;

            if (_input != null)
            {
                _input.SetInputEnabled(false);
            }

            _finished.OnNext(new GameResult(isCleared, ElapsedTime));
            _finished.OnCompleted();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _finished.Dispose();
        }
    }
}
