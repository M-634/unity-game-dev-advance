using R3;
using UnityEngine;

namespace DemoGame
{
    public class GameProgress : MonoBehaviour
    {
        public const int RequiredLitCount = 3;

        private readonly ReactiveProperty<int> _litCount = new(0);
        private readonly ReactiveProperty<AreaType> _currentArea = new(AreaType.Awakening);

        public ReadOnlyReactiveProperty<int> LitCount => _litCount;

        public ReadOnlyReactiveProperty<AreaType> CurrentArea => _currentArea;

        public Observable<Unit> AllLit =>
            _litCount.Where(count => count >= RequiredLitCount).Select(_ => Unit.Default).Take(1);

        public bool IsAllLit => _litCount.Value >= RequiredLitCount;

        public void AddLit()
        {
            if (IsAllLit) return;

            _litCount.Value++;
        }

        public void EnterArea(AreaType area)
        {
            if (area <= _currentArea.Value) return;

            _currentArea.Value = area;
        }

        public void ResetProgress()
        {
            _litCount.Value = 0;
            _currentArea.Value = AreaType.Awakening;
        }

        private void OnDestroy()
        {
            _litCount.Dispose();
            _currentArea.Dispose();
        }
    }
}
