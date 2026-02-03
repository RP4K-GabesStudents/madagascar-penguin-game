using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
#if UNITY_EDITOR && PARREL_SYNC
using ParrelSync;
#endif

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class NetcodeAutoHostSequence : MonoBehaviour, IEntrySequence
    {
        [Header("Host Settings")]
        [Tooltip("Next sequence to execute after successfully starting as HOST.")]
        [SerializeField] private Behaviour hostSuccess;
        
        [Tooltip("Next sequence to execute after successfully connecting as CLIENT.")]
        [SerializeField] private Behaviour clientSuccess;
        
        [Tooltip("Sequence to execute if connection fails.")]
        [SerializeField] private Behaviour failure;

        [Header("Connection Settings")]
        [Tooltip("Maximum time to wait for client connection (in seconds).")]
        [SerializeField] private float clientConnectionTimeout = 10f;

        public IEntrySequence Default => null; // Not used, we have separate paths
        
        public bool IsCompleted => NetworkManager.Singleton != null && 
                                    NetworkManager.Singleton.IsListening;

        public event Action<string> DisplayMessage;

        private bool IsMainEditor()
        {
#if UNITY_EDITOR && PARREL_SYNC
            return !ClonesManager.IsClone();
#elif UNITY_EDITOR
            // Check if this is a virtual player in Multiplayer Play Mode
            // CurrentPlayer.ReadOnly returns true for virtual players
            var arguments = Environment.GetCommandLineArgs();
            int nameIndex = Array.IndexOf(arguments, "-name");

            foreach (string  s in arguments)
            {
                Debug.Log(s);
            }
        
            if(nameIndex >= 0 && nameIndex + 1 < arguments.Length)
            {
                var playerName = arguments[nameIndex + 1];
                if(playerName != "Player1")
                {
                    return false;
                }
            }
#else
            return true; // In builds, default to host
#endif
            return true;
        }

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            try
            {
                
                
                // Ensure NetworkManager exists
                if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsConnectedClient)
                {
                    DisplayMessage?.Invoke("[NetcodeAutoHostSequence] NetworkManager.Singleton is null!");
                    Debug.LogError("[NetcodeAutoHostSequence] NetworkManager.Singleton is null. Make sure NetworkManager exists in the scene.");
                    return failure as IEntrySequence;
                }

                // Determine if this should be host or client
                bool shouldBeHost = IsMainEditor();

                if (shouldBeHost)
                {
                    return await StartAsHost();
                }
                else
                {
                    return await StartAsClient();
                }
            }
            catch (Exception e)
            {
                DisplayMessage?.Invoke($"[NetcodeAutoHostSequence] Error: {e.Message}");
                Debug.LogError($"[NetcodeAutoHostSequence] Exception occurred: {e}");
                return failure as IEntrySequence;
            }
        }

        private async UniTask<IEntrySequence> StartAsHost()
        {
            // Check if already hosting
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[NetcodeAutoHostSequence] Already hosting.");
                return hostSuccess as IEntrySequence;
            }

            // Start as host
            Debug.Log("[NetcodeAutoHostSequence] Starting as HOST...");
            DisplayMessage?.Invoke("Starting as host...");
            
            bool started = NetworkManager.Singleton.StartHost();

            if (!started)
            {
                DisplayMessage?.Invoke("[NetcodeAutoHostSequence] Failed to start host!");
                Debug.LogError("[NetcodeAutoHostSequence] Failed to start host.");
                return failure as IEntrySequence;
            }

            // Wait a frame to ensure connection is established
            await UniTask.Yield();

            // Verify we're actually connected as host
            if (!NetworkManager.Singleton.IsHost)
            {
                DisplayMessage?.Invoke("[NetcodeAutoHostSequence] Host started but not connected!");
                Debug.LogError("[NetcodeAutoHostSequence] StartHost returned true but IsHost is false.");
                return failure as IEntrySequence;
            }

            Debug.Log($"[NetcodeAutoHostSequence] Successfully started as HOST. Client ID: {NetworkManager.Singleton.LocalClientId}");
            DisplayMessage?.Invoke("Host started successfully!");
            
            return hostSuccess as IEntrySequence;
        }

        private async UniTask<IEntrySequence> StartAsClient()
        {
            // Check if already connected as client
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            {
                Debug.Log("[NetcodeAutoHostSequence] Already connected as client.");
                return clientSuccess as IEntrySequence;
            }

            // Start as client
            Debug.Log("[NetcodeAutoHostSequence] Starting as CLIENT...");
            DisplayMessage?.Invoke("Connecting as client...");
            
            bool started = NetworkManager.Singleton.StartClient();

            if (!started)
            {
                DisplayMessage?.Invoke("[NetcodeAutoHostSequence] Failed to start client!");
                Debug.LogError("[NetcodeAutoHostSequence] Failed to start client.");
                return failure as IEntrySequence;
            }

            // Wait for client to connect with timeout
            float elapsedTime = 0f;
            while (!NetworkManager.Singleton.IsConnectedClient && elapsedTime < clientConnectionTimeout)
            {
                await UniTask.Yield();
                elapsedTime += Time.deltaTime;
            }

            // Verify connection
            if (!NetworkManager.Singleton.IsConnectedClient)
            {
                DisplayMessage?.Invoke("[NetcodeAutoHostSequence] Client connection timeout!");
                Debug.LogError($"[NetcodeAutoHostSequence] Failed to connect as client within {clientConnectionTimeout} seconds.");
                NetworkManager.Singleton.Shutdown();
                return failure as IEntrySequence;
            }

            Debug.Log($"[NetcodeAutoHostSequence] Successfully connected as CLIENT. Client ID: {NetworkManager.Singleton.LocalClientId}");
            DisplayMessage?.Invoke("Client connected successfully!");
            
            return clientSuccess as IEntrySequence;
        }

        private void OnDrawGizmos()
        {
            if (hostSuccess && hostSuccess is not IEntrySequence)
            {
                Debug.LogError("Host Success is INVALID", gameObject);
            }
            if (clientSuccess && clientSuccess is not IEntrySequence)
            {
                Debug.LogError("Client Success is INVALID", gameObject);
            }
            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("Failure is INVALID", gameObject);
            }
        }
    }
}