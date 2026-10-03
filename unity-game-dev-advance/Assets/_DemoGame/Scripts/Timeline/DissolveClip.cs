using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DemoGame
{
    // クリップの間に _Dissolve を from → to へ動かす（Ease In Out）。長さはクリップの長さで決まる
    [Serializable]
    public class DissolveClip : PlayableAsset, ITimelineClipAsset
    {
        [Range(0f, 1f)]
        public float From = 0f;

        [Range(0f, 1f)]
        public float To = 1f;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DissolveBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.From = From;
            behaviour.To = To;
            return playable;
        }
    }
}
