using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Section.One.Timeline
{
    [TrackColor(0.2f, 0.7f, 0.8f)]
    [TrackClipType(typeof(TransformMoveClip))]
    [TrackBindingType(typeof(Transform))]
    public class TransformMoveTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var playable = ScriptPlayable<TransformMoveMixer>.Create(graph, inputCount);
            var mixer = playable.GetBehaviour();
            mixer.Clips = GetClips().OrderBy(clip => clip.start).ToArray();
            mixer.Director = graph.GetResolver() as PlayableDirector;
            return playable;
        }

        // エディターのプレビュー終了時に、編集前の位置を復元する。
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var target = director.GetGenericBinding(this) as Transform;
            if (target != null)
            {
                driver.AddFromName<Transform>(target.gameObject, "m_LocalPosition.x");
                driver.AddFromName<Transform>(target.gameObject, "m_LocalPosition.y");
                driver.AddFromName<Transform>(target.gameObject, "m_LocalPosition.z");
            }
            base.GatherProperties(director, driver);
        }
    }
}
