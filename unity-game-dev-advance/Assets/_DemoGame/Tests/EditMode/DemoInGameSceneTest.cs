using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DemoGame.Tests
{
    public class DemoInGameSceneTest
    {
        private const string ScenePath = "Assets/_DemoGame/Scenes/DemoInGame.unity";

        [Test]
        public void SceneHasNoBrokenObjects()
        {
            LogAssert.ignoreFailingMessages = true;
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            LogAssert.ignoreFailingMessages = false;

            var report = new StringBuilder();
            var all = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToArray();

            foreach (var go in all)
            {
                bool unnamed = string.IsNullOrEmpty(go.name);
                bool missingScript = go.GetComponents<Component>().Any(c => c == null);
                if (!unnamed && !missingScript) continue;

                string parent = go.transform.parent != null ? GetPath(go.transform.parent) : "(root)";
                string components = string.Join(", ", go.GetComponents<Component>().Select(c => c == null ? "MISSING" : c.GetType().Name));
                string children = string.Join(", ", Enumerable.Range(0, go.transform.childCount).Select(i => go.transform.GetChild(i).name));
                string prefab = PrefabUtility.IsPartOfAnyPrefab(go) ? AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(go)) : "-";
                report.AppendLine($"[{(unnamed ? "unnamed" : "missing-script")}] parent={parent} components=[{components}] children=[{children}] prefab={prefab} id={GlobalObjectId.GetGlobalObjectIdSlow(go)}");
            }

            Debug.Log("SCENE-REPORT objects=" + all.Length + "\n" + report);
            Assert.IsEmpty(report.ToString(), report.ToString());
        }

        private static string GetPath(Transform t)
        {
            return t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
        }
    }
}
