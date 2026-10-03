using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DemoGame
{
    // 画面の縁の炎を描く。PC_Renderer に1つだけ足す。設計：doc/demo-game/11-lighting.md §5.3
    // 設定は Volume（FlameVignetteVolume）から読む。Intensity が 0（＝チャージしていない）なら何もしない
    public class FlameVignetteRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader _shader;

        [Tooltip("炎の形に使うノイズ（T_Noise）")]
        [SerializeField]
        private Texture2D _noise;

        [Tooltip("ポストエフェクトの前。Bloom で炎を光らせる")]
        [SerializeField]
        private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        private Material _material;
        private FlameVignettePass _pass;

        public override void Create()
        {
            if (_shader == null)
            {
                _shader = Shader.Find("Hidden/DemoGame/FlameVignette");
            }

            if (_shader == null) return;

            _material = CoreUtils.CreateEngineMaterial(_shader);
            _pass = new FlameVignettePass(_material, _noise)
            {
                renderPassEvent = _renderPassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null) return;

            // ゲームのカメラだけ（Scene ビューには出さない）
            if (renderingData.cameraData.cameraType != CameraType.Game) return;

            var volume = VolumeManager.instance.stack.GetComponent<FlameVignetteVolume>();
            if (volume == null || !volume.IsActive()) return;

            _pass.Setup(volume);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_material);
        }
    }
}
