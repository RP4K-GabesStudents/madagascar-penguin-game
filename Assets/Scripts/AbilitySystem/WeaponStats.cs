using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "WeaponStats", menuName = "Scriptable Objects/WeaponStats")]
    public class WeaponStats : ScriptableObject
    {
        
        [SerializeField] private float useSpeed;
        [SerializeField] private float contactDamage;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float swingSpeed;
        
        
        public float UseSpeed => useSpeed;
        public float ContactDamage => contactDamage;
        public float RotationSpeed => rotationSpeed;
        public float SwingSpeed => swingSpeed;
    }
}
