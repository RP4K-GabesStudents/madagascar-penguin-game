using System.Collections;
using Abilities;
using Inventory;
using penguin;
using Unity.Netcode;
using UnityEngine;

namespace Items.Weapons
{
    public abstract class BaseWeapon : Item
    {
        protected Animator _animator;
        private bool _isOnCooldown;
        private Coroutine _reFire;
        private float _curLifeTime;
        [SerializeField] protected WeaponStats abilityStats;

        public void Begin()
        {
            //when we begin left-clicking
            if(!CanBeUsed()) return;
            Execute();
        }

        public void UseInstant()
        {
            if(CanBeUsed()) Execute();
        }

        public void End()
        {
            //at the end of our left click
            if (_reFire != null)
            {
                StopCoroutine(_reFire);
                _reFire = null;
            }
        }
        
        public virtual bool CanBeUsed()
        {
            return !_isOnCooldown;
        }

        private IEnumerator RefireLoop()
        {
            while (true)
            {
                yield return new WaitUntil(CanBeUsed);
                Execute();
            }
        }
        public abstract void Execute();

        private float GetCurrentLifeTime()
        {
            if(abilityStats.LifeTime <= 0)
            {
                return 1;
            }
            return _curLifeTime / abilityStats.LifeTime;
        }
        
    }
}