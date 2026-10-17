using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    public class FieldEnemySpawner : MonoBehaviour
    {
        [SerializeField]
        private GameProgress _progress;

        [SerializeField]
        private InGameController _inGameController;

        [Tooltip("この数の灯火が点いたら出し始める")]
        [SerializeField, Range(1, GameProgress.RequiredLitCount)]
        private int _startLitCount = 1;

        [Tooltip("このエリアに入ったら出すのをやめる")]
        [SerializeField]
        private AreaType _stopArea = AreaType.Temple;

        [SerializeField]
        private GameObject _enemyPrefab;

        [SerializeField]
        private Transform[] _spawnPoints;

        [SerializeField]
        private GameObject _spawnEffect;

        [Header("出し方")]
        [SerializeField, Range(1, 10)]
        private int _maxAlive = 4;

        [Tooltip("オフなら、最初の数だけ出して、倒されても出し直さない")]
        [SerializeField]
        private bool _respawns = true;

        [Tooltip("倒されてから次を出すまでの時間（秒）")]
        [SerializeField, Range(0f, 120f)]
        private float _respawnDelay = 20f;

        [Tooltip("出し始めたとき、1 体ずつずらして出す間隔（秒）")]
        [SerializeField, Range(0f, 10f)]
        private float _initialInterval = 2f;

        [Tooltip("プレイヤーからこの距離（m）より近い出現点には出さない")]
        [SerializeField, Range(0f, 60f)]
        private float _minPlayerDistance = 25f;

        private readonly CompositeDisposable _disposables = new();
        private readonly List<EnemyController> _alive = new();
        private readonly List<float> _readyTimes = new();
        private Transform _player;

        private void Start()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }

            if (_progress == null || _enemyPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogWarning($"{nameof(FieldEnemySpawner)}: 参照が足りないので敵を出しません。", this);
                return;
            }

            var playerObject = GameObject.FindGameObjectWithTag("Player");
            _player = playerObject != null ? playerObject.transform : null;

            _progress.LitCount
                .Where(count => count >= _startLitCount)
                .Take(1)
                .Subscribe(_ => RunAsync(destroyCancellationToken).SuppressCancellationThrow().Forget())
                .AddTo(_disposables);
        }

        private async UniTask RunAsync(CancellationToken ct)
        {
            for (int i = 0; i < _maxAlive; i++)
            {
                _readyTimes.Add(Time.time + _initialInterval * i);
            }

            while (!ct.IsCancellationRequested)
            {
                if (ShouldStop()) return;
                if (!_respawns && _readyTimes.Count == 0) return;

                _alive.RemoveAll(enemy => enemy == null);

                if (_readyTimes.Count > 0 && Time.time >= _readyTimes[0] && _alive.Count < _maxAlive && TrySpawn())
                {
                    _readyTimes.RemoveAt(0);
                }

                await UniTask.Delay(500, cancellationToken: ct);
            }
        }

        private bool ShouldStop()
        {
            if (_inGameController != null && _inGameController.IsFinished) return true;

            return _progress.CurrentArea.CurrentValue >= _stopArea;
        }

        private bool TrySpawn()
        {
            Transform point = PickPoint();
            if (point == null) return false;

            FxSpawner.Spawn(_spawnEffect, point.position);
            var enemy = Instantiate(_enemyPrefab, point.position, point.rotation);
            enemy.SetActive(true);

            if (!enemy.TryGetComponent(out EnemyController controller)) return true;

            _alive.Add(controller);
            controller.Died.Take(1).Subscribe(_ =>
            {
                _alive.Remove(controller);
                if (_respawns) _readyTimes.Add(Time.time + _respawnDelay);
            }).AddTo(_disposables);

            return true;
        }

        private Transform PickPoint()
        {
            int start = Random.Range(0, _spawnPoints.Length);
            float minSqr = _minPlayerDistance * _minPlayerDistance;

            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                Transform point = _spawnPoints[(start + i) % _spawnPoints.Length];
                if (point == null) continue;
                if (_player != null && (point.position - _player.position).sqrMagnitude < minSqr) continue;

                return point;
            }

            return null;
        }

        private void OnDrawGizmosSelected()
        {
            if (_spawnPoints == null) return;

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            foreach (var point in _spawnPoints)
            {
                if (point != null) Gizmos.DrawWireSphere(point.position, 0.6f);
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
