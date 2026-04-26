using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cutscene.TimeLine
{
    [System.Serializable]
    public class BlackBarsClip : PlayableAsset, ITimelineClipAsset
    {
        public BlackBarsBehaviour template = new BlackBarsBehaviour();

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<BlackBarsBehaviour>.Create(graph, template);
        }
    }
}