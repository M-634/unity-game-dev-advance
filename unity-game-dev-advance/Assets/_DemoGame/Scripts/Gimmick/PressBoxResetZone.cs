using UnityEngine;

namespace DemoGame
{
    [RequireComponent(typeof(Collider))]
    public class PressBoxResetZone : MonoBehaviour
    {
        [Tooltip("プレイヤーがこの範囲から出たら元の位置へ戻す箱")]
        [SerializeField]
        private PressBox[] _boxes = new PressBox[0];

        [Tooltip("この灯火が点いたら、もう戻さない")]
        [SerializeField]
        private Lantern _lantern;

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (_lantern != null && _lantern.IsLit) return;

            foreach (var box in _boxes)
            {
                if (box == null || box.IsBeingPushed) continue;

                box.ResetPosition();
            }
        }
    }
}
