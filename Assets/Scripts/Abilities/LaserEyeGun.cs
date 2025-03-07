using System;
using Objects;
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
            
            Instantiate(laser, _oner.laserSpawn.position, Quaternion.LookRotation(leftLaserDir, Vector3.up)).Init(_oner.gameObject);
            Instantiate(laser, _oner.laserSpawn2.position, Quaternion.LookRotation(rightLaserDir, Vector3.up)).Init(_oner.gameObject);
        }

    }
}