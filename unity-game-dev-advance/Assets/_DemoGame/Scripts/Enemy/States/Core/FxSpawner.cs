using UnityEngine;

namespace DemoGame
{
    public static class FxSpawner
    {
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float scale = 1f, Transform parent = null)
        {
            if (prefab == null) return null;

            var instance = Object.Instantiate(prefab, position, rotation, parent);
            if (!Mathf.Approximately(scale, 1f))
            {
                instance.transform.localScale *= scale;
            }

            return instance;
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, float scale = 1f)
        {
            return Spawn(prefab, position, Quaternion.identity, scale);
        }

        public static GameObject Attach(GameObject prefab, Transform parent, Vector3 localPosition = default)
        {
            if (prefab == null || parent == null) return null;

            var instance = Object.Instantiate(prefab, parent);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;

            Vector3 s = parent.lossyScale;
            instance.transform.localScale = new Vector3(Inverse(s.x), Inverse(s.y), Inverse(s.z));
            return instance;
        }

        public static void Play(GameObject effect, bool clear = false)
        {
            if (effect == null) return;

            foreach (var ps in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (clear) ps.Clear(false);
                ps.Play(false);
            }
        }

        public static void Stop(GameObject effect)
        {
            if (effect == null) return;

            foreach (var ps in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private static float Inverse(float v) => Mathf.Abs(v) < 1e-5f ? 1f : 1f / v;
    }
}
