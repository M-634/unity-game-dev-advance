namespace DemoGame
{
    public readonly struct GraphicsQualityPreset
    {
        public readonly float RenderScale;
        public readonly int MsaaSampleCount;
        public readonly float ShadowDistance;
        public readonly int ShadowResolution;
        public readonly int ShadowCascadeCount;
        public readonly int MaxAdditionalLights;

        public GraphicsQualityPreset(
            float renderScale, int msaaSampleCount, float shadowDistance,
            int shadowResolution, int shadowCascadeCount, int maxAdditionalLights)
        {
            RenderScale = renderScale;
            MsaaSampleCount = msaaSampleCount;
            ShadowDistance = shadowDistance;
            ShadowResolution = shadowResolution;
            ShadowCascadeCount = shadowCascadeCount;
            MaxAdditionalLights = maxAdditionalLights;
        }

        public static GraphicsQualityPreset Get(GraphicsQuality quality)
        {
            return quality switch
            {
                GraphicsQuality.Low => new GraphicsQualityPreset(0.7f, 1, 25f, 1024, 1, 2),
                GraphicsQuality.Medium => new GraphicsQualityPreset(0.85f, 1, 35f, 1024, 2, 4),
                GraphicsQuality.Ultra => new GraphicsQualityPreset(1f, 4, 70f, 4096, 4, 8),
                _ => new GraphicsQualityPreset(1f, 1, 50f, 2048, 4, 4),
            };
        }
    }
}
