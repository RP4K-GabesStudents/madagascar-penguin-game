using Game.Characters;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;


namespace AbilitySystem.Abilities
{
    public abstract class GenericAbility : NetworkBehaviour
    {
        protected GenericCharacter _oner;
        [SerializeField] protected AbilityStats abilityStats;


        [ServerRpc]
        public void SetOwner_ServerRpc([NotNull] GenericCharacter oner)
        {
            if (_oner != oner)
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