using UnityEngine;

namespace Section1.AnimationRigging
{
    // 再生中はカーソルを画面内に固定して隠す
    public class CursorLock : MonoBehaviour
    {
        [SerializeField]
        private bool _lockCursor = true;

        private void OnEnable()
        {
            SetLocked(_lockCursor);
        }

        private void OnDisable()
        {
            SetLocked(false);
        }

        private static void SetLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
