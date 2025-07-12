using AbilitySystem.Abilities;
using UnityEngine;

namespace Game.Characters
{
    [CreateAssetMenu(fileName = "GenericCharacter", menuName = "Characters/GenericCharacter")]
    public class CharacterStats : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private float hp = 0;
        [SerializeField, Range(0,1)] private float baseResistance = 0;
        
        [Header("Movement")]
        [SerializeField] private float speed = 0;
        [SerializeField] private float crouchSpeed = 0;
        [SerializeField] private float sprintSpeed = 0;
        [SerializeField] private float speedLimit = 0;
        [Space]
        [SerializeField] private float jumpPower = 0;
        [SerializeField] private float jumpCooldown = 0;

        [Header("Abilities")] 
        [SerializeField] private GenericAbility[] defaultAbilities;


        public float Hp => hp;
        public float BaseResistance => baseResistance;
        public float Speed => speed;
        public float SpeedLimit => speedLimit;
        public float SprintSpeed => sprintSpeed;
        public float JumpPower => jumpPower;
        public float JumpCooldown => jumpCooldown;
        public GenericAbility[] DefaultAbilities => defaultAbilities;
    }
}