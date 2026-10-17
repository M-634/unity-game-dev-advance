using UnityEngine;
using UnityEngine.Rendering;

namespace DemoGame
{
    public class FirstPersonBodyHider : MonoBehaviour
    {
        [SerializeField]
        private PlayerController _controller;

        [Tooltip("隠す範囲。プレイヤーの Visual を指定する")]
        [SerializeField]
        private Transform _visualRoot;

        private Renderer[] _renderers;
        private ShadowCastingMode[] _defaultModes;
        private bool _isHidden;

        private void Awake()
        {
            if (_controller == null) _controller = GetComponent<PlayerController>();
            if (_visualRoot == null && _controller != null) _visualRoot = _controller.VisualRoot;

            _renderers = _visualRoot != null ? _visualRoot.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
            _defaultModes = new ShadowCastingMode[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
            {
                _defaultModes[i] = _renderers[i].shadowCastingMode;
            }
        }

        private void LateUpdate()
        {
            bool hide = _controller != null && _controller.ViewMode == CameraMode.Fps;
            if (hide == _isHidden) return;

            _isHidden = hide;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;

                _renderers[i].shadowCastingMode = hide ? ShadowCastingMode.ShadowsOnly : _defaultModes[i];
            }
        }
    }
}
