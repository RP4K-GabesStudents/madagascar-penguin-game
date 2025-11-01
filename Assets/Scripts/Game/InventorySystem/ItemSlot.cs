using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Hover;

namespace Game.InventorySystem
{
    public class Itemslot : MonoBehaviour
    {
        [SerializeField] private ItemStats item;
        [SerializeField] private Image image;
        [SerializeField] private Image frame;
        [SerializeField] private Image rarity;
        [SerializeField] private TextMeshProUGUI slotHotKey;

        public int InvCount
        {
            get => _invCount;
            set
            {
                _invCount = !item ? 0 : Mathf.Clamp(value, 0, item.ItemLimit);
                itemCount.text = _invCount.ToString();
                itemCount.enabled = _invCount > 1;
            }
        }
        private int _invCount;
        [SerializeField]private TextMeshProUGUI itemCount;

        private UIHoverScale _hover;

        private void Awake()
        {
            _hover = GetComponent<UIHoverScale>();
        }

        public ItemStats GetItem()
        {
            return item;
        }
        
        public void SetItem(ItemStats newItem, int newInvCount)
        {
            if (item != newItem)
            {
                item = newItem;
                image.sprite = newItem?.Icon;
                image.enabled = newItem;
            }
            if (InvCount != newInvCount)
            {
                InvCount = newInvCount;
            }
        }
        
        public void RemoveItem()
        {
            image.sprite = null;
            image.enabled = false;
            InvCount = 0;
            item = null;
            MarkUnselected(); // temporary.
        }
        
        public void ChangeSlotKeyBing(string newHotKey)
        {
            slotHotKey.text = newHotKey;
        }

        public void MarkSelected()
        {
            _hover.Grow();
            frame.color = Color.white;
        }

        public void MarkUnselected()
        {
            _hover.Shrink();
            frame.color = Color.gray;
        }
    }
}
