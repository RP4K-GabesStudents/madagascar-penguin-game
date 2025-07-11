using Game.Characters;
using Unity.Netcode;
using UnityEngine;


namespace AbilitySystem.Abilities
{
    public abstract class GenericAbility : NetworkBehaviour
    {
        protected GenericCharacter _oner;
        [SerializeField] protected AbilityStats abilityStats;
        

        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            base.OnOwnershipChanged(previous, current);
            if (previous != current)
            {
                UnbindFromOner();
            }
            _oner = oner;
            BindToOner();
        }

        protected abstract void BindToOner();
        protected abstract void UnbindFromOner();

    }
}