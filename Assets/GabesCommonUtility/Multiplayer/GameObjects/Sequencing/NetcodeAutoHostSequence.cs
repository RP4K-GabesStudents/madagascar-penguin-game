using System;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Unity.Netcode;
using UnityEngine;

namespace GabesCommonUtility.Multiplayer.GameObjects.Sequencing
{
    public class NetcodeAutoHostSequence : MonoBehaviour, IEntrySequence
    {
        [Header("Host Settings")]
        [Tooltip("Optional: Next sequence to execute after hosting starts.")]
        [SerializeField] private Behaviour next;
        
        [Tooltip("Optional: Sequence to execute if hosting fails.")]
        [SerializeField] private Behaviour failure;

        public IEntrySequence Default => next as IEntrySequence;
        
        public bool IsCompleted => NetworkManager.Singleton != null && 
                                    NetworkManager.Singleton.IsListening && 
                                    NetworkManager.Singleton.IsHost;

        public event Action<string> DisplayMessage;

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            try
            {
                // Ensure NetworkManager exists
                if (NetworkManager.Singleton == null)
                {
                    DisplayMessage?.Invoke("[NetcodeHostSequence] NetworkManager.Singleton is null!");
                    Debug.LogError("[NetcodeHostSequence] NetworkManager.Singleton is null. Make sure NetworkManager exists in the scene.");
                    return failure as IEntrySequence;
                }

                // Check if already hosting
                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("[NetcodeHostSequence] Already hosting.");
                    return Default;
                }

                // Start as host
                Debug.Log("[NetcodeHostSequence] Starting as host...");
                DisplayMessage?.Invoke("Starting local host...");
                
                bool started = NetworkManager.Singleton.StartHost();

                if (!started)
                {
                    DisplayMessage?.Invoke("[NetcodeHostSequence] Failed to start host!");
                    Debug.LogError("[NetcodeHostSequence] Failed to start host.");
                    return failure as IEntrySequence;
                }

                // Wait a frame to ensure connection is established
                await UniTask.Yield();

                // Verify we're actually connected as host
                if (!NetworkManager.Singleton.IsHost)
                {
                    DisplayMessage?.Invoke("[NetcodeHostSequence] Host started but not connected!");
                    Debug.LogError("[NetcodeHostSequence] StartHost returned true but IsHost is false.");
                    return failure as IEntrySequence;
                }

                Debug.Log($"[NetcodeHostSequence] Successfully started as host. Client ID: {NetworkManager.Singleton.LocalClientId}");
                DisplayMessage?.Invoke("Host started successfully!");
                
                return Default;
            }
            catch (Exception e)
            {
                DisplayMessage?.Invoke($"[NetcodeHostSequence] Error: {e.Message}");
                Debug.LogError($"[NetcodeHostSequence] Exception occurred: {e}");
                return failure as IEntrySequence;
            }
        }

        private void OnDrawGizmos()
        {
            if (next && Default == null)
            {
                Debug.LogError("Success is INVALID", gameObject);
            }
            if (failure && failure is not IEntrySequence)
            {
                Debug.LogError("Failure is INVALID", gameObject);
            }
        }
    }
}