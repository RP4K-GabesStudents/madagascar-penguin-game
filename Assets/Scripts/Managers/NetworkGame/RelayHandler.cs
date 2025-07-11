using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class RelayHandler : MonoBehaviour
    {
        //private float x = 1.1f;
        private const string ConnectionType = "udp";
        public static RelayHandler Instance { get; private set; }

        public static Dictionary<ulong, byte[]> PlayerData = new ();

        private void Start()
        {
            if(Instance) Destroy(gameObject);
            Instance = this;
            NetworkManager.Singleton.OnServerStarted += () => Debug.LogWarning("Server Started");
            NetworkManager.Singleton.OnClientStarted += () => Debug.LogWarning("Client Started");
            NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
            {
                Debug.LogWarning($"Client {clientId} disconnected.");
            };
            NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            {
            
                print("I connected as: " + id);
                if (!NetworkManager.Singleton.IsHost) return;
                foreach (var variable in  NetworkManager.Singleton.ConnectedClientsIds)
                {
                    print("Clients connected: " + variable);                
                }
            };

            NetworkManager.Singleton.ConnectionApprovalCallback += (request, response) =>
            {
                PlayerData.Add(request.ClientNetworkId, request.Payload);
            };
        }


        public async Task<string> CreateRelay(int maxPlayers)
        {
            PlayerData .Clear();
            
            try
            {

                var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
                
                Debug.Log("Creating Relay: " + joinCode);

                var relayServerData = hostAllocation.ToRelayServerData(ConnectionType);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();

                NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
                NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);

                Debug.Log("Connected to Relay as host: " + NetworkManager.Singleton.IsHost);
                NetworkManager.Singleton.SceneManager.OnLoad += (clientId, sceneName, loadSceneMode, asyncOperation) =>
                {
                    Debug.Log($"Client {clientId} is loading scene {sceneName} with mode {loadSceneMode}");
                };
                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Failed while trying to create relay" + e);
            }
            return null;
        }
    

        public async Task JoinRelay(string joinCode)
        {
            try
            {
                Debug.Log("Joining with code: " + joinCode);

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var relayServerData = joinAllocation.ToRelayServerData(ConnectionType);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();
                NetworkManager.Singleton.SceneManager.OnLoad += (clientId, sceneName, loadSceneMode, asyncOperation) =>
                {
                    Debug.Log($"Client {clientId} is loading scene {sceneName} with mode {loadSceneMode}");
                };
                Debug.Log("Connected to Relay as client: " + NetworkManager.Singleton.IsClient);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Failed while trying to join relay: " + e);
            }
        }

        public void SetLocalServerInfo(byte[] data)
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionData = data;
        }
    }
}
