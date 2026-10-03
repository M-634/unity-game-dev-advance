using UnityEngine;
using UnityEngine.AI;

namespace Week01
{
    // 周回ルートを歩いてプレイヤーを探す。ルートが無ければ出現位置のまわりをランダムに回る
    public sealed class SearchState : IState
    {
        private readonly EnemyController _owner;
        private readonly SearchStateSettings _settings;

        private int _pointIndex;
        private bool _isWaiting;
        private float _waitEndTime;

        public SearchState(EnemyController owner, SearchStateSettings settings)
        {
            _owner = owner;
            _settings = settings;
        }

        public void OnEnter()
        {
            GoToNextPoint();
        }

        public void OnUpdate()
        {
            if (_owner.DistanceToTarget <= _settings.DetectRange)
            {
                _owner.ChangeState(_owner.PursueState);
                return;
            }

            if (_isWaiting)
            {
                if (Time.time >= _waitEndTime)
                {
                    GoToNextPoint();
                }

                return;
            }

            if (_owner.HasArrived)
            {
                Wait();
            }
        }

        public void OnExit()
        {
            _owner.StopMoving();
        }

        private void GoToNextPoint()
        {
            if (!TryGetNextPoint(out var point))
            {
                Wait();
                return;
            }

            _isWaiting = false;
            _owner.MoveTo(point, _owner.Parameters.SearchWalkSpeed);
        }

        private void Wait()
        {
            _isWaiting = true;
            _waitEndTime = Time.time + _settings.WaitTime;
            _owner.StopMoving();
        }

        private bool TryGetNextPoint(out Vector3 point)
        {
            var points = _settings.PatrolPoints;

            if (points != null && points.Length > 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var candidate = points[_pointIndex];
                    _pointIndex = (_pointIndex + 1) % points.Length;

                    if (candidate == null) continue;

                    point = candidate.position;
                    return true;
                }
            }
            else
            {
                Vector2 offset = Random.insideUnitCircle * _settings.PatrolRadius;
                Vector3 random = _owner.HomePosition + new Vector3(offset.x, 0f, offset.y);

                if (NavMesh.SamplePosition(random, out var hit, _settings.PatrolRadius, NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = default;
            return false;
        }
    }
}
