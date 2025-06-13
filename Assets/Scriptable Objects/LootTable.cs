using System;
using System.Collections;
using Managers;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "Scriptable Objects/LootTable")]
    public class LootTable : ScriptableObject
    {
        [SerializeField] private LootData[] lootData;

        public void Spawn(Vector3 position, float launchForce, float delay)
        {
            // WaitForSeconds wait = new WaitForSeconds(delay);
            // for (int i = 0; i < lootData.Length; i++)
            // {
            //     Rigidbody gameObject = lootData[i].TrySpawnObject(out int amount).GetComponent<Rigidbody>();
            //     if (amount == 0) continue;
            //     for (int j = 0; j < amount; j++)
            //     {
            //         Rigidbody rb = Instantiate(gameObject, position, Quaternion.identity);
            //         rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
            //         yield return wait;
            //     }
            // }
            LootManager.Instance.SpawnLoot_ServerRpc(position, launchForce, delay, lootData);
        }


        [Serializable]
        public struct LootData : INetworkSerializable
        {
            [SerializeField, Min(0)] private int minSpawnAmount;
            [SerializeField, Min(1)] private int maxSpawnAmount;
            [SerializeField] private NetworkPrefab spawnPrefab; // Prefab reference (editor-only)
            [SerializeField, Range(0, 1)] private float chanceToSpawn;

            // Runtime-only: Stores the prefab's hash ID
            private uint prefabHash;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref minSpawnAmount);
                serializer.SerializeValue(ref maxSpawnAmount);
                serializer.SerializeValue(ref chanceToSpawn);

                if (serializer.IsWriter)
                {
                    // When sending: Convert prefab to its network hash
                    prefabHash = spawnPrefab.SourcePrefabGlobalObjectIdHash;
                    serializer.SerializeValue(ref prefabHash);
                }
                else
                {
                    // When receiving: Look up prefab from hash
                    serializer.SerializeValue(ref prefabHash);
                    spawnPrefab = GetNetworkPrefabFromHash(prefabHash);
                }
            }

            public NetworkPrefab TrySpawnObject(out int amount)
            {
                float rng = Random.Range(0f, 1f);
                if (chanceToSpawn >= rng)
                {
                    amount = Random.Range(minSpawnAmount, maxSpawnAmount + 1);
                    return spawnPrefab;
                }

                amount = 0;
                return null;
            }

            private NetworkPrefab GetNetworkPrefabFromHash(uint hash)
            {
                // Fetch prefab from NetworkManager's prefab list
                foreach (NetworkPrefab prefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
                {
                    if (prefab.SourcePrefabGlobalObjectIdHash == hash)
                        return prefab;
                }

                throw new System.Exception($"Prefab with hash {hash} not found!");
            }
        }
    }
}