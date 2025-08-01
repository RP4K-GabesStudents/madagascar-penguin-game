using System;
using Game.Characters.Capabilities;
using Managers;
using Objects;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class LaserEyesCapability : BaseCapability
    {
        [SerializeField] private Transform[] eyes;
        private LaserEyeCapabilityStats _stats;
        private const float TargetDistance = 50;
        
        protected override void OnBound()
        {
            base.OnBound();
                                           
            _stats = genericStats as LaserEyeCapabilityStats;
            if (_stats == null) { Debug.LogAssertion($"Wrong stats assigned to object {name},expected {typeof(LaserEyeCapabilityStats)}, but retrieved {genericStats.GetType()}.", gameObject); }
        }

        public override bool CanExecute()
        {
            Debug.LogWarning("We (LASER EYES) want to be animation driven right? Do we want a cooldown as well? ", gameObject);
            return true;
        }

        protected override void Execute()
        {
            Debug.LogWarning("Optimize with Object Pool");
            SpawnLaser_ServerRpc(owner.Head.forward * TargetDistance + owner.Head.position);
        }

        [ServerRpc]
        private void SpawnLaser_ServerRpc(Vector3 target, ServerRpcParams info = default)
        {
            Vector3[] locations = new Vector3[_stats.NumProjectiles];
            Quaternion[] rotations = new Quaternion[_stats.NumProjectiles];

            for (var index = 0; index < eyes.Length; index++)
            {
                var trans = eyes[index];
                Vector3 pos = trans.position;
                Quaternion rot = trans.rotation;
                locations[index] = pos;
                rotations[index] = rot;
                
                Vector3 laserDir = (target - pos).normalized;
                Quaternion laserRot = Quaternion.LookRotation(laserDir, laserDir);
                for (int i = 0; i < _stats.NumProjectiles; ++i)
                {
                    float ang = _stats.Inaccuracy;
                    float pitch = Random.Range(-ang, ang);
                    float yaw = Random.Range(-ang, ang);
                    
                    Laser l1 = Instantiate(_stats.Projectile, pos, laserRot * Quaternion.Euler(pitch, yaw, 0));
                    l1.NetworkObject.SpawnWithOwnership(info.Receive.SenderClientId);
                    l1.Init(StaticUtilities.EnemyAttackLayers);
                }
            }

            PlayParticle_ClientRpc(locations, rotations);
        }

        [ClientRpc]
        private void PlayParticle_ClientRpc(Vector3[] location, Quaternion[] rotation)
        {
            for (int i = 0; i < location.Length; ++i)
            {
                ParticleSystem particles = Instantiate(_stats.ParticleSystem, location[i], rotation[i]);
                Destroy(particles.gameObject, particles.main.duration);
            }
        }

        private void OnEnable()
        {
            owner.OnAttack += TryExecute;
        }

        private void OnDisable()
        {
            owner.OnAttack -= TryExecute;
        }
    }
}