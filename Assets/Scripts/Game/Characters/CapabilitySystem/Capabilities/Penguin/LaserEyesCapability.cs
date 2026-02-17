using System;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin;
using Game.InventorySystem;
using Game.Items.Weapons;
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
        
        protected override void OnBound()
        {
            base.OnBound();
            _inventory = GetComponent<InventoryCapability>();                               
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
            SpawnLaser_ServerRpc(cam.forward * TargetDistance + cam.position);
        }

        [ServerRpc]
        private void SpawnLaser_ServerRpc(Vector3 target, ServerRpcParams info = default)
        {
            Vector3[] locations = new Vector3[_stats.NumProjectiles];
            Quaternion[] rotations = new Quaternion[_stats.NumProjectiles];
            int numprojectiles = _stats.NumProjectiles;
            if (_inventory != null)
            {
                if (_inventory.CurrentSelectedItem is GenericWeapon weapon)
                {
                    numprojectiles += weapon.additionalProjectiles;    
                }
            }
            for (var index = 0; index < eyes.Length; index++)
            {
                var trans = eyes[index];
                Vector3 pos = trans.position;
                Quaternion rot = trans.rotation;
                locations[index] = pos;
                rotations[index] = rot;
                
                Vector3 laserDir = (target - pos).normalized;
                Quaternion laserRot = Quaternion.LookRotation(laserDir, laserDir);
                for (int i = 0; i < numprojectiles; ++i)
                {
                    float ang = _stats.Inaccuracy;
                    float pitch = Random.Range(-ang, ang);
                    float yaw = Random.Range(-ang, ang);
                    var x = PoolingManager.SpawnObject(_stats.Projectile.name);
                    
                    x.transform.SetPositionAndRotation(pos, laserRot * Quaternion.Euler(pitch, yaw, 0));
                    //Laser l1 = Instantiate(_stats.Projectile, pos, laserRot * Quaternion.Euler(pitch, yaw, 0));
                    ((IPoolable)x).Spawn(info.Receive.SenderClientId);
                }
            }

            PlayParticle_ClientRpc();
            // PlayParticle_ClientRpc(locations, rotations);
        }
        
    
        [ClientRpc]
        private void PlayParticle_ClientRpc()
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