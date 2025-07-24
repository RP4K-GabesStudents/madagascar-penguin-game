using System;
using Game.Characters.World;
using Game.penguin;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.AbilitySystem.Effects
{
    public class SpontaneousCombustionEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private EffectStats effectStats;
        [SerializeField] private float curDuration;
        PlayerController _player;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            while (effectStats.IsActive && curDuration > 0)
            {
                OnEffectActivated(_player);
                curDuration -= Time.deltaTime;
                if (curDuration <= 0)
                {
                    OnEffectDeactivated(_player);
                }
            }
        }
        private void OnCollisionEnter(Collision other)
        {
            effectStats.IsActive = true;
            curDuration = effectStats.Duration;
        }

        public void OnEffectActivated(PlayerController player)
        {
            player.Health -= effectStats.BurnDamage;
        }

        public void OnEffectDeactivated(PlayerController player)
        {
            effectStats.IsActive = false;
        }
    }
}