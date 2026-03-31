using UnityEngine;
using Cutscene.Core;
using Cutscene.Managers;
using Unity.Netcode;

namespace Cutscene.Cutscenes
{
    public class TaskedBasedCutscene : MonoBehaviour, ICutscenes
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
        
        public void OnNetworkSpawn()
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
