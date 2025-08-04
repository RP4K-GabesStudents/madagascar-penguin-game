using System;
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
        private float t;


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
            StartCoroutine(FadeOut());
            cinemachineCamera.enabled = false;

        }

        private void OnEnable()
        {
            textObject.alpha = 1;
            textObject.gameObject.SetActive(false);
            EvaluateLights(0,0);
        }


        private IEnumerator FadeOut()
        {
            EvaluateLights(1,1);
            
            textObject.alpha = 1;
            while (t >= 0)
            {
                t -= Time.deltaTime * 2;

                EvaluateLights(Mathf.Clamp01((t / selectorStats.FrontLightTime)), Mathf.Clamp01((t / selectorStats.BackLightsTime)));
                textObject.alpha = (t / selectorStats.MaxLightTime);
                yield return null;
            }

            t = 0;

            textObject.alpha = 1;
            textObject.gameObject.SetActive(false);
            EvaluateLights(0,0);
        }
        
        
        private IEnumerator FadeIn()
        {
            EvaluateLights(0,0);
            yield return new WaitForSeconds(_main.DefaultBlend.BlendTime);
         
            
            audioSource.PlayOneShot(selectorStats.LightActiveSound);
            
            textObject.gameObject.SetActive(true);

            while (t <= selectorStats.MaxLightTime)
            {
                t += Time.deltaTime;
                EvaluateLights(Mathf.Clamp01((t / selectorStats.FrontLightTime)), Mathf.Clamp01((t / selectorStats.BackLightsTime)));
                yield return null;
            }

            t = selectorStats.MaxLightTime;
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
