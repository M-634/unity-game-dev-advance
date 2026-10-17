using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DemoGame
{
    [Serializable, VolumeComponentMenu("DemoGame/Flame Vignette")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public sealed class FlameVignetteVolume : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("炎の強さ。0 なら描かない")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        [Tooltip("炎の芯の色（HDR。Bloom で光る）")]
        public ColorParameter innerColor = new ColorParameter(new Color(4f, 2.4f, 0.8f), true, false, true);

        [Tooltip("炎の外側の色（HDR）")]
        public ColorParameter outerColor = new ColorParameter(new Color(2f, 0.5f, 0.1f), true, false, true);

        [Tooltip("画面の縁からの幅（画面の半分に対する割合）")]
        public ClampedFloatParameter width = new ClampedFloatParameter(0.35f, 0.05f, 1f);

        [Tooltip("下側をどれだけ強くするか")]
        public ClampedFloatParameter bottomBoost = new ClampedFloatParameter(0.5f, 0f, 2f);

        [Tooltip("炎が昇る速さ")]
        public ClampedFloatParameter speed = new ClampedFloatParameter(0.6f, 0f, 5f);

        [Tooltip("炎の揺らぎの細かさ")]
        public ClampedFloatParameter noiseScale = new ClampedFloatParameter(3f, 0.5f, 10f);

        public bool IsActive() => intensity.value > 0f;
    }
}
