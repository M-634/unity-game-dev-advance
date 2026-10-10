using UnityEngine;

namespace Section1.AnimationRigging
{
    // いつもカメラと同じ向きにする（HP バー・ダメージ数など、World Space に置く表示用）
    // カメラ（Cinemachine）が動いたあとで向きを合わせるため、実行順を遅くしている
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
