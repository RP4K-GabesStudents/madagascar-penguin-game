using System;
using System.Collections;
using Abilities;
using Inventory;
using UnityEngine;

namespace Game.Items.Weapons
{
    public abstract class GenericWeapon : Item
    {
        protected Animator _animator;
        private bool _isOnCooldown;
        private Coroutine _reFire;
        private float _curLifeTime;
        [SerializeField] protected WeaponStats abilityStats;

        private void Start()
        {
            
        }

        public override bool CanBeUsed()
        {
            return base.CanBeUsed() && !_isOnCooldown;
        }

        public void Use()
        {
            
        }
        public abstract void Execute();
        
    }
}