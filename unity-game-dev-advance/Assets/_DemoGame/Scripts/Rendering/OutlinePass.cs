using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace DemoGame
{
    // 輪郭線の Render Graph パス
    // カメラのカラーを読み、線を足した結果を新しいテクスチャに書いて、それをカメラのカラーに差し替える
    public class OutlinePass : ScriptableRenderPass
    {
        private static readonly int IntensityId = Shader.PropertyToID("_OutlineIntensity");
        private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int ThicknessId = Shader.PropertyToID("_OutlineThickness");
        private static readonly int DepthThresholdId = Shader.PropertyToID("_OutlineDepthThreshold");
        private static readonly int NormalThresholdId = Shader.PropertyToID("_OutlineNormalThreshold");
        private static readonly int FadeDistanceId = Shader.PropertyToID("_OutlineFadeDistance");

        private readonly Material _material;

        private class PassData
        {
            internal Material Material;
            internal TextureHandle Source;
        }

        public OutlinePass(Material material)
        {
            _material = material;
            profilingSampler = new ProfilingSampler("DemoGame Outline");

            // SSAO と同じ DepthNormals を使う。SSAO が既に要求しているので、事前パスは増えない
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);

            // バックバッファには直接書けない（差し替えができない）ので、中間テクスチャを要求する
            requiresIntermediateTexture = true;
        }

        public void Setup(OutlineVolume volume)
        {
            _material.SetFloat(IntensityId, volume.intensity.value);
            _material.SetColor(ColorId, volume.color.value);
            _material.SetFloat(ThicknessId, volume.thickness.value);
            _material.SetFloat(DepthThresholdId, volume.depthThreshold.value);
            _material.SetFloat(NormalThresholdId, volume.normalThreshold.value);
            _material.SetFloat(FadeDistanceId, volume.fadeDistance.value);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer) return;

            TextureHandle source = resourceData.activeColorTexture;

            var desc = renderGraph.GetTextureDesc(source);
            desc.name = "_DemoOutlineColor";
            desc.clearBuffer = false;
            TextureHandle destination = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("DemoGame Outline", out var passData, profilingSampler))
            {
                passData.Material = _material;
                passData.Source = source;

                builder.UseTexture(source, AccessFlags.Read);

                if (resourceData.cameraDepthTexture.IsValid())
                {
                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                }

                if (resourceData.cameraNormalsTexture.IsValid())
                {
                    builder.UseTexture(resourceData.cameraNormalsTexture, AccessFlags.Read);
                }

                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    // TextureHandle は RTHandle と RenderTargetIdentifier の両方に暗黙変換できるので、型を明示する
                    RTHandle sourceHandle = data.Source;
                    Blitter.BlitTexture(context.cmd, sourceHandle, new Vector4(1f, 1f, 0f, 0f), data.Material, 0);
                });
            }

            // コピーで書き戻さず、結果のテクスチャをそのままカメラのカラーにする
            resourceData.cameraColor = destination;
        }
    }
}
