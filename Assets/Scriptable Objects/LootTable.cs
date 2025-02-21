using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "Scriptable Objects/LootTable")]
    public class LootTable : ScriptableObject
    {
        [SerializeField] private LootData[] lootData;
        public IEnumerator Spawn(Vector3 position, float launchForce, float delay)
        {
            WaitForSeconds wait = new WaitForSeconds(delay);
            for (int i = 0; i < lootData.Length; i++)
            {
                Rigidbody gameObject = lootData[i].TrySpawnObject(out int amount);
                if (amount == 0) continue;
                for (int j = 0; j < amount; j++)
                {
                    Rigidbody rb = Instantiate(gameObject, position, Quaternion.identity);
                    rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
                    yield return wait;
                }
            }
        }
        [Serializable] public struct LootData
        {
            [SerializeField, Min(0)]private int minSpawnAmount;
            [SerializeField, Min(1)]private int maxSpawnAmount;
            [SerializeField] private Rigidbody spawnPrefab;
            [SerializeField, Range(0,1)]private float chanceToSpawn;

            public Rigidbody TrySpawnObject(out int amount)
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
        }
    }
}
