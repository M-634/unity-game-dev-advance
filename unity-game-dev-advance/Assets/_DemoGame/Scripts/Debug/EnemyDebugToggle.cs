using DemoGame;
using UnityEngine;
using UnityEngine.InputSystem;

// この名前空間の中では `Debug` が Demo.Debug 名前空間を指す。ログを出すときは UnityEngine.Debug と書くこと
namespace Demo.Debug
{
    // F1 で全部の敵のデバッグモード（DebugState：棒立ち・一撃で倒れる）を切り替える
    // シーンには置かない。再生を始めたときに自分で作られ、シーンをまたいで残る
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
