using UnityEngine;

namespace AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "AbilityStats", menuName = "AbilitySystem/Ability")]
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
