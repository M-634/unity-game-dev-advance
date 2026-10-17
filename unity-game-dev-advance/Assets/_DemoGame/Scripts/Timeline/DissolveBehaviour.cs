using UnityEngine;
using UnityEngine.Playables;

namespace DemoGame
{
    public class DissolveBehaviour : PlayableBehaviour
    {
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        public float From;
        public float To = 1f;

        private GameObject _target;
        private Renderer[] _renderers;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var target = playerData as GameObject;
            if (target != _target)
            {
                _target = target;
                _renderers = target != null ? target.GetComponentsInChildren<Renderer>(true) : null;
            }

            double duration = playable.GetDuration();
            float t = duration > 0 ? Mathf.Clamp01((float)(playable.GetTime() / duration)) : 1f;
            Set(Mathf.Lerp(From, To, t * t * (3f - 2f * t)));
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (playable.GetTime() >= playable.GetDuration())
            {
                Set(To);
            }
        }

        private void Set(float value)
        {
            if (!Application.isPlaying || _renderers == null) return;

            SetDissolve(_renderers, value);
        }

        public static void SetDissolve(Renderer[] renderers, float value)
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                foreach (var material in renderer.materials)
                {
                    material.SetFloat(DissolveId, value);
                }
            }
        }
    }
}
