using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Week01
{
    // 入口を閉じて戦闘を始め、全滅したら入口を開ける
    //   _beginOnEnter ON ：部屋の中のトリガーにプレイヤーが入ったら始まる
    //   _beginOnEnter OFF：Begin() で始まる（灯火 2：灯火に火をつけたら。InteractLanternTrigger から）
    public class EncounterRoom : MonoBehaviour
    {
        [SerializeField]
        private EncounterArea _encounter;

        [Tooltip("戦闘中に閉じる入口。未設定なら閉じない")]
        [SerializeField]
        private SlidingDoor _entrance;

        [Tooltip("部屋に入っただけで始めるか")]
        [SerializeField]
        private bool _beginOnEnter = true;

        private readonly CompositeDisposable _disposables = new();
        private readonly Subject<Unit> _cleared = new();
        private bool _isStarted;

        // 全滅して入口が開き始めたとき
        public Observable<Unit> Cleared => _cleared;

        public bool IsStarted => _isStarted;

        private void OnTriggerEnter(Collider other)
        {
            if (!_beginOnEnter) return;
            if (!other.CompareTag("Player")) return;

            Begin();
        }

        public void Begin()
        {
            if (_isStarted) return;
            if (_encounter == null) return;

            _isStarted = true;

            _encounter.Cleared.Subscribe(_ => OnCleared()).AddTo(_disposables);

            if (_entrance != null)
            {
                _entrance.CloseAsync().SuppressCancellationThrow().Forget();
            }

            _encounter.Begin();
        }

        private void OnCleared()
        {
            if (_entrance != null)
            {
                _entrance.OpenAsync().SuppressCancellationThrow().Forget();
            }

            _cleared.OnNext(Unit.Default);
            _cleared.OnCompleted();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _cleared.Dispose();
        }
    }
}
