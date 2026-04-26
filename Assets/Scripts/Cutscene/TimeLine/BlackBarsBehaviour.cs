using UnityEngine;
using UnityEngine.Playables;

namespace Cutscene.TimeLine
{
    [System.Serializable]
    public class BlackBarsBehaviour : PlayableBehaviour
    {
        public float barHeight   = 120f;
        public float fadeOutTime = 0.5f;

        public float BarHeight   => barHeight;
        public float FadeOutTime => fadeOutTime;
    }
}