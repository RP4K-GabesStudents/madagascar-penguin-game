using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "WeaponStats", menuName = "Items/WeaponStats")]
    public class WeaponStats : ScriptableObject
    {
        
        [SerializeField] private float useSpeed;
        [SerializeField] private float contactDamage;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float swingSpeed;
        [SerializeField] private float knockbackForce;
        
        public float UseSpeed => useSpeed;
        public float ContactDamage => contactDamage;
        public float RotationSpeed => rotationSpeed;
        public float SwingSpeed => swingSpeed;
        public float KnockbackForce => knockbackForce;
    }
}
