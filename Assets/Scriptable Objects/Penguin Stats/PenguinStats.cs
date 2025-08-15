using Game.Characters;
using Game.Characters.Stats;
using UnityEngine;

namespace Scriptable_Objects.Penguin_Stats
{
    [CreateAssetMenu(fileName = "PenguinStats", menuName = "Characters/Penguin")]
    public class PenguinStats : CharacterStats
    {
        [Header("Movement")]
        [SerializeField] private float slideSpeed = 0;
        [SerializeField] private float slideCooldown = 0;
        
        public float SlideSpeed => slideSpeed;
        public float SlideCooldown => slideCooldown;

    }
}
