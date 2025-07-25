using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Managers.Game
{
    public class EntryManager : NetworkBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
        private void OnEnable()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += StartGame;
            NetworkManager.SceneManager.OnLoadComplete += OnLocalLoaded;
        }

        private void OnLocalLoaded(ulong clientid, string scenename, LoadSceneMode loadscenemode)
        {
            Debug.Log("OnLocalLoaded");
            SpawnPenguin_ServerRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SpawnPenguin_ServerRpc(ServerRpcParams serverRpcParams = default)
        {
            int location = Random.Range(0, spawnPoints.Count);
            Vector3 spawn = spawnPoints[location].position;
            spawnPoints.RemoveAt(location);
            foreach (NetworkPrefab potato in NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList)
            {
                if (potato.SourcePrefabGlobalObjectIdHash == GameData.Games[serverRpcParams.Receive.SenderClientId].PrefabID)
                {
                    GameObject go = Instantiate(potato.Prefab, spawn, Quaternion.identity);
                    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(serverRpcParams.Receive.SenderClientId);   
                    break;
                }
            }
        }

        private void StartGame(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
        {
            Debug.Log("startGame");
        }

        private void OnDisable()
        {
            
        }
    }
}
