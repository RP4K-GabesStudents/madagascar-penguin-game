using Abilities;
using Game.Characters.World;
using Game.InventorySystem;
using Game.Objects;
using Unity.Netcode;
using UnityEngine;

namespace Game.Items.Weapons
{
    /// <summary>
    /// A weapon. On the ground it's a WorldItem you pick up; once equipped it's
    /// parented in the mouth and you attack by SPINNING the player, the weapon
    /// swings through the world and hurts what it touches.
    ///
    /// It is NOT a firer. It does not own an attack button (lasers fire off the
    /// character's OnAttack regardless). A weapon does two things:
    ///   1. Exposes WeaponStats so other capabilities (movement, jump, lasers)
    ///      can read modifiers while it's equipped. See InventoryCapability.
    ///   2. Deals passive CONTACT damage: damage scales with how fast the weapon
    ///      head is actually moving through space (positional delta, so spinning
    ///      in place still counts, which rigidbody.linearVelocity would miss).
    ///
    /// Damage authority (project choice: trust the attacker): contact detection
    /// and the speed measurement run on the OWNER's client so the swing feels
    /// instant. The hit is then routed through IDamageable.ApplyNetworkedDamage,
    /// which sends the victim's server Rpc; the server applies the result and it
    /// replicates. No server-side re-simulation of the swing.
    /// </summary>
    public class GenericWeapon : WorldItem, IHeldItem
    {
        [SerializeField] protected WeaponStats abilityStats;

        public WeaponStats Stats => abilityStats;

        protected Animator _animator;
        protected bool _isEquipped;

        // Owner-side speed tracking. World position last physics tick, used to
        // derive positional speed including rotation about the mouth anchor.
        private Vector3 _lastPos;
        private float _currentSpeed;
        private bool _hasLastPos;

        // ---- IHeldItem ----

        public virtual void OnEquip(GenericCharacter owner)
        {
            _isEquipped = true;
            _hasLastPos = false; // reset so the first tick doesn't read a stale delta
            if (!_animator) _animator = GetComponentInChildren<Animator>();
        }

        // Lasers fire off the character's attack independently; a weapon does
        // not consume these. Kept so special weapons can react if they want.
        public virtual void OnStartUse() { }
        public virtual void OnStopUse() { }

        public virtual void OnUnequip()
        {
            _isEquipped = false;
            _hasLastPos = false;
        }

        // ---- Contact damage ----

        private void FixedUpdate()
        {
            // Only the owner measures and arbitrates contact (trust-the-attacker).
            if (!_isEquipped || !IsOwner) return;

            Vector3 pos = transform.position;
            if (_hasLastPos)
                _currentSpeed = (pos - _lastPos).magnitude / Time.fixedDeltaTime;
            _lastPos = pos;
            _hasLastPos = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isEquipped || !IsOwner || abilityStats == null) return;
            if (_currentSpeed < abilityStats.MinContactSpeed) return;

            // Don't hit our own wielder.
            if (collision.transform.root == transform.root) return;

            // The thing we hit must be damageable to matter.
            if (!collision.collider.TryGetComponent(out IDamageable victim) &&
                !collision.transform.root.TryGetComponent(out victim))
                return;

            float damage = abilityStats.ContactDamage * _currentSpeed;

            // Knockback follows the weapon's own travel direction (the swing),
            // so spinning shoves the target the way the weapon was going.
            Vector3 dir = (transform.position - _lastPos);
            Vector3 knockback = (dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward)
                                * abilityStats.KnockbackForce;

            OnContactHit(victim, collision);

            // Networked, trust-the-attacker: victim's server Rpc applies it.
            victim.ApplyNetworkedDamage(damage, knockback);
        }

        /// <summary>
        /// Optional hook for special weapons (apply a status, spawn a hit VFX,
        /// etc.) the moment before damage is sent. Base does nothing.
        /// </summary>
        protected virtual void OnContactHit(IDamageable victim, Collision collision) { }
    }
}