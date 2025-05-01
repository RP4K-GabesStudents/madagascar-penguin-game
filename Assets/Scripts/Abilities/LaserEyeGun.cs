using System;
using Managers;
using Objects;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Abilities
{
    public class LaserEyeGun : BaseWeaponGun
    {
        [SerializeField] private Laser laser;
        public override void Execute()
        {
            Vector3 cameraForward = _oner.headXRotator.forward * 100 + _oner.headXRotator.position;
            Vector3 leftLaserDir = (cameraForward - _oner.laserSpawn.position).normalized;
            Vector3 rightLaserDir = (cameraForward - _oner.laserSpawn2.position).normalized;
            
            ShootProjectile_ServerRpc(_oner.laserSpawn.position, _oner.laserSpawn2.position, leftLaserDir, rightLaserDir);
        }
        
        [ServerRpc(RequireOwnership = false)]// << Any client can shoot, but we need to validate the server.
        void ShootProjectile_ServerRpc(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ServerRpcParams rpcParams = default)
        {
            Laser l1 = Instantiate(laser, a, quaternion.LookRotation(c, Vector3.up));
            Laser l2 = Instantiate(laser, b, Quaternion.LookRotation(d, Vector3.up)); 
            
            l1.Init(StaticUtilities.EnemyAttackLayers, rpcParams.Receive.SenderClientId);
            l2.Init(StaticUtilities.EnemyAttackLayers, rpcParams.Receive.SenderClientId);
        }

    }
}