
using Managers;
using Objects;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace AbilitySystem.Abilities
{
    public class LaserEye : GenericAbility
    {
        [SerializeField] private Laser laser;
        [SerializeField] private ParticleSystem eyePrefab;
        private ParticleSystem[] _eyes;

        private void Execute()
        {
            Vector3 cameraForward = _oner.headXRotator.forward * 100 + _oner.headXRotator.position;
            Vector3 leftLaserDir = (cameraForward - _oner.laserSpawn.position).normalized;
            Vector3 rightLaserDir = (cameraForward - _oner.laserSpawn2.position).normalized;

            ShootProjectile_ServerRpc(_oner.laserSpawn.position, _oner.laserSpawn2.position, leftLaserDir, rightLaserDir);
        }

        [ServerRpc(RequireOwnership = true)] // << Any client can shoot, but we need to validate the server.
        void ShootProjectile_ServerRpc(ServerRpcParams rpcParams = default)
        {
           


            ShootProjectile_ClientRpc();
        }

        private void ShootProjectile(Vector3 a, Vector3 b, ulong owner)
        {
            Laser l1 = Instantiate(laser, a, quaternion.LookRotation(b, Vector3.up));
            l1.NetworkObject.SpawnWithOwnership(owner);
            l1.Init(StaticUtilities.EnemyAttackLayers, owner);
        }
        


        [ClientRpc]
        void ShootProjectile_ClientRpc()
        {
            foreach (ParticleSystem eye in _eyes)
            {
                eye.Play();
            }
        }

        protected override void BindToOner()
        {

            if (!IsServer) return;
            _oner.OnAttack += Execute;
            SpawnEyeEffects_ClientRpc();


        }

        [ClientRpc]
        private void SpawnEyeEffects_ClientRpc()
        {
            int n = _oner.Eyes.Length;
            _eyes = new ParticleSystem[n];
            for (var i = 0; i < n; i++)
            {
                var eyeTransforms = _oner.Eyes[i];
                _eyes[n] = Instantiate(eyePrefab, eyeTransforms);
            }
        }


        protected override void UnbindFromOner()
        {
            _oner.OnAttack -= Execute;
        }
    }
}