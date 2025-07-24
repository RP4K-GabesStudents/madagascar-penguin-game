
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

        public override void Execute()
        {
            ShootProjectile_ServerRpc();
            
        }

        [ServerRpc(RequireOwnership = true)] // << Any client can shoot, but we need to validate the server.
        void ShootProjectile_ServerRpc(ServerRpcParams rpcParams = default)
        {

            Vector3 origin = _oner.Head.position;
            Vector3 cameraForward = _oner.Head.forward * 100 + origin;

            foreach (Transform eye in _oner.Eyes)
            {
                Vector3 laserDir = (cameraForward - eye.position).normalized;
                ShootProjectile(origin, laserDir, rpcParams.Receive.SenderClientId);
            }

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