using System.Collections;
using Game.Characters;
using TMPro;
using UI.Transition;
using Unity.Cinemachine;
using UnityEngine;

namespace UI
{
    public class PenguinSelector : MonoBehaviour
    {
        
        [SerializeField] private PenguinSelectorStats selectorStats;
        [SerializeField] private GenericCharacter target;
        [SerializeField] private TextMeshPro textObject;
        
        [Header("Objects")] 
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Light[] backingLights;
        [SerializeField] private Light[] frontLights;
        [SerializeField] private Animator penguinAnimator;
        [SerializeField] private Material targetMaterial;
        
        [SerializeField] private CinemachineCamera cinemachineCamera;
        
        private static readonly int IntensityID = Shader.PropertyToID("_Intensity");

        private CinemachineBrain _main;
        private bool _isFadedIn;
        
        
        
        protected void Start()
        {
            _main ??= Camera.main.GetComponent<CinemachineBrain>();
            textObject.text = target.name;
            
        }

        public void Select()
        {
            _main ??= Camera.main.GetComponent<CinemachineBrain>();
            StartCoroutine(FadeIn());
            
            cinemachineCamera.enabled = true;

        }

        public void Deselect()
        {
            StopAllCoroutines();
            if(_isFadedIn) StartCoroutine(FadeOut());
            _isFadedIn = false;
            
            cinemachineCamera.enabled = false;

        }


        private IEnumerator FadeOut()
        {
            EvaluateLights(1,1);
            float t = 0;
            textObject.alpha = 1;
            while (t < selectorStats.MaxLightTime)
            {
                t += Time.deltaTime;

                EvaluateLights(Mathf.Clamp01(1 - (t / selectorStats.FrontLightTime)), Mathf.Clamp01(1 - (t / selectorStats.BackLightsTime)));
                textObject.alpha = 1-(t / selectorStats.MaxLightTime);
                yield return null;
            }

            textObject.alpha = 1;
            textObject.gameObject.SetActive(false);
            EvaluateLights(0,0);
        }
        
        
        private IEnumerator FadeIn()
        {
            EvaluateLights(0,0);

            yield return new WaitForSeconds(_main.DefaultBlend.BlendTime);
            _isFadedIn = true;
            
            audioSource.PlayOneShot(selectorStats.LightActiveSound);
            
            textObject.gameObject.SetActive(true);

            float t = 0;
            while (t < selectorStats.MaxLightTime)
            {
                t += Time.deltaTime;
                EvaluateLights(Mathf.Clamp01((t / selectorStats.FrontLightTime)), Mathf.Clamp01((t / selectorStats.BackLightsTime)));
                yield return null;
            }
            EvaluateLights(1,1);

        }

        private void EvaluateLights(float percent1, float percent2)
        {
            float intensity = selectorStats.FrontLights.Evaluate(percent1);
            foreach (Light l in frontLights)
            {
                l.intensity = intensity;
            }
            intensity =  selectorStats.BackLights.Evaluate(percent2);
            foreach (Light l in backingLights)
            {
                l.intensity = intensity;
            }
            targetMaterial.SetFloat(IntensityID, selectorStats.MaterialIntensity.Evaluate(percent2));
        }


        public void ChooseCharacter()
        {
            Debug.Log("implement on chosen effect");
        }
        public GenericCharacter Character => target;
    }
}
