using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "AbilityStats", menuName = "Scriptable Objects/AbilityStats")]
    public class AbilityStats : ScriptableObject
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private bool fullyAutomatic;
        [SerializeField] private bool isAnimationBound;
        [SerializeField] private float useSpeed;
        [SerializeField] private float lifeTime;
        
        
        public Sprite Icon => icon;
        public bool FullyAutomatic => fullyAutomatic;
        public bool IisAnimationBound => isAnimationBound;

        public float UseSpeed => useSpeed;
        public float LifeTime => lifeTime;
    }
}
