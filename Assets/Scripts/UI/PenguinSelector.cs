using System.Collections;
using UI.Transition;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PenguinSelector : Selectable
    {
        private PenguinSelector _currentPenguinSelector;
        
        [SerializeField] private PenguinSelectorStats selectorStats;
        
        [Header("Objects")] 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Light[] backingLights;
        [SerializeField] private Light[] frontLights;
        [SerializeField] private Animator penguinAnimator;
        [SerializeField] private Material targetMaterial;
        
        private static readonly int IntensityID = Shader.PropertyToID("_Intensity");

        private CinemachineBrain _main;
        private bool _isFadedIn;
        
        
        
        protected override void Start()
        {
            base.Start();
            
            _main = Camera.main.GetComponent<CinemachineBrain>();
        }

        public override void Select()
        {
            _currentPenguinSelector?.Deselect();
            _currentPenguinSelector = this;
            StartCoroutine(FadeIn());
        }

        public void Deselect()
        {
            StopAllCoroutines();
            if(_isFadedIn) StartCoroutine(FadeOut());
            _isFadedIn = false;
        }


        private IEnumerator FadeOut()
        {
            yield return null;

        }

        private IEnumerator FadeIn()
        {
            yield return new WaitForSeconds(_main.DefaultBlend.BlendTime);
            _isFadedIn = true;
            
            audioSource.PlayOneShot(selectorStats.LightActiveSound);

           
            float intensity = selectorStats.FrontLights.Evaluate(0);
            foreach (Light l in frontLights)
            {
                l.intensity = intensity;
            }

            float t = 0;
            while (t < selectorStats.BackLightsOnTime)
            {
                t += Time.deltaTime;
                float p = t / selectorStats.BackLightsOnTime;
                
                intensity = selectorStats.BackLightsOn.Evaluate(p);
                foreach (Light l in frontLights)
                {
                    l.intensity = intensity;
                }
                
                targetMaterial.SetFloat(IntensityID, selectorStats.MaterialIntensity.Evaluate(p));
                
                yield return null;
            }
            
            intensity = selectorStats.BackLightsOn.Evaluate(1);
            foreach (Light l in frontLights)
            {
                l.intensity = intensity;
            }
            targetMaterial.SetFloat(IntensityID, selectorStats.MaterialIntensity.Evaluate(1));

        }

    }
}
