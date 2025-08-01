using UnityEngine;

namespace Game.Characters
{
    [CreateAssetMenu(fileName = "Generic Character", menuName = "Characters/Generic Character", order = 1)]
    public class CharacterStats : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private float hp = 0;
        [SerializeField, Range(0,1)] private float baseResistance = 0;
        

        //[Header("Abilities")] 
       // [SerializeField] private GenericAbility[] defaultAbilities;


        public float Hp => hp;
        public float BaseResistance => baseResistance;
       // public GenericAbility[] DefaultAbilities => defaultAbilities;
    }
}