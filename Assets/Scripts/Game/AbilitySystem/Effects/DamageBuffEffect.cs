using System;
using Game.penguin;
using UnityEngine;

namespace Game.AbilitySystem.Effects
{
    public class DamageBuffEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private EffectStats effectStats;
        [SerializeField] private float curDuration;
        private float _originalDamage;
        private float _curDamage;
        PlayerController _player;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _originalDamage = _player.ProjectileStats.Damage;
            _curDamage = _originalDamage;
        }

        private void Update()
        {
            if(curDuration > 0 && effectStats.IsActive)
            {
                OnEffectActivated(_player);
            }

            if (effectStats.IsActive == false || curDuration <= 0)
            {
                OnEffectDeactivated(_player);
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            effectStats.IsActive = true;
            curDuration = effectStats.Duration;
        }
        
        public void OnEffectActivated(PlayerController player)
        {
            player.ProjectileStats.Damage = _curDamage += effectStats.DamageBuff;
        }

        public void OnEffectDeactivated(PlayerController player)
        {
            player.ProjectileStats.Damage = _originalDamage;
        }
    }
}