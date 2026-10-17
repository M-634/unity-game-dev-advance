using DemoGame;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Demo.Debug
{
    public class EnemyDebugToggle : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var go = new GameObject(nameof(EnemyDebugToggle));
            DontDestroyOnLoad(go);
            go.AddComponent<EnemyDebugToggle>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.f1Key.wasPressedThisFrame) return;

            EnemyController.DebugModeForAll = !EnemyController.DebugModeForAll;
            UnityEngine.Debug.Log($"[Debug] 敵のデバッグモード：{(EnemyController.DebugModeForAll ? "ON" : "OFF")}");
        }
    }
}
