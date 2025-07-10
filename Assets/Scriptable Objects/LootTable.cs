using System;
using Managers;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "Scriptable Objects/LootTable")]
    public class LootTable : ScriptableObject, INetworkSerializable
    {
        [SerializeField] private LootData[] lootData;
        private int _summativeSpawnWeight = -1;


        public NetworkPrefab RetrieveRandomNetworkPrefab(out int amount)
        {
            if(_summativeSpawnWeight == -1) ComputeSpawnWeight();
            float rng = Random.Range(0, _summativeSpawnWeight + 1);
            
            Debug.Log("Getting random loot with RNG: " + rng);
            
            foreach (LootData ld in lootData)
            {
                rng -= ld.SpawnWeight;
                if (rng <= 0)
                {
                    
                    return ld.GetSpawnInfo(out amount);
                }
            }
            Debug.LogWarning("We failed to spawn anything...");
            amount = 0;
            return null;
        }

        private void ComputeSpawnWeight()
        {
            _summativeSpawnWeight = 0;
            foreach (var d in lootData)
            {
                _summativeSpawnWeight += d.SpawnWeight;
            }
        }

        public void Spawn(Vector3 position, Quaternion rotation, float launchForce = 0, float torque = 0, float delay = 0, int rolls = 1)
        {
            LootManager.Instance.SpawnLoot_ServerRpc(this, position, rotation, launchForce, torque, delay,rolls);
        }
        public void Spawn(Vector3 position, float launchForce = 0, float torque = 0, float delay = 0, int rolls = 1)
        {
            LootManager.Instance.SpawnLoot_ServerRpc(this, position, Quaternion.identity, launchForce, torque, delay,rolls);
        }
        public void Spawn(Vector3 position, Quaternion rotation, int rolls = 1)
        {
            LootManager.Instance.SpawnLoot_ServerRpc(this, position, rotation,rolls);
        }


        [Serializable]
        public struct LootData : INetworkSerializable
        {
            [SerializeField, Min(0)] private int minSpawnAmount;
            [SerializeField, Min(1)] private int maxSpawnAmount;
            [SerializeField] private NetworkPrefab spawnPrefab; // Prefab reference (editor-only)
            [SerializeField] private int spawnWeight;
            
            public int SpawnWeight => spawnWeight;

            // Runtime-only: Stores the prefab's hash ID
            private uint _prefabHash;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref minSpawnAmount);
                serializer.SerializeValue(ref maxSpawnAmount);
                serializer.SerializeValue(ref spawnWeight);

                if (serializer.IsWriter)
                {
                    // When sending: Convert prefab to its network hash
                    _prefabHash = spawnPrefab.SourcePrefabGlobalObjectIdHash;
                    serializer.SerializeValue(ref _prefabHash);
                }
                else
                {
                    // When receiving: Look up prefab from hash
                    serializer.SerializeValue(ref _prefabHash);
                    spawnPrefab = GetNetworkPrefabFromHash(_prefabHash);
                }
            }

            private NetworkPrefab GetNetworkPrefabFromHash(uint hash)
            {
                // Fetch prefab from NetworkManager's prefab list
                foreach (NetworkPrefab prefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
                {
                    if (prefab.SourcePrefabGlobalObjectIdHash == hash)
                        return prefab;
                }
                throw new Exception($"Prefab with hash {hash} not found!");
            }

            public NetworkPrefab GetSpawnInfo(out int amount)
            {
                amount = Random.Range(minSpawnAmount, maxSpawnAmount);
                Debug.Log("Retrieved spawn info for: " + spawnPrefab.Prefab.name + ", " + amount);
                return spawnPrefab;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref lootData);
        }
    }
}