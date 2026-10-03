using System;
using R3;

namespace DemoGame
{
    public enum TitleScreen
    {
        Menu,
        Settings,
    }

    // 画面の都合を持たない、タイトル画面の状態そのもの。
    // 開始か終了を受け付けたら、そのあとの操作は受け付けない。
    public sealed class TitleModel : IDisposable
    {
        private readonly ReactiveProperty<TitleScreen> _screen = new(TitleScreen.Menu);
        private readonly ReactiveProperty<bool> _isStartRequested = new(false);
        private readonly ReactiveProperty<bool> _isQuitRequested = new(false);

        public ReadOnlyReactiveProperty<TitleScreen> Screen => _screen;
        public ReadOnlyReactiveProperty<bool> IsStartRequested => _isStartRequested;
        public ReadOnlyReactiveProperty<bool> IsQuitRequested => _isQuitRequested;

        private bool IsLocked => _isStartRequested.Value || _isQuitRequested.Value;

        public void RequestStart()
        {
            if (IsLocked || _screen.Value != TitleScreen.Menu) return;

            _isStartRequested.Value = true;
        }

        public void RequestQuit()
        {
            if (IsLocked || _screen.Value != TitleScreen.Menu) return;

            _isQuitRequested.Value = true;
        }

        public void OpenSettings()
        {
            if (IsLocked) return;

            _screen.Value = TitleScreen.Settings;
        }

        public void CloseSettings()
        {
            _screen.Value = TitleScreen.Menu;
        }

        public void Dispose()
        {
            _screen.Dispose();
            _isStartRequested.Dispose();
            _isQuitRequested.Dispose();
        }
    }
}
