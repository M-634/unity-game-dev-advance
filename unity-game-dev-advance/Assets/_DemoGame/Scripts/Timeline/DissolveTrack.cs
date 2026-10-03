using UnityEngine;
using UnityEngine.Timeline;

namespace DemoGame
{
    // GameObject の下の Renderer すべてを、DemoGame/Toon の _Dissolve で溶かす（消す／現す）トラック
    // モデルを差し替えても、子の Renderer を集め直すだけで動く
    // Animation Track でマテリアルを動かすと MaterialPropertyBlock になり SRP Batcher から外れるので、自作にしている
    [TrackColor(0.95f, 0.6f, 0.2f)]
    [TrackBindingType(typeof(GameObject))]
    [TrackClipType(typeof(DissolveClip))]
    public class DissolveTrack : TrackAsset
    {
    }
}
