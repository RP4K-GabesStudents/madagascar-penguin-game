using System;
using Cutscene.Core;
using Cutscene.Managers;
using Unity.Netcode;
using UnityEngine;

namespace Cutscene.Cutscenes
{
    public class TimerBasedCutscene : NetworkBehaviour, ICutscenes
    {
        [SerializeField] private CutsceneManager cutsceneManager;
        [SerializeField] private float timeTillCutscene;
        private NetworkVariable<float> _time;
        
        private int _loadedPlayers;
        
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void LoadPlayers_ServerRPC()
        {
            _loadedPlayers++;
        }
        
        public override void OnNetworkSpawn()
        {
            LoadPlayers_ServerRPC();
        }


        private void Update()
        {
            if (_loadedPlayers == NetworkManager.Singleton.ConnectedClients.Count)
            {
                _time.Value += Time.deltaTime;
                if (_time.Value >= timeTillCutscene)
                {
                    PlayCutscene_ClientRpc();
                }
            }
        }
        


        [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server)]
        private void PlayCutscene_ClientRpc()
        {
            cutsceneManager.Instance.PlayCutscene(this);
        }
    }
}