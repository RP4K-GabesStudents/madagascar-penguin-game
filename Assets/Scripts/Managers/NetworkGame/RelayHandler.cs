using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GabesCommonUtility.Multiplayer.GameObjects
{
    public class RelayHandler : IDisposable
    {
        // "dtls" is Unity Relay's reliable protocol for NGO. Raw "udp" joins the
        // allocation but frequently never bridges host<->client (silent timeout,
        // empty DisconnectReason). For WebGL builds use "wss" instead.
        private const string ConnectionType = "dtls";
        private const string Tag = "[Relay]";

        private static RelayHandler _instance;
        public static RelayHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RelayHandler();
                }
                return _instance;
            }
        }

        private bool _isInitialized;

        private RelayHandler()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError($"{Tag} NetworkManager.Singleton is null. Cannot initialize RelayHandler.");
                return;
            }

            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            _isInitialized = true;
        }

        private void OnServerStarted() => Debug.LogWarning($"{Tag} Server started.");
        private void OnClientStarted() => Debug.LogWarning($"{Tag} Client started.");

        private void OnClientDisconnect(ulong clientId)
        {
            Debug.LogWarning($"{Tag} Client {clientId} disconnected. " +
                             $"Reason: '{NetworkManager.Singleton.DisconnectReason}'");
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log($"{Tag} Connected as client id: {id}");
            if (!NetworkManager.Singleton.IsHost) return;

            foreach (var variable in NetworkManager.Singleton.ConnectedClientsIds)
                Debug.Log($"{Tag} Connected client: {variable}");
        }

        public bool IsConnected() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;

        public async UniTask<string> CreateRelay(int maxPlayers, string region = null)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError($"{Tag} NetworkManager.Singleton is null; cannot create relay.");
                return null;
            }
            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning($"{Tag} Already listening; ignoring duplicate CreateRelay call.");
                return null;
            }

            try
            {
                var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers, region);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

                Debug.Log($"{Tag} Creating relay (maxConnections={maxPlayers}). Join code: {joinCode}");

                var relayServerData = hostAllocation.ToRelayServerData(ConnectionType);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();

                NetworkManager.Singleton.SceneManager.ActiveSceneSynchronizationEnabled = true;
                NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);

                Debug.Log($"{Tag} StartHost done. IsHost={NetworkManager.Singleton.IsHost}, " +
                          $"IsListening={NetworkManager.Singleton.IsListening}");
                NetworkManager.Singleton.SceneManager.OnLoad += OnSceneLoad;

                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"{Tag} Failed to create relay: {e}");
            }
            return null;
        }

        public async UniTask<bool> JoinRelay(string joinCode)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError($"{Tag} NetworkManager.Singleton is null; cannot join relay.");
                return false;
            }
            // Defensive: prevents a stray second call (e.g. an old UI hook) from
            // calling StartClient() over an in-flight connection and detaching it.
            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning($"{Tag} Already listening; ignoring duplicate JoinRelay call (code '{joinCode}').");
                return false;
            }

            try
            {
                Debug.Log($"{Tag} Joining with code: {joinCode}");

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var relayServerData = joinAllocation.ToRelayServerData(ConnectionType);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();
                NetworkManager.Singleton.SceneManager.OnLoad += OnSceneLoad;

                Debug.Log($"{Tag} StartClient called. Waiting for the connection to be approved...");

                // StartClient() returns immediately - it does NOT mean we connected.
                // Wait for the actual NGO handshake so we can tell join failures apart
                // from scene-transfer failures.
                float timeout = 10f;
                while (timeout > 0f
                       && NetworkManager.Singleton.IsClient
                       && !NetworkManager.Singleton.IsConnectedClient)
                {
                    await UniTask.Delay(100);
                    timeout -= 0.1f;
                }

                if (!NetworkManager.Singleton.IsConnectedClient)
                {
                    Debug.LogError($"{Tag} Client started but never connected. " +
                                   $"DisconnectReason: '{NetworkManager.Singleton.DisconnectReason}'. " +
                                   "Likely causes: Connection Approval enabled with no handler on the " +
                                   "NetworkManager, wrong/expired relay allocation, or a version mismatch.");
                    return false;
                }

                Debug.Log($"{Tag} Client fully connected. IsConnectedClient={NetworkManager.Singleton.IsConnectedClient}");
                return true;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"{Tag} Failed to join relay: {e}");
            }

            return false;
        }

        private void OnSceneLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            Debug.Log($"{Tag} Client {clientId} loading scene {sceneName} mode {loadSceneMode}");
        }

        public void Dispose()
        {
            if (!_isInitialized) return;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

                if (NetworkManager.Singleton.SceneManager != null)
                    NetworkManager.Singleton.SceneManager.OnLoad -= OnSceneLoad;
            }

            _isInitialized = false;
            _instance = null;
        }
    }
}