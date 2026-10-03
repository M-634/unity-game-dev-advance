using R3;
using UnityEngine;

namespace Week01
{
    // 進行状態を持つ唯一の場所。
    // 値が変わったことを知らせるだけで、UI や扉を直接触らない。
    public class GameProgress : MonoBehaviour
    {
        public const int RequiredLitCount = 3;

        private readonly ReactiveProperty<int> _litCount = new(0);

        public ReadOnlyReactiveProperty<int> LitCount => _litCount;

        public Observable<Unit> AllLit =>
            _litCount.Where(count => count >= RequiredLitCount).Select(_ => Unit.Default).Take(1);

        public bool IsAllLit => _litCount.Value >= RequiredLitCount;

        public void AddLit()
        {
            if (IsAllLit) return;

            _litCount.Value++;
        }

        public void ResetProgress()
        {
            _litCount.Value = 0;
        }

        private void OnDestroy()
        {
            _litCount.Dispose();
        }
    }
}
