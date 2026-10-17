using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Section.One.Timeline
{
    public class TransformMoveClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("始点の目印。移動対象自身や、その子オブジェクトは指定しないでください。")]
        public ExposedReference<Transform> startPoint;

        [Tooltip("終点の目印。ワールド座標で移動します。")]
        public ExposedReference<Transform> endPoint;
        
        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return Playable.Create(graph);
        }
    }
}
