using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemoGame
{
    public static class LevelLightBaker
    {
        private const string MenuRoot = "DemoGame/Lighting/";
        private const string LevelRootName = "LevelRoot";
        private const string LevelLayerName = "Level";
        private const int LevelLayerIndex = 9;
        private const string SettingsPath = "Assets/_DemoGame/Graphics/Settings/LS_DemoInGame.lighting";

        private const StaticEditorFlags LevelStaticFlags =
            StaticEditorFlags.ContributeGI
            | StaticEditorFlags.OccluderStatic
            | StaticEditorFlags.OccludeeStatic
            | StaticEditorFlags.ReflectionProbeStatic
            | StaticEditorFlags.BatchingStatic;

        [MenuItem(MenuRoot + "Bake Level")]
        private static void BakeLevel()
        {
            if (!ApplyStaticFlags()) return;

            AssignLightingSettings();
            CheckLights();

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (!Lightmapping.BakeAsync())
            {
                Debug.LogError($"{nameof(LevelLightBaker)}: ベイクを始められませんでした。シーンが保存されているか確認してください。");
            }
        }

        [MenuItem(MenuRoot + "Apply Static Flags")]
        private static void ApplyStaticFlagsMenu()
        {
            ApplyStaticFlags();
        }

        [MenuItem(MenuRoot + "Clear Baked Data")]
        private static void ClearBakedData()
        {
            Lightmapping.Clear();
            Debug.Log($"{nameof(LevelLightBaker)}: ベイクした結果を消しました。");
        }

        [MenuItem(MenuRoot + "Bake Level", true)]
        [MenuItem(MenuRoot + "Apply Static Flags", true)]
        private static bool CanEdit()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode && !Lightmapping.isRunning;
        }

        private static bool ApplyStaticFlags()
        {
            int levelLayer = EnsureLevelLayer();
            if (levelLayer < 0) return false;

            var scene = SceneManager.GetActiveScene();
            var levelRoots = FindLevelRoots(scene);
            if (levelRoots.Count == 0)
            {
                Debug.LogError($"{nameof(LevelLightBaker)}: アクティブなシーン（{scene.name}）に {LevelRootName} がありません。");
                return false;
            }

            var log = new StringBuilder();
            int staticCount = 0;
            int changedCount = 0;

            foreach (var root in levelRoots)
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    var go = t.gameObject;
                    bool isLevel = go.layer == levelLayer;

                    var current = GameObjectUtility.GetStaticEditorFlags(go);
                    var next = isLevel ? current | LevelStaticFlags : current & ~LevelStaticFlags;

                    if (isLevel)
                    {
                        staticCount++;

                        if (go.TryGetComponent(out Rigidbody body) && !body.isKinematic)
                        {
                            Debug.LogWarning($"{nameof(LevelLightBaker)}: {GetPath(go)} は Layer {LevelLayerName} ですが、動く Rigidbody を持っています。", go);
                        }
                    }

                    if (next != current)
                    {
                        Undo.RecordObject(go, "Apply Level Static Flags");
                        GameObjectUtility.SetStaticEditorFlags(go, next);
                        log.AppendLine($"  {(isLevel ? "静的にした" : "静的を外した")}：{GetPath(go)}");
                        changedCount++;
                    }

                    if (isLevel && go.TryGetComponent(out MeshRenderer meshRenderer) && meshRenderer.receiveGI != ReceiveGI.Lightmaps)
                    {
                        Undo.RecordObject(meshRenderer, "Apply Level Static Flags");
                        meshRenderer.receiveGI = ReceiveGI.Lightmaps;
                    }
                }
            }

            WarnLevelLayerOutsideRoot(scene, levelRoots, levelLayer);

            if (changedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            Debug.Log($"{nameof(LevelLightBaker)}: 静的な物 {staticCount} 個（Layer {LevelLayerName}）。設定を変えた物 {changedCount} 個。\n{log}");

            if (staticCount == 0)
            {
                Debug.LogWarning($"{nameof(LevelLightBaker)}: {LevelRootName} の下に Layer {LevelLayerName} の物がありません。焼く物が無いので、ライトマップは作られません。");
            }

            return true;
        }

        private static int EnsureLevelLayer()
        {
            int layer = LayerMask.NameToLayer(LevelLayerName);
            if (layer >= 0) return layer;

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var element = tagManager.FindProperty("layers").GetArrayElementAtIndex(LevelLayerIndex);

            if (!string.IsNullOrEmpty(element.stringValue))
            {
                Debug.LogError($"{nameof(LevelLightBaker)}: User Layer {LevelLayerIndex} は「{element.stringValue}」に使われています。Layer {LevelLayerName} を作れません。");
                return -1;
            }

            element.stringValue = LevelLayerName;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"{nameof(LevelLightBaker)}: User Layer {LevelLayerIndex} に {LevelLayerName} を足しました。");
            return LevelLayerIndex;
        }

        private static List<GameObject> FindLevelRoots(Scene scene)
        {
            var result = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == LevelRootName)
                {
                    result.Add(root);
                }
            }

            return result;
        }

        private static void WarnLevelLayerOutsideRoot(Scene scene, List<GameObject> levelRoots, int levelLayer)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (levelRoots.Contains(root)) continue;

                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.gameObject.layer != levelLayer) continue;

                    Debug.LogWarning($"{nameof(LevelLightBaker)}: {GetPath(t.gameObject)} は Layer {LevelLayerName} ですが {LevelRootName} の外にあるので、焼かれません。", t.gameObject);
                }
            }
        }

        private static void AssignLightingSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(SettingsPath);
            if (settings == null)
            {
                settings = new LightingSettings
                {
                    name = "LS_DemoInGame",
                    realtimeGI = false,
                    bakedGI = true,
                    mixedBakeMode = MixedLightingMode.IndirectOnly,
                    lightmapper = LightingSettings.Lightmapper.ProgressiveGPU,
                    directionalityMode = LightmapsMode.NonDirectional,
                    lightmapResolution = 8f,
                    lightmapMaxSize = 1024,
                    ao = false,
                };

                AssetDatabase.CreateAsset(settings, SettingsPath);
                Debug.Log($"{nameof(LevelLightBaker)}: {SettingsPath} を作りました。");
            }

            if (Lightmapping.TryGetLightingSettings(out var current) && current == settings) return;

            Lightmapping.lightingSettings = settings;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"{nameof(LevelLightBaker)}: シーンに {settings.name} を割り当てました。");
        }

        private static void CheckLights()
        {
            var scene = SceneManager.GetActiveScene();
            int mixed = 0;
            int realtime = 0;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var light in root.GetComponentsInChildren<Light>(true))
                {
                    switch (light.lightmapBakeType)
                    {
                        case LightmapBakeType.Baked:
                            Debug.LogWarning($"{nameof(LevelLightBaker)}: {GetPath(light.gameObject)} が Baked です。Mixed か Realtime にしてください（直接光を焼くとトゥーンの段が出なくなります）。", light);
                            break;
                        case LightmapBakeType.Mixed:
                            mixed++;
                            break;
                        default:
                            realtime++;
                            break;
                    }
                }
            }

            Debug.Log($"{nameof(LevelLightBaker)}: ライト Mixed {mixed} 個（間接光を焼く）・Realtime {realtime} 個（焼かない）。");
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
