using Game.Characters.CapabilitySystem.Capabilities.AI;
using Managers.Movement.Stats;
using UnityEngine;

namespace Managers.Movement
{
    public interface IMoveType
    {
        public void Initialize(AIBrain brain, MoveTypeStats stats);
        public void Update();

        public void OnBecameActive();
    }
}
