using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace DemoGame
{
    // 設定の値そのもの。どこに保存されるか、どう反映されるかは知らない。
    public sealed class SettingsModel : IDisposable
    {
        // 0 は上限なし
        public static readonly IReadOnlyList<int> FrameRateChoices = new[] { 30, 60, 120, 144, 0 };

        private readonly ReactiveProperty<ScreenResolution> _resolution = new(ScreenResolution.Native);
        private readonly ReactiveProperty<ScreenMode> _screenMode = new();
        private readonly ReactiveProperty<GraphicsQuality> _quality = new();
        private readonly ReactiveProperty<bool> _vSync = new();
        private readonly ReactiveProperty<int> _frameRateLimit = new();
        private readonly ReactiveProperty<float> _masterVolume = new();
        private readonly ReactiveProperty<float> _bgmVolume = new();
        private readonly ReactiveProperty<float> _seVolume = new();

        public ReadOnlyReactiveProperty<ScreenResolution> Resolution => _resolution;
        public ReadOnlyReactiveProperty<ScreenMode> ScreenMode => _screenMode;
        public ReadOnlyReactiveProperty<GraphicsQuality> Quality => _quality;
        public ReadOnlyReactiveProperty<bool> VSync => _vSync;
        public ReadOnlyReactiveProperty<int> FrameRateLimit => _frameRateLimit;
        public ReadOnlyReactiveProperty<float> MasterVolume => _masterVolume;
        public ReadOnlyReactiveProperty<float> BgmVolume => _bgmVolume;
        public ReadOnlyReactiveProperty<float> SeVolume => _seVolume;

        public SettingsModel(SettingsData data)
        {
            Load(data);
        }

        public void SetResolution(ScreenResolution value) => _resolution.Value = value;

        public void SetScreenMode(ScreenMode value) => _screenMode.Value = value;

        public void SetQuality(GraphicsQuality value) => _quality.Value = value;

        public void SetVSync(bool value) => _vSync.Value = value;

        public void SetFrameRateLimit(int value)
        {
            if (!Contains(FrameRateChoices, value)) return;

            _frameRateLimit.Value = value;
        }

        public void SetMasterVolume(float value) => _masterVolume.Value = Mathf.Clamp01(value);

        public void SetBgmVolume(float value) => _bgmVolume.Value = Mathf.Clamp01(value);

        public void SetSeVolume(float value) => _seVolume.Value = Mathf.Clamp01(value);

        public void ResetToDefault()
        {
            Load(new SettingsData());
        }

        public void Load(SettingsData data)
        {
            GraphicsSettingsData graphics = data?.Graphics ?? new GraphicsSettingsData();
            SoundSettingsData sound = data?.Sound ?? new SoundSettingsData();

            SetResolution(new ScreenResolution(graphics.ResolutionWidth, graphics.ResolutionHeight));
            SetScreenMode(graphics.ScreenMode);
            SetQuality(graphics.Quality);
            SetVSync(graphics.VSync);
            _frameRateLimit.Value = Contains(FrameRateChoices, graphics.FrameRateLimit)
                ? graphics.FrameRateLimit
                : new GraphicsSettingsData().FrameRateLimit;
            SetMasterVolume(sound.Master);
            SetBgmVolume(sound.Bgm);
            SetSeVolume(sound.Se);
        }

        public SettingsData ToData()
        {
            return new SettingsData
            {
                Graphics = new GraphicsSettingsData
                {
                    ResolutionWidth = _resolution.Value.Width,
                    ResolutionHeight = _resolution.Value.Height,
                    ScreenMode = _screenMode.Value,
                    Quality = _quality.Value,
                    VSync = _vSync.Value,
                    FrameRateLimit = _frameRateLimit.Value,
                },
                Sound = new SoundSettingsData
                {
                    Master = _masterVolume.Value,
                    Bgm = _bgmVolume.Value,
                    Se = _seVolume.Value,
                },
            };
        }

        private static bool Contains(IReadOnlyList<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value) return true;
            }

            return false;
        }

        public void Dispose()
        {
            _resolution.Dispose();
            _screenMode.Dispose();
            _quality.Dispose();
            _vSync.Dispose();
            _frameRateLimit.Dispose();
            _masterVolume.Dispose();
            _bgmVolume.Dispose();
            _seVolume.Dispose();
        }
    }
}
