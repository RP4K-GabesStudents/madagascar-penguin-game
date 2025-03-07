using System.Collections;
using penguin;
using UnityEngine;

namespace Abilities
{
    public abstract class BaseWeaponGun : MonoBehaviour
    {
        protected PlayerController _oner;
        protected Animator _animator;
        private bool _isOnCooldown;
        private Coroutine _reFire;
        private float _curLifeTime;
        [SerializeField] protected AbilityStats abilityStats;
        public bool IsAnimationBound => abilityStats.IisAnimationBound;
        public bool IsFullyAutomatic => abilityStats.FullyAutomatic;

        public void Begin()
        {
            //when we begin left-clicking
            if (abilityStats.FullyAutomatic)
            {
                _reFire = StartCoroutine(RefireLoop());
            }
            else if(CanBeUsed())
            {
                Execute();
            }
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

        public void SetOwner(PlayerController oner)
        {
            _oner = oner;
            _animator = _oner.GetComponentInChildren<Animator>();
            _curLifeTime = abilityStats.LifeTime;
            
            //blue
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