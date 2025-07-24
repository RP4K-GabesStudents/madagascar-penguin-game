using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public abstract class BaseCapability : NetworkBehaviour // Must be because of [ServerRPC] and [ClientRPC]
    {
        public Characters.CapabilityStats genericStats;
        
        protected GenericCharacter owner;

        private void Awake()
        {
            transform.root.TryGetComponent(out owner);
            if (owner == null)
            {
                Debug.LogAssertion($"The capability is attached to {transform.root.name} which cannot handle capabilities as it's not a {typeof(GenericCharacter)}", gameObject);
            }
            OnBound();
        }

        protected virtual void OnBound() { }

        public abstract bool CanExecute();
        protected abstract void Execute();
        
        public virtual void TryExecute()
        {
            if (CanExecute()) Execute();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            enabled = IsOwner;
        }
        
        [CanBeNull]
        public T GetStats<T>() where T : Characters.CapabilityStats => genericStats as T;
    }
}