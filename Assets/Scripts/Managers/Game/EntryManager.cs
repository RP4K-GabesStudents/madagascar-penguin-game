using System;
using System.Collections.Generic;
using Eflatun.SceneReference;
using Game.Characters;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using NetworkPrefab = Unity.Netcode.NetworkPrefab;
using Random = UnityEngine.Random;

namespace Managers.Game
{
    public class EntryManager : NetworkBehaviour
    {
        private static readonly int OnExploded = Animator.StringToHash("OnExploded");
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private SceneReference selectionScene;
        [SerializeField] private GenericCharacter failSafePrefab;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI timerText;
        private NetworkVariable<float> _selectionTime = new ();
        private Dictionary<ulong, ulong> _playerPrefabs = new ();
        private bool _isSelectionSpawn;

        [SerializeField] private GameObject[] enableLater;
        private void OnEnable()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += StartGame;
            NetworkManager.SceneManager.OnLoadComplete += OnLocalLoaded;
        }
        private void OnDisable()
        {
            NetworkManager.SceneManager.OnLoadEventCompleted += StartGame;
            NetworkManager.SceneManager.OnLoadComplete += OnLocalLoaded;
        }

        private void Awake()
        {
            
            foreach (GameObject go in enableLater)
            {
                go.SetActive(false);
            }
        }
        void Update()
        {
            if (!IsServer) return;
            _selectionTime.Value -= Time.deltaTime;
            if (_selectionTime.Value < 0)
            {
                ForceChoosePenguin_ClientRpc();
            }
        }

        [ClientRpc]
        private void ForceChoosePenguin_ClientRpc()
        {
            SelectionManager.Instance.SelectCurPenguin();
            gameObject.SetActive(false);
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
            //Doing just .NetworkObject doesn't work until it's spawned in.
            ChoosePenguin_ServerRpc(obj.GetComponent<NetworkObject>().PrefabIdHash);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChoosePenguin_ServerRpc(ulong myPenguin, ServerRpcParams serverRpcParams = default)
        {
            if (!_playerPrefabs.TryAdd(serverRpcParams.Receive.SenderClientId, myPenguin))
            {
                _playerPrefabs[serverRpcParams.Receive.SenderClientId] = myPenguin;
            }
            if (NetworkManager.ConnectedClients.Count == _playerPrefabs.Count)
            {
                SpawnPenguins();
                OnGameStarting_ClientRpc();
            }
        }
        
        private void SpawnPenguins()
        {
            foreach (var kvp in _playerPrefabs)
            {
                int location = Random.Range(0, spawnPoints.Count);
                Vector3 spawn = spawnPoints[location].position;
                spawnPoints.RemoveAt(location);
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
            _selectionTime.OnValueChanged += (oldvalue, newvalue) => { timerText.SetText(newvalue.ToString("N0"));};
            animator.SetTrigger(OnExploded);
            
        }

        [ClientRpc]
        private void OnGameStarting_ClientRpc()
        {
            Debug.Log("I know game start as client");
            SceneManager.UnloadSceneAsync(selectionScene.BuildIndex);
            foreach (GameObject go in enableLater)
            {
                go.SetActive(true);
            }
        }
    }
}
