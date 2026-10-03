using UnityEngine;

namespace DemoGame
{
    // ゲーム中に1つだけの設定を持つ場所。シーンをまたいでも同じ SettingsModel を使う。
    // 初めて使われたときにファイルから読み込み、グラフィックの反映を始める。
    public static class SettingsStore
    {
        private static ISettingsRepository _repository;
        private static SettingsModel _model;
        private static GraphicsSettingsApplier _graphicsApplier;

        public static SettingsModel Model
        {
            get
            {
                EnsureInitialized();
                return _model;
            }
        }

        public static void Save()
        {
            EnsureInitialized();
            _repository.Save(_model.ToData());
        }

        private static void EnsureInitialized()
        {
            if (_model != null) return;

            _repository = new JsonSettingsRepository();
            _model = new SettingsModel(_repository.Load());
            _graphicsApplier = new GraphicsSettingsApplier(_model);

            Application.quitting += Shutdown;
        }

        // Editor では再生の終わりに呼ばれる。URP アセットなどを元の値に戻す
        private static void Shutdown()
        {
            Application.quitting -= Shutdown;

            _graphicsApplier?.Dispose();
            _model?.Dispose();
            _graphicsApplier = null;
            _model = null;
            _repository = null;
        }

        // Domain Reload を切っていても、再生のたびに読み込み直す
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Shutdown();
        }
    }
}
