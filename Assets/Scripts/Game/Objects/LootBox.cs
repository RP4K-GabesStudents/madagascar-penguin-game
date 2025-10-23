using Game.Objects;
using Scriptable_Objects;
using UnityEngine;

namespace Objects
{
    public class LootBox : MonoBehaviour, IDamageable
    {
        [SerializeField] private LootTable lootTable;
        
        public void DropLoot()
        {
            Debug.Log("I dropped my loot");
            lootTable.Spawn(transform.position, 3, 0.1f);
        }

        public void Die(Vector3 force)
        {
            DropLoot();
        }

        public void OnHurt(float amount, Vector3 force) { }

        [field: SerializeField] public float Health { get; set; }
        public float DamageRes => 0;
    }
}
