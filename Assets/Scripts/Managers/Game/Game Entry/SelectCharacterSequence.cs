using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GabesCommonUtility.Sequence;
using Game.Characters.World;
using Managers;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class SelectCharacterSequence : NetworkBehaviour, IEntrySequence
{
    public event Action<string> DisplayMessage;
    
    private NetworkVariable<bool> _isSelectionDone = new ();
    public NetworkVariable<float> _selectionTime = new (30);

    private Dictionary<ulong, ulong> _playerPrefabs = new ();
    
    [SerializeField] private List<Transform> spawnPoints;


    public async UniTask<IEntrySequence> ExecuteSequence()
    {
        Debug.Log("[SelectCharacterSequence] Executing Sequence");

        
        if (!SelectionManager.Instance)
        {
            Debug.LogError("Selection Manager is NULL");
            return failure as IEntrySequence;
        }

        SelectionManager.Instance.OnCharacterSelected += LocalSelectCharacter;
        
        Debug.Log("[SelectCharacterSequence] Await for a message to be received from the selection manager....");
        while (!IsDoneSelecting())
        {
            if(NetworkManager.IsServer) _selectionTime.Value -= Time.deltaTime;
            await UniTask.Yield();
        }

        if (IsServer) SpawnPenguins_ServerRpc();

        await UniTask.WaitUntil(() => _isSelectionDone.Value);

        return Default;
    }

    private bool IsDoneSelecting()
    {
        //If all the clients selected OR we are selection time... Then we are done selecting
        return NetworkManager.ConnectedClients.Count == _playerPrefabs.Count || _selectionTime.Value < 0;
    }

    //When the local player, selects a character
    private void LocalSelectCharacter(GenericCharacter obj)
    {
        //FIRST: perform any checks, are we able to select a character locally? Don't waste server messages.
        Debug.Log("[SelectCharacterSequence] LocalPlayer Selected Character");
        
        //Figure out what character they selected, and send a message to the server
        SelectCharacter_ServerRpc(obj.GetComponent<NetworkObject>().PrefabIdHash);
    }

    [Rpc(SendTo.Server)] //<< Only the OWNER of this object can say we've selected...
    private void SelectCharacter_ServerRpc(ulong characterID, RpcParams serverRpcParams = default)
    {
        if (!_playerPrefabs.TryAdd(serverRpcParams.Receive.SenderClientId, characterID))
        {
            _playerPrefabs[serverRpcParams.Receive.SenderClientId] = characterID;
        }
    }
    
    [Rpc(SendTo.Server)]
    private void SpawnPenguins_ServerRpc()
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
        
        Debug.Log("[SelectCharacterSequence] [ServerRpc] Spawned all the penguins");
        _isSelectionDone.Value = true;
    }
    
    //1. Await for a message to be received from the selection manager... Do we create the selection manager ourselves to ensure lifetime?
    //2. When we receive a message, send a server RPC
    //2.1 Propagate a client RPC to allow everyone to block the character (no duplicates)
    //2.2 Prepare to spawn the player UNISON {wait for everyone}
    //3. Once everyone is ready (Or unselected, and choose from pool) spawn the players and assign ownership

    [SerializeField] private Behaviour next;
    [SerializeField] private Behaviour failure;
    public IEntrySequence Default => next as IEntrySequence;
    public bool IsCompleted => false;
    
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
