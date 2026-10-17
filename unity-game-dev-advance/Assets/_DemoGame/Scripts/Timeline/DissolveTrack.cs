using UnityEngine;
using UnityEngine.Timeline;

namespace DemoGame
{
    [TrackColor(0.95f, 0.6f, 0.2f)]
    [TrackBindingType(typeof(GameObject))]
    [TrackClipType(typeof(DissolveClip))]
    public class DissolveTrack : TrackAsset
    {
    }
}
