using System;

namespace DemoGame
{
    public enum ScreenMode
    {
        FullScreen,
        Windowed,
    }

    public enum GraphicsQuality
    {
        Low,
        Medium,
        High,
        Ultra,
    }

    // 保存する形。JsonUtility で読み書きするので、public なフィールドだけで持つ。
    [Serializable]
    public sealed class SettingsData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public GraphicsSettingsData Graphics = new();
        public SoundSettingsData Sound = new();
    }

    [Serializable]
    public sealed class GraphicsSettingsData
    {
        // 0 × 0 は「画面の解像度に合わせる」
        public int ResolutionWidth;
        public int ResolutionHeight;
        public ScreenMode ScreenMode = ScreenMode.FullScreen;
        public GraphicsQuality Quality = GraphicsQuality.High;
        public bool VSync = true;
        public int FrameRateLimit = 60;
    }

    [Serializable]
    public sealed class SoundSettingsData
    {
        public float Master = 0.8f;
        public float Bgm = 0.5f;
        public float Se = 0.5f;
    }
}
