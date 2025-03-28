using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class Itemslot : MonoBehaviour
    {
        [SerializeField] private ItemStats item;
        [SerializeField] private Image image;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI slotHotKey;
        
        public ItemStats GetItem()
        {
            return item;
        }
        
        public void SetItem(ItemStats item)
        {
            this.item = item;
            image.sprite = item.Icon;
        }
        
        public void RemoveItem()
        {
            image.sprite = null;
        }
        
        public void ChangeSlotKeyBing(string newHotKey)
        {
            slotHotKey.text = newHotKey;
        }
    }
}
