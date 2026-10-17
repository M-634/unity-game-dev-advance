using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DemoGame
{
    public class OutlineRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader _shader;

        [Tooltip("不透明の後、半透明（エフェクト）の前。エフェクトに線が入らない")]
        [SerializeField]
        private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        private Material _material;
        private OutlinePass _pass;

        public override void Create()
        {
            if (_shader == null)
            {
                _shader = Shader.Find("Hidden/DemoGame/Outline");
            }

            if (_shader == null) return;

            _material = CoreUtils.CreateEngineMaterial(_shader);
            _pass = new OutlinePass(_material)
            {
                renderPassEvent = _renderPassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null) return;

            var cameraType = renderingData.cameraData.cameraType;
            if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection) return;

            var volume = VolumeManager.instance.stack.GetComponent<OutlineVolume>();
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
