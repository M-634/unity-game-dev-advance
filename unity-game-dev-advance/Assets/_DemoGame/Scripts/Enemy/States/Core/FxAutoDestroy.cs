using UnityEngine;

namespace DemoGame
{
    public class FxAutoDestroy : MonoBehaviour
    {
        [Tooltip("出てから消えるまでの秒数（いちばん長く残るパーティクルの寿命より長く）")]
        [SerializeField, Range(0.1f, 20f)]
        private float _lifetime = 2f;

        private void Start()
        {
            Destroy(gameObject, _lifetime);
        }
    }
}
