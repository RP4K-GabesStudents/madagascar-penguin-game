using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cutscene.TimeLine
{ 
    [TrackColor(0.05f, 0.05f, 0.05f)] 
    [TrackClipType(typeof(BlackBarsClip))] 
    [TrackBindingType(typeof(RectTransform))]
    public class BlackBarsTrack : TrackAsset
    {
        //called by the unity timeline to create the track when black bar is added
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<BlackBarsMixerBehaviour>.Create(graph, inputCount);
            mixer.GetBehaviour().Director = go.GetComponent<PlayableDirector>();
            return mixer;
        }
    }
}
