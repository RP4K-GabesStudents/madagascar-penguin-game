using penguin;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Network
{
    public class NetworkHost : MonoBehaviour
    {
        private NetworkManager _networkManager;

        void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!_networkManager.IsClient && !_networkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (GUILayout.Button("HostButton")) _networkManager.StartHost();
            if (GUILayout.Button("JoinButton")) _networkManager.StartClient();
            if (GUILayout.Button("ServerButton")) _networkManager.StartServer();
        }

        void StatusLabels()
        {
            var mode = _networkManager.IsHost ? "Host" : _networkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " + _networkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        void SubmitNewPosition()
        {
            if (GUILayout.Button(_networkManager.IsServer ? "Move" : "Request Position Change"))
            {
                if (_networkManager.IsServer && !_networkManager.IsClient)
                {
                    //foreach (ulong uid in _networkManager.ConnectedClientsIds)
                        //_networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<>().Move();
                }
                else
                {
                    var playerObject = _networkManager.SpawnManager.GetLocalPlayerObject();
                    //var player = playerObject.GetComponent<>();
                    //player.Move();
                }
            }
        }
    }
}