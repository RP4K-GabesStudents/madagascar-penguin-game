using System;
using Cutscene.Core;
using Cutscene.Managers;
using Unity.Netcode;
using UnityEngine;

namespace Cutscene.Cutscenes
{
    public class InstantCutscene : NetworkBehaviour, ICutscenes
    {
        [SerializeField] private CutsceneManager cutsceneManager;
        private int _loadedPlayers;
        

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void LoadPlayers_ServerRPC()
        {
            _loadedPlayers++;
            if (_loadedPlayers == NetworkManager.Singleton.ConnectedClients.Count)
            {
                PlayCutscene_ClientRpc();
            }
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
