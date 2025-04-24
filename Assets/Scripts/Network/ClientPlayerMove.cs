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
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Player player;
     
        private void Awake()
        {
            playerInput.enabled = false;
            playerController.enabled = false;
            player.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                playerController.enabled = true;
                playerInput.enabled = true;
                player.enabled = true;
            }
        }
    }
}
