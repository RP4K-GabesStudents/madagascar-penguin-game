using UnityEngine;

namespace UI.Transition
{
    [CreateAssetMenu(fileName = "TransitionInfo", menuName = "SOAP/TransitionInfo")]
    public class PenguinSelectorStats : ScriptableObject
    {
        [SerializeField] private AudioClip lightActiveSound;
        
        [SerializeField] private float frontLightOffTime;
        [SerializeField] private AnimationCurve frontLights;
        
        [SerializeField] private float backLightsOffTime;
        [SerializeField] private AnimationCurve backLightsOff;
        
        [SerializeField] private float backLightsOnTime;
        [SerializeField] private AnimationCurve backLightsOn;
        [SerializeField] private AnimationCurve materialIntensity;
        
        public AudioClip LightActiveSound => lightActiveSound;
        
        
        public float FrontLightOffTime => frontLightOffTime;
        public AnimationCurve FrontLights => frontLights;
        public float BackLightsOffTime => backLightsOffTime;
        public AnimationCurve BackLightsOff => backLightsOff;
        
        
        
        
        public float BackLightsOnTime => backLightsOnTime;
        public AnimationCurve BackLightsOn => backLightsOn; 
        public AnimationCurve MaterialIntensity => materialIntensity;
    }
}
