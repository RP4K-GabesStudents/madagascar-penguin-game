using Game.Characters; 
using UnityEngine;

namespace Game.AbilitySystem.Effects
{
    public class SpontaneousCombustionEffect : MonoBehaviour, IEffect
    {
        [SerializeField] private EffectStats effectStats;
        [SerializeField] private float curDuration;
        GenericCharacter _player;

        private void Awake()
        {
            _player = GetComponent<GenericCharacter>();
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

        public void OnEffectActivated(GenericCharacter player)
        {
            player.Health -= effectStats.BurnDamage;
        }

        public void OnEffectDeactivated(GenericCharacter player)
        {
            effectStats.IsActive = false;
        }
    }
}