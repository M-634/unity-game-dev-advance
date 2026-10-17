using R3;
using UnityEngine;

namespace DemoGame
{
    public abstract class SwitchBase : MonoBehaviour
    {
        private readonly Subject<Unit> _pressed = new();
        private readonly Subject<Unit> _released = new();

        public Observable<Unit> Pressed => _pressed;

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
