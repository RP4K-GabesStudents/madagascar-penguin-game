using Scriptable_Objects;
using UnityEngine;

namespace Objects
{
    public class LootBox : MonoBehaviour
    {
        [SerializeField] private LootTable lootTable;
        
        public void DropLoot()
        {
            Debug.Log("I dropped my loot");
            StartCoroutine(lootTable.Spawn(transform.position, 3, 0.1f));
        }
        
    }
}
