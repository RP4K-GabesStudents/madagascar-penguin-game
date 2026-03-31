using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Cutscene.TimeLine
{
    [System.Serializable]
    public class BlackBarsBehaviour : PlayableBehaviour
    {
        [SerializeField] private float barHeight;
        [SerializeField] private float fadeOutTime;
        
        public float BarHeight => barHeight;
        public float FadeOutTime => fadeOutTime;
    }
}
