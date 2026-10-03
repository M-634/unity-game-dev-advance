using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace DemoGame
{
    // 画面の縁の炎の Render Graph パス。作りは OutlinePass と同じ（カメラのカラーを読んで、新しいテクスチャに書いて差し替える）
    public class FlameVignettePass : ScriptableRenderPass
    {
        private static readonly int IntensityId = Shader.PropertyToID("_FlameIntensity");
        private static readonly int InnerColorId = Shader.PropertyToID("_FlameInnerColor");
        private static readonly int OuterColorId = Shader.PropertyToID("_FlameOuterColor");
        private static readonly int WidthId = Shader.PropertyToID("_FlameWidth");
        private static readonly int BottomBoostId = Shader.PropertyToID("_FlameBottomBoost");
        private static readonly int SpeedId = Shader.PropertyToID("_FlameSpeed");
        private static readonly int NoiseScaleId = Shader.PropertyToID("_FlameNoiseScale");
        private static readonly int NoiseTexId = Shader.PropertyToID("_FlameNoise");

        private readonly Material _material;

        private class PassData
        {
            internal Material Material;
            internal TextureHandle Source;
        }

        public FlameVignettePass(Material material, Texture2D noise)
        {
            _material = material;
            profilingSampler = new ProfilingSampler("DemoGame Flame Vignette");
            requiresIntermediateTexture = true;

            if (noise != null)
            {
                _material.SetTexture(NoiseTexId, noise);
            }
        }

        public void Setup(FlameVignetteVolume volume)
        {
            _material.SetFloat(IntensityId, volume.intensity.value);
            _material.SetColor(InnerColorId, volume.innerColor.value);
            _material.SetColor(OuterColorId, volume.outerColor.value);
            _material.SetFloat(WidthId, volume.width.value);
            _material.SetFloat(BottomBoostId, volume.bottomBoost.value);
            _material.SetFloat(SpeedId, volume.speed.value);
            _material.SetFloat(NoiseScaleId, volume.noiseScale.value);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer) return;

            TextureHandle source = resourceData.activeColorTexture;

            var desc = renderGraph.GetTextureDesc(source);
            desc.name = "_DemoFlameVignetteColor";
            desc.clearBuffer = false;
            TextureHandle destination = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("DemoGame Flame Vignette", out var passData, profilingSampler))
            {
                passData.Material = _material;
                passData.Source = source;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    RTHandle sourceHandle = data.Source;
                    Blitter.BlitTexture(context.cmd, sourceHandle, new Vector4(1f, 1f, 0f, 0f), data.Material, 0);
                });
            }

            resourceData.cameraColor = destination;
        }
    }
}
