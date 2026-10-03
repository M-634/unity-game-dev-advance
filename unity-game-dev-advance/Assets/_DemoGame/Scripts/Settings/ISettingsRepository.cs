namespace DemoGame
{
    // 設定の保存先。Model や Presenter は保存の方法（JSON か PlayerPrefs か）を知らずに済む。
    public interface ISettingsRepository
    {
        SettingsData Load();

        void Save(SettingsData data);
    }
}
