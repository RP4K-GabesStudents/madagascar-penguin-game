using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "WeaponStats", menuName = "Items/WeaponStats")]
    public class WeaponStats : ScriptableObject
    {
        [Header("Use / contact")]
        [SerializeField] private float useSpeed;
        [SerializeField] private float contactDamage;     // damage = contactDamage * weaponSpeed (positional)
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float swingSpeed;
        [SerializeField] private float knockbackForce;     // scales the contact knockback impulse
        [SerializeField] private float coolDownTime;

        [Header("Contact gating")]
        [Tooltip("Below this positional speed (units/sec) the weapon does no contact damage.")]
        [SerializeField] private float minContactSpeed = 1f;

        [Header("Movement modifiers (while equipped)")]
        [SerializeField] private float moveSpeedMultiplier = 1f;
        [SerializeField] private float maxSpeedMultiplier = 1f;

        [Header("Jump modifiers (while equipped)")]
        [SerializeField] private float jumpPowerMultiplier = 1f;
        [SerializeField] private int additionalAirJumps = 0;

        [Header("Laser modifiers (while equipped)")]
        [SerializeField] private int additionalLasers = 0;
        [Tooltip("Multiplies the laser capability's cooldown. <1 fires faster, >1 slower.")]
        [SerializeField] private float laserCooldownMultiplier = 1f;

        public float UseSpeed => useSpeed;
        public float ContactDamage => contactDamage;
        public float RotationSpeed => rotationSpeed;
        public float SwingSpeed => swingSpeed;
        public float KnockbackForce => knockbackForce;
        public float CoolDownTime => coolDownTime;
        public float MinContactSpeed => minContactSpeed;

        public float MoveSpeedMultiplier => moveSpeedMultiplier;
        public float MaxSpeedMultiplier => maxSpeedMultiplier;

        public float JumpPowerMultiplier => jumpPowerMultiplier;
        public int AdditionalAirJumps => additionalAirJumps;

        public int AdditionalLasers => additionalLasers;
        public float LaserCooldownMultiplier => laserCooldownMultiplier;
    }
}