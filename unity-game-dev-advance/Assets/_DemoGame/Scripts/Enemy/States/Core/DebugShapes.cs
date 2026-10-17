using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace DemoGame
{
    public static class DebugShapes
    {
        private const string ConditionEditor = "UNITY_EDITOR";
        private const string ConditionDebug = "APP_DEBUG";
        private const int CircleSegments = 48;

        public static bool Enabled { get; set; } = true;

        private static readonly List<Vector3> Vertices = new List<Vector3>();
        private static readonly List<Color> Colors = new List<Color>();

        [Conditional(ConditionEditor), Conditional(ConditionDebug)]
        public static void Line(Vector3 from, Vector3 to, Color color)
        {
            if (!Enabled) return;

            Vertices.Add(from);
            Vertices.Add(to);
            Colors.Add(color);
            Colors.Add(color);
        }

        [Conditional(ConditionEditor), Conditional(ConditionDebug)]
        public static void Circle(Vector3 center, float radius, Color color)
        {
            Arc(center, Vector3.forward, radius, 180f, color);
        }

        [Conditional(ConditionEditor), Conditional(ConditionDebug)]
        public static void Sector(Vector3 center, Vector3 forward, float radius, float angle, Color color, int spokes = 0)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;

            forward.Normalize();
            Arc(center, forward, radius, angle, color);

            Line(center, center + Quaternion.Euler(0f, -angle, 0f) * forward * radius, color);
            Line(center, center + Quaternion.Euler(0f, angle, 0f) * forward * radius, color);

            for (int i = 1; i < spokes; i++)
            {
                float a = Mathf.Lerp(-angle, angle, (float)i / spokes);
                Line(center, center + Quaternion.Euler(0f, a, 0f) * forward * radius, color);
            }
        }

        [Conditional(ConditionEditor), Conditional(ConditionDebug)]
        public static void WireSphere(Vector3 center, float radius, Color color)
        {
            Circle(center, radius, color);
            Ring(center, radius, Vector3.right, Vector3.up, color);
            Ring(center, radius, Vector3.forward, Vector3.up, color);
        }

        private static void Arc(Vector3 center, Vector3 forward, float radius, float angle, Color color)
        {
            if (!Enabled || radius <= 0f) return;

            int segments = Mathf.Max(4, Mathf.CeilToInt(CircleSegments * angle / 180f));
            Vector3 previous = center + Quaternion.Euler(0f, -angle, 0f) * forward * radius;

            for (int i = 1; i <= segments; i++)
            {
                float a = Mathf.Lerp(-angle, angle, (float)i / segments);
                Vector3 point = center + Quaternion.Euler(0f, a, 0f) * forward * radius;
                Line(previous, point, color);
                previous = point;
            }
        }

        private static void Ring(Vector3 center, float radius, Vector3 axisA, Vector3 axisB, Color color)
        {
            if (!Enabled || radius <= 0f) return;

            Vector3 previous = center + axisA * radius;

            for (int i = 1; i <= CircleSegments; i++)
            {
                float a = i * Mathf.PI * 2f / CircleSegments;
                Vector3 point = center + (axisA * Mathf.Cos(a) + axisB * Mathf.Sin(a)) * radius;
                Line(previous, point, color);
                previous = point;
            }
        }

#if UNITY_EDITOR || APP_DEBUG
        [DefaultExecutionOrder(10000)]
        private sealed class DebugShapesRenderer : MonoBehaviour
        {
            private static readonly int ZTestId = Shader.PropertyToID("_ZTest");
            private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");
            private static readonly int CullId = Shader.PropertyToID("_Cull");
            private static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
            private static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");

            private readonly List<int> _indices = new List<int>();
            private Mesh _mesh;
            private Material _material;

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            private static void ResetStatics()
            {
                Enabled = true;
                Vertices.Clear();
                Colors.Clear();
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            private static void Create()
            {
                var go = new GameObject(nameof(DebugShapes));
                go.hideFlags = HideFlags.HideInHierarchy;
                DontDestroyOnLoad(go);
                go.AddComponent<DebugShapesRenderer>();
            }

            private void Awake()
            {
                var shader = Shader.Find("Hidden/Internal-Colored");
                if (shader == null)
                {
                    UnityEngine.Debug.LogWarning($"{nameof(DebugShapes)}: Hidden/Internal-Colored が見つからないので、線を描きません。");
                    enabled = false;
                    return;
                }

                _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                _material.SetInt(ZTestId, (int)CompareFunction.Always);
                _material.SetInt(ZWriteId, 0);
                _material.SetInt(CullId, (int)CullMode.Off);
                _material.SetInt(SrcBlendId, (int)BlendMode.SrcAlpha);
                _material.SetInt(DstBlendId, (int)BlendMode.OneMinusSrcAlpha);
                _material.renderQueue = (int)RenderQueue.Overlay;

                _mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
                _mesh.MarkDynamic();
            }

            private void LateUpdate()
            {
                _mesh.Clear();

                if (Vertices.Count > 0)
                {
                    _indices.Clear();
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        _indices.Add(i);
                    }

                    _mesh.indexFormat = Vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                    _mesh.SetVertices(Vertices);
                    _mesh.SetColors(Colors);
                    _mesh.SetIndices(_indices, MeshTopology.Lines, 0);

                    var renderParams = new RenderParams(_material)
                    {
                        worldBounds = new Bounds(Vector3.zero, Vector3.one * 100000f),
                        shadowCastingMode = ShadowCastingMode.Off,
                        receiveShadows = false,
                    };
                    Graphics.RenderMesh(renderParams, _mesh, 0, Matrix4x4.identity);
                }

                Vertices.Clear();
                Colors.Clear();
            }

            private void OnDestroy()
            {
                Destroy(_mesh);
                Destroy(_material);
            }
        }
#endif
    }
}
