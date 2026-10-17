using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Section.One.Timeline
{
    public class TransformMoveMixer : PlayableBehaviour
    {
        public TimelineClip[] Clips;
        public PlayableDirector Director;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var target = playerData as Transform;
            if (target == null || Director == null || Clips == null || Clips.Length == 0)
            {
                return;
            }

            double time = Director.time;
            TimelineClip selected = Clips[0];
            foreach (var clip in Clips)
            {
                if (clip.start > time) break;
                selected = clip;
            }

            var asset = selected.asset as TransformMoveClip;
            if (asset == null) return;

            var start = asset.startPoint.Resolve(Director);
            var end = asset.endPoint.Resolve(Director);
            if (start == null || end == null) return;

            if (start == target || end == target || start.IsChildOf(target) || end.IsChildOf(target))
            {
                return;
            }

            float progress = selected.duration > 0.0
                ? Mathf.Clamp01((float)((time - selected.start) / selected.duration))
                : (time >= selected.start ? 1f : 0f);
            target.position = Vector3.Lerp(start.position, end.position, progress);
        }
    }
}
