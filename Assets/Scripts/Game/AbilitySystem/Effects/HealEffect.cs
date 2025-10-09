using Game.Characters;
using Game.Characters.World;
using UnityEngine;

namespace Game.AbilitySystem.Effects
{
    public class HealEffect : MonoBehaviour, IEffect
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
            if (effectStats.IsActive && curDuration > 0)
            {
                OnEffectActivated(_player);
                curDuration -= Time.deltaTime;
            }
            if (curDuration <= 0)
            {
                OnEffectDeactivated(_player);
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
            effectStats.IsActive = true;
            curDuration = effectStats.Duration;
        }

        public void OnEffectActivated(GenericCharacter player)
        {
            player.Health += effectStats.HealAmount;
        }

        public void OnEffectDeactivated(GenericCharacter player)
        {
            effectStats.IsActive = false;
        }
    }
}