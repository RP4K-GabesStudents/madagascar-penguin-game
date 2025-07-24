using UnityEngine;

namespace Game.Characters
{
    [CreateAssetMenu(fileName = "Additional Movement Stats", menuName = "Characters/Additional Movement Stats", order = 30)]
    public class AdditionalMovementStats : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float crouchSpeed = 0;
        [SerializeField] private float sprintSpeed = 0;
        
        public float SprintSpeed => sprintSpeed;
        public float CrouchSpeed => crouchSpeed;
    }
}