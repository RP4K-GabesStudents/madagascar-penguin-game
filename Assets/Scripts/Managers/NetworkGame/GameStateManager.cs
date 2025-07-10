using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : NetworkBehaviour
{
    private void Awake()
    {
        if (!IsServer)
        {
            gameObject.SetActive(false);
            return;
        }
        
        NetworkManager.SceneManager.OnLoadEventCompleted += SpawnPenguins;
    }

    private void SpawnPenguins(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SpawnPenguinsWithOwner_ServerRpc(clientsCompleted.ToArray());
    }
    

    [ServerRpc]
    private void SpawnPenguinsWithOwner_ServerRpc(ulong[] clientsCompleted)
    {
        Debug.LogError("Gabe come back");

        foreach (ulong id in clientsCompleted)
        {
            //NetworkManager.ConnectedClients[id].
        }
        
    }

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        base.OnOwnershipChanged(previous, current);
        gameObject.SetActive(IsServer);
    }
}
