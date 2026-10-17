using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DemoGame
{
    [Serializable, VolumeComponentMenu("DemoGame/Outline")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public sealed class OutlineVolume : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("線の濃さ。0 なら描かない")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        [Tooltip("下の色に掛ける色。黒一色ではなく、下の色を暗くする")]
        public ColorParameter color = new ColorParameter(new Color(0.15f, 0.1f, 0.2f), false, false, true);

        [Tooltip("線の太さ（1080p 基準のピクセル数）")]
        public ClampedFloatParameter thickness = new ClampedFloatParameter(1.5f, 0.5f, 4f);

        [Tooltip("深度の差がこの割合を超えたら線にする。下げると線が増える")]
        public ClampedFloatParameter depthThreshold = new ClampedFloatParameter(0.1f, 0.01f, 1f);

        [Tooltip("法線の差がこの値を超えたら線にする。下げると内側の線（服のしわなど）が増える")]
        public ClampedFloatParameter normalThreshold = new ClampedFloatParameter(0.4f, 0f, 2f);

        [Tooltip("この距離（m）から先は線を薄くする")]
        public MinFloatParameter fadeDistance = new MinFloatParameter(40f, 1f);

        public bool IsActive() => intensity.value > 0f;
    }
}
