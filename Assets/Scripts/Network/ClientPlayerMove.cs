using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class ClientPlayerMove : NetworkBehaviour
    {
        [SerializeField] private MonoBehaviour[] ownershipRequiredComponents;
        
        private void Awake()
        {
            foreach(MonoBehaviour mb in ownershipRequiredComponents)
                mb.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                foreach(MonoBehaviour mb in ownershipRequiredComponents)
                    mb.enabled = true;
            }
        }
    }
}
