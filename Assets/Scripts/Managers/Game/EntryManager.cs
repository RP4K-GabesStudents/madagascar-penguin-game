using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using Game.Characters;
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
        [SerializeField] private SceneReference selectionScene;
        [SerializeField] private GenericCharacter failSafePrefab;
        private Dictionary<ulong, ulong> _prefabIdHashes = new ();
        private bool _isSelectionSpawn;
        private void OnEnable()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += StartGame;
            NetworkManager.SceneManager.OnLoadComplete += OnLocalLoaded;
        }

        private async void OnLocalLoaded(ulong clientid, string scenename, LoadSceneMode loadscenemode)
        {
            if (_isSelectionSpawn) return;
            _isSelectionSpawn = true;
            Debug.Log("OnLocalLoaded");
            try
            {
                await SceneManager.LoadSceneAsync(selectionScene.BuildIndex, LoadSceneMode.Additive);
                Debug.Log("finished loading scene");
                SelectionManager.Instance.OnCharacterSelected += RequestSpawnCharacter;
            }
            catch (Exception e)
            {
                Debug.LogError("Faild while loading local clinet: " + e.Message);
            }
            //SpawnPenguin_ServerRpc();
        }

        private void RequestSpawnCharacter(GenericCharacter obj)
        {
            ChoosePenguin_ServerRpc(obj.NetworkObject.PrefabIdHash);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChoosePenguin_ServerRpc(ulong myPenguin, ServerRpcParams serverRpcParams = default)
        {
            if (!_prefabIdHashes.TryAdd(serverRpcParams.Receive.SenderClientId, myPenguin))
            {
                _prefabIdHashes[serverRpcParams.Receive.SenderClientId] = myPenguin;
            }
            if (NetworkManager.ConnectedClients.Count == _prefabIdHashes.Count)
            {
                SpawnPenguins();
                OnGameStarting_ClientRpc();
            }
        }
        
        private void SpawnPenguins()
        {
            int location = Random.Range(0, spawnPoints.Count);
            Vector3 spawn = spawnPoints[location].position;
            spawnPoints.RemoveAt(location);
            foreach (var kvp in _prefabIdHashes)
            {
                foreach (NetworkPrefab potato in NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists[0].PrefabList)
                {
                    if (potato.SourcePrefabGlobalObjectIdHash == kvp.Value)
                    {
                        GameObject go = Instantiate(potato.Prefab, spawn, Quaternion.identity);
                        go.GetComponent<NetworkObject>().SpawnAsPlayerObject(kvp.Key);
                        break;
                    }
                }
            }
        }

        private void StartGame(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
        {
            Debug.Log("startGame todo:timer and ui explosion");
        }

        [ClientRpc]
        private void OnGameStarting_ClientRpc()
        {
            Debug.Log("I know game start as client");
            SceneManager.UnloadSceneAsync(selectionScene.BuildIndex);
        }
    }
}
