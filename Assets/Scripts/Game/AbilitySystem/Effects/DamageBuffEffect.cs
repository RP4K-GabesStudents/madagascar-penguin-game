using System;
using Game.Characters;
using Game.Characters.World;
using UnityEngine;

namespace Game.AbilitySystem.Effects
{
    public class DamageBuffEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private EffectStats effectStats;
        [SerializeField] private float curDuration;
        private float _originalDamage;
        private float _curDamage;
        GenericCharacter _character;

        private void Awake()
        {
            _character = GetComponent<GenericCharacter>();
            //_originalDamage = _character.ProjectileStats.Damage;
            _curDamage = _originalDamage;
            Debug.LogWarning("Will need a new way to do this, probably use the data dictionary");
        }

        private void Update()
        {
            if(curDuration > 0 && effectStats.IsActive)
            {
               // OnEffectActivated(_player);
            }

            if (effectStats.IsActive == false || curDuration <= 0)
            {
                //OnEffectDeactivated(_player);
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            effectStats.IsActive = true;
            curDuration = effectStats.Duration;
        }
        
        public void OnEffectActivated(GenericCharacter player)
        {
            //player.ProjectileStats.Damage = _curDamage += effectStats.DamageBuff;
        }

        public void OnEffectDeactivated(GenericCharacter player)
        {
            //player.ProjectileStats.Damage = _originalDamage;
        }

    }
}