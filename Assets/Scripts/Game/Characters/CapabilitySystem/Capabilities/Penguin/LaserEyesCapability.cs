using Game.Characters.CapabilitySystem.Capabilities;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin;
using Managers.Pooling_System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin
{
    public class LaserEyesCapability : BaseCapability
    {
        [SerializeField] private Transform[] eyes;
        [SerializeField] private Transform cam;
        private LaserEyeCapabilityStats _stats;
        private InventoryCapability _inventory;
        private const float TargetDistance = 50;

        private float _laserCooldownEnd; // owner-side cooldown gate

        protected override void OnBound()
        {
            base.OnBound();
            _inventory = GetComponent<InventoryCapability>();
            _stats = genericStats as LaserEyeCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(LaserEyeCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }
        }

        public override bool CanExecute()
        {
            // Lasers always fire on attack (with or without a weapon equipped),
            // gated only by the laser cooldown. The equipped weapon modifies
            // that cooldown but never gates firing.
            return Time.time >= _laserCooldownEnd;
        }

        protected override void Execute()
        {
            // Resolve modifiers OWNER-side (trust-the-attacker), then bake the
            // resulting projectile count into the RPC so the server doesn't
            // have to re-read this client's selection.
            int projectiles = _stats.NumProjectiles;
            float cooldownMul = 1f;

            var weapon = _inventory ? _inventory.EquippedWeapon : null;
            if (weapon && weapon.Stats)
            {
                projectiles += weapon.Stats.AdditionalLasers;
                cooldownMul = weapon.Stats.LaserCooldownMultiplier;
            }

            _laserCooldownEnd = Time.time + (_stats.Cooldown * cooldownMul);

            SpawnLaser_Rpc(cam.forward * TargetDistance + cam.position, projectiles);
        }

        [Rpc(SendTo.Server)]
        private void SpawnLaser_Rpc(Vector3 target, int projectiles, RpcParams info = default)
        {
            for (var index = 0; index < eyes.Length; index++)
            {
                var trans = eyes[index];
                Vector3 pos = trans.position;

                Vector3 laserDir = (target - pos).normalized;
                Quaternion laserRot = Quaternion.LookRotation(laserDir, laserDir);
                for (int i = 0; i < projectiles; ++i)
                {
                    float ang = _stats.Inaccuracy;
                    float pitch = Random.Range(-ang, ang);
                    float yaw = Random.Range(-ang, ang);
                    var x = PoolingManager.SpawnObject(_stats.Projectile.name);

                    x.transform.SetPositionAndRotation(pos, laserRot * Quaternion.Euler(pitch, yaw, 0));
                    ((IPoolable)x).Spawn(info.Receive.SenderClientId);
                }
            }

            PlayParticle_Rpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PlayParticle_Rpc()
        {
            for (int i = 0; i < eyes.Length; ++i)
            {
                ParticleSystem particles = Instantiate(_stats.ParticleSystem, eyes[i]);
                Destroy(particles.gameObject, particles.main.duration);
            }
        }

        private void OnEnable()
        {
            _owner.OnAttack += TryExecute;
        }

        private void OnDisable()
        {
            _owner.OnAttack -= TryExecute;
        }
    }
}