using System;
using UnityEngine;

namespace Game.Potions
{
    public class PotionCloud : MonoBehaviour
    {
        [SerializeField] private Material tempPotionCloud;
        [SerializeField] private ParticleSystem lingeringParticles;
        [SerializeField] private ParticleSystem smokeParticles;
        
        public void InheritMaterial(Material masterial)
        {
            Color color = masterial.GetColor("_GradientTop");
            Color c = masterial.GetColor("_GradientBottom");
            var gradient = new ParticleSystem.MinMaxGradient(new Gradient()
            {
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1, 0.0f)
                },
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(color, 0.5f),
                    new GradientColorKey(c, 2)
                },
                mode = GradientMode.Fixed
            })
            {
                mode = ParticleSystemGradientMode.RandomColor
            };
            var main = lingeringParticles.main;
            main.startColor = gradient;
            var module = smokeParticles.main;
            module.startColor = gradient;
        }

        private void PlayPartice()
        {
            smokeParticles.Play();
        }

        private void StopPartice()
        {
            smokeParticles.Stop();
        }
        private void Awake()
        {
            InheritMaterial(tempPotionCloud);    
        }
    }
}
