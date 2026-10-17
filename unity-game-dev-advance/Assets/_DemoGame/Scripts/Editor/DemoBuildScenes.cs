using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace DemoGame
{
    [InitializeOnLoad]
    public static class DemoBuildScenes
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/_DemoGame/Scenes/DemoTitle.unity",
            "Assets/_DemoGame/Scenes/DemoInGame.unity",
        };

        static DemoBuildScenes()
        {
            EditorApplication.delayCall += Ensure;
        }

        [MenuItem("DemoGame/Build/Register Demo Scenes")]
        private static void Ensure()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(scene => File.Exists(scene.path))
                .ToList();

            bool changed = scenes.Count != EditorBuildSettings.scenes.Length;

            for (int i = 0; i < ScenePaths.Length; i++)
            {
                string path = ScenePaths[i];
                int index = scenes.FindIndex(scene => scene.path == path);

                if (index == i && scenes[i].enabled) continue;

                if (index >= 0) scenes.RemoveAt(index);

                scenes.Insert(i, new EditorBuildSettingsScene(path, true));
                changed = true;
            }

            if (changed)
            {
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
