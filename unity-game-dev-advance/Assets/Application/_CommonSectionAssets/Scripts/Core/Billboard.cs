using UnityEngine;

namespace Section.Common
{
    [DefaultExecutionOrder(1000)]
    public class Billboard : MonoBehaviour
    {
        private Camera _camera;

        private void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

            transform.rotation = _camera.transform.rotation;
        }
    }
}
