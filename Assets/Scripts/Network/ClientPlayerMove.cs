using System;
using System.Text;
using penguin;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
