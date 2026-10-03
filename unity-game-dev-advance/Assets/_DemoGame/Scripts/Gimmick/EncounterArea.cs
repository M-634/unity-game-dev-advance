using System.Collections.Generic;
using R3;
using UnityEngine;

namespace DemoGame
{
    // 敵を出して、全滅したことを知らせる場所。
    // 誰が始めるか、終わったあと何が起きるかは知らない。
    public class EncounterArea : MonoBehaviour
    {
        [SerializeField]
        private GameObject _enemyPrefab;

        [Tooltip("敵を出す位置。ここに置いた数だけ出る")]
        [SerializeField]
        private Transform[] _spawnPoints;

        [Tooltip("敵が出てくるときのエフェクト（黒い霧。doc/demo-game/12-vfx.md #26）")]
        [SerializeField]
        private GameObject _spawnEffect;

        private readonly Subject<Unit> _started = new();
        private readonly Subject<Unit> _cleared = new();
        private readonly ReactiveProperty<int> _aliveCount = new(0);
        private readonly CompositeDisposable _disposables = new();
        private readonly List<EnemyController> _spawned = new();

        public Observable<Unit> Started => _started;
        public Observable<Unit> Cleared => _cleared;
        public ReadOnlyReactiveProperty<int> AliveCount => _aliveCount;

        public bool IsRunning { get; private set; }
        public bool IsCleared { get; private set; }

        public void Begin()
        {
            if (IsRunning || IsCleared) return;

            if (_enemyPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogWarning($"{nameof(EncounterArea)}: 敵プレハブか出現位置が未設定です。素通りさせます。", this);
                MarkCleared();
                return;
            }

            IsRunning = true;
            _started.OnNext(Unit.Default);

            foreach (var point in _spawnPoints)
            {
                if (point == null) continue;

                FxSpawner.Spawn(_spawnEffect, point.position);
                var enemy = Instantiate(_enemyPrefab, point.position, point.rotation);

                // テンプレートをシーンに非アクティブで置いている場合があるので、必ず起こす
                enemy.SetActive(true);

                if (!enemy.TryGetComponent(out EnemyController controller))
                {
                    Debug.LogWarning($"{nameof(EncounterArea)}: 敵プレハブに {nameof(EnemyController)} がありません。", this);
                    continue;
                }

                _spawned.Add(controller);
                controller.Died.Subscribe(_ => OnEnemyDied(controller)).AddTo(_disposables);
            }

            _aliveCount.Value = _spawned.Count;

            if (_aliveCount.Value == 0)
            {
                MarkCleared();
            }
        }

        private void OnEnemyDied(EnemyController enemy)
        {
            _spawned.Remove(enemy);
            _aliveCount.Value = _spawned.Count;

            if (_aliveCount.Value > 0) return;

            MarkCleared();
        }

        private void MarkCleared()
        {
            IsRunning = false;
            IsCleared = true;
            _cleared.OnNext(Unit.Default);
            _cleared.OnCompleted();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _started.Dispose();
            _cleared.Dispose();
            _aliveCount.Dispose();
        }
    }
}
