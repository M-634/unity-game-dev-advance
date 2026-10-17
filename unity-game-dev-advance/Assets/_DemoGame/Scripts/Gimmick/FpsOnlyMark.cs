using R3;
using UnityEngine;

namespace DemoGame
{
    public class FpsOnlyMark : MonoBehaviour
    {
        [SerializeField]
        private CameraModeSwitcher _switcher;

        [Tooltip("FPS のときだけ出すもの。空なら子の Renderer すべて")]
        [SerializeField]
        private Renderer[] _renderers;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }

            if (_switcher == null)
            {
                _switcher = FindFirstObjectByType<CameraModeSwitcher>();
            }

            if (_switcher == null)
            {
                SetVisible(false);
                return;
            }

            _switcher.Mode.Subscribe(mode => SetVisible(mode == CameraMode.Fps)).AddTo(_disposables);
        }

        private void SetVisible(bool visible)
        {
            foreach (var target in _renderers)
            {
                if (target != null) target.enabled = visible;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
