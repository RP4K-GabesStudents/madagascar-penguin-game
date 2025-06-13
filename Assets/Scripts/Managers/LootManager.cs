using System;
using System.Collections;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class LootManager : NetworkBehaviour
    {
        public static LootManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);   
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnLoot_ServerRpc(Vector3 position, float launchForce, float delay, LootTable.LootData[] lootData)
        {
            StartCoroutine(Spawn(position, launchForce, delay, lootData));
        }
        public IEnumerator Spawn(Vector3 position, float launchForce, float delay, LootTable.LootData[] lootData)
        {
            WaitForSeconds wait = new WaitForSeconds(delay);
            for (int i = 0; i < lootData.Length; i++)
            {
                NetworkPrefab gameObject = lootData[i].TrySpawnObject(out int amount);
                if (amount == 0) continue;
                for (int j = 0; j < amount; j++)
                {
                    Rigidbody rb = Instantiate(gameObject.Prefab, position, Quaternion.identity).GetComponent<Rigidbody>();
                    rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
                    yield return wait;
                }
            }
        }
    }
}
