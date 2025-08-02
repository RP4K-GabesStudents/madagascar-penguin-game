using UnityEngine;

namespace UI.Transition
{
    [CreateAssetMenu(fileName = "TransitionInfo", menuName = "SOAP/TransitionInfo")]
    public class PenguinSelectorStats : ScriptableObject
    {
        [SerializeField] private AudioClip lightActiveSound;
        
        [SerializeField] private float frontLightTime;
        [SerializeField] private AnimationCurve frontLights;
        
        [SerializeField] private float backLightsTime;
        [SerializeField] private AnimationCurve backLights;

        [SerializeField] private AnimationCurve materialIntensity;
        
        public AudioClip LightActiveSound => lightActiveSound;

        private float _maxLightTime;

        private void OnEnable()
        {
            _maxLightTime = Mathf.Max(frontLightTime, backLightsTime);
        }

        public float MaxLightTime => _maxLightTime;
        public float FrontLightTime => frontLightTime;
        public AnimationCurve FrontLights => frontLights;
        public float BackLightsTime => backLightsTime;
        public AnimationCurve BackLights => backLights; 
        public AnimationCurve MaterialIntensity => materialIntensity;
    }
}
