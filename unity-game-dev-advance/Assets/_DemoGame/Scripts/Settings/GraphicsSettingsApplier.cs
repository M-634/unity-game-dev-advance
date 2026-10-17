using System;
using R3;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DemoGame
{
    public sealed class GraphicsSettingsApplier : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly UniversalRenderPipelineAsset _pipeline;
        private readonly GraphicsQualityPreset _originalPreset;
        private readonly int _originalVSyncCount;

        public GraphicsSettingsApplier(SettingsModel model)
        {
            _pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            _originalVSyncCount = QualitySettings.vSyncCount;

            if (_pipeline != null)
            {
                _originalPreset = Capture(_pipeline);
            }

            Observable.CombineLatest(model.Resolution, model.ScreenMode, (resolution, mode) => (resolution, mode))
                .Subscribe(x => ApplyScreen(x.resolution, x.mode))
                .AddTo(_disposables);

            model.Quality
                .Subscribe(quality => ApplyPreset(GraphicsQualityPreset.Get(quality)))
                .AddTo(_disposables);

            model.VSync
                .Subscribe(on => QualitySettings.vSyncCount = on ? 1 : 0)
                .AddTo(_disposables);

            model.FrameRateLimit
                .Subscribe(limit => Application.targetFrameRate = limit > 0 ? limit : -1)
                .AddTo(_disposables);
        }

        private static void ApplyScreen(ScreenResolution resolution, ScreenMode mode)
        {
#if UNITY_EDITOR
#else
            if (resolution.IsNative)
            {
                Resolution current = Screen.currentResolution;
                resolution = new ScreenResolution(current.width, current.height);
            }

            FullScreenMode fullScreenMode = mode == ScreenMode.FullScreen
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;
            Screen.SetResolution(resolution.Width, resolution.Height, fullScreenMode);
#endif
        }

        private void ApplyPreset(GraphicsQualityPreset preset)
        {
            if (_pipeline == null) return;

            _pipeline.renderScale = preset.RenderScale;
            _pipeline.msaaSampleCount = preset.MsaaSampleCount;
            _pipeline.shadowDistance = preset.ShadowDistance;
            _pipeline.mainLightShadowmapResolution = preset.ShadowResolution;
            _pipeline.shadowCascadeCount = preset.ShadowCascadeCount;
            _pipeline.maxAdditionalLightsCount = preset.MaxAdditionalLights;
        }

        private static GraphicsQualityPreset Capture(UniversalRenderPipelineAsset pipeline)
        {
            return new GraphicsQualityPreset(
                pipeline.renderScale,
                pipeline.msaaSampleCount,
                pipeline.shadowDistance,
                pipeline.mainLightShadowmapResolution,
                pipeline.shadowCascadeCount,
                pipeline.maxAdditionalLightsCount);
        }

        public void Dispose()
        {
            _disposables.Dispose();

            ApplyPreset(_originalPreset);
            QualitySettings.vSyncCount = _originalVSyncCount;
        }
    }
}
