using R3;
using UnityEngine;

namespace DemoGame
{
    // 押されたこと・離されたことを知らせるだけのスイッチ。押されて何が起きるかは知らない（聞く側が購読する）
    // 床スイッチ・手で押すボタンなど、押し方の違いは派生クラスが持つ
    public abstract class SwitchBase : MonoBehaviour
    {
        private readonly Subject<Unit> _pressed = new();
        private readonly Subject<Unit> _released = new();

        public Observable<Unit> Pressed => _pressed;

        // 押されていた状態が終わった瞬間（押しっぱなしにできないスイッチでは流れない）
        public Observable<Unit> Released => _released;

        public bool IsPressed { get; private set; }

        protected void NotifyPressed()
        {
            IsPressed = true;
            _pressed.OnNext(Unit.Default);
        }

        protected void NotifyReleased()
        {
            IsPressed = false;
            _released.OnNext(Unit.Default);
        }

        protected virtual void OnDestroy()
        {
            _pressed.Dispose();
            _released.Dispose();
        }
    }
}
