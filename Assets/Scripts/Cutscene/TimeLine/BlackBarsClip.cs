using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cutscene.TimeLine
{
    public class BlackBarsClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] private BlackBarsBehaviour template = new ();
        [SerializeField] private List<ExposedReference<CanvasGroup>> canvasTohide; //ExposedReference is like a ticket that can be stored anywhere and is accessible by the playable director rather than being a direct reference
        public ClipCaps clipCaps => ClipCaps.Blending; //ClipCaps are an enumeration withing the timeline
        
        //creates new blackbars behaviour instance and makes is playable so the timeline knows how to run it
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            Playable playable = ScriptPlayable<BlackBarsBehaviour>.Create(graph, template);
            return playable;
        }

        
        //internal is only accessible within the assembly definition
        //IExposedPropertyTable is like a chef in the kitchen, it reads the ticker(exposedreference) and knows what rules it must follow
        internal List<CanvasGroup> ResolveCanvases(IExposedPropertyTable resolver)
        {
            List<CanvasGroup> resolved = new List<CanvasGroup>();
            foreach (var reference in canvasTohide)
            {
                //getting the original canvas back
                var group = reference.Resolve(resolver);
                if (group != null) resolved.Add(group);
            }
            return resolved;
        }
    }
}
