using System;
using UnityEngine;
using Cutscene.Core;
using Cutscene.Managers;
using Unity.Netcode;

namespace Cutscene.Cutscenes
{
    public class TriggerBasedCutscene : NetworkBehaviour, ICutscenes
    {
        [SerializeField] private CutsceneManager cutsceneManager;
        [SerializeField] private GameObject triggerObject;
        [SerializeField] private bool requireAllPlayers;
        private NetworkVariable<int> _playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        private int _loadedPlayers;
        
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void LoadPlayers_ServerRPC()
        {
            _loadedPlayers++;
        }

        
        private void OnTriggerEnter(Collider other)
        {
            _playerCount.Value++;
            bool playersLoaded = _loadedPlayers == NetworkManager.Singleton.ConnectedClients.Count;
            bool canPlayCutscene = playersLoaded && (!requireAllPlayers || _playerCount.Value == _loadedPlayers);
            
            if (canPlayCutscene)
            {
                PlayCutscene_ClientRpc();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _playerCount.Value--;
        }

        public override void OnNetworkSpawn()
        {
            LoadPlayers_ServerRPC();
        } 

        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PlayCutscene_ClientRpc()
        {
            cutsceneManager.Instance.PlayCutscene(this);
        }
        
        
    }
}
