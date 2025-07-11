using System.Collections;
using Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

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
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnLoot_ServerRpc(LootTable lootData, Vector3 position, Quaternion rotation, float launchForce = 0, float torque = 0, float delay = 0, int rolls = 1)
        {
            StartCoroutine(Spawn(lootData,position,rotation, launchForce, torque,delay,rolls));
        }
        private IEnumerator Spawn(LootTable lootData, Vector3 position, Quaternion rotation, float launchForce = 0, float torque = 0, float delay = 0, int rolls = 1)
        {
            WaitForSeconds wait = new WaitForSeconds(delay);
            for (int i = 0; i < rolls; i++)
            {
                Debug.Log("Rolling a loot drop");
                GameObject prefab = lootData.RetrieveRandomNetworkPrefab(out int amount)?.Prefab;
                
                if (amount == 0 || !prefab) continue;
                
                Debug.Log($"Spawned {amount}x {prefab.name} at {position}");
                
                
                for (int j = 0; j < amount; j++)
                {
                    Debug.Log("Attempt to spawn: " + prefab!.name);
                    GameObject inst = Instantiate(prefab, position, rotation);
                    Rigidbody rb = inst.GetComponent<Rigidbody>();
                    inst.GetComponent<NetworkObject>().Spawn();
                    rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.Impulse);
                    yield return wait;
                }
            }
        }
    }
}
