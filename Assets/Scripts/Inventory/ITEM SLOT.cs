
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities.Hover;

namespace Inventory
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
                itemCount.enabled = _invCount <= 1;
            }
        }
        private int _invCount;
        [SerializeField]private TextMeshPro itemCount;

        private UIHoverScale _hover;

        private void Awake()
        {
            _hover = GetComponent<UIHoverScale>();
            MarkSelected();
        }

        public ItemStats GetItem()
        {
            return item;
        }
        
        public void SetItem(ItemStats newItem)
        {
            if (newItem == item)
            {
                InvCount += 1;
            }
            else
            {
                item = newItem;
                image.sprite = newItem.Icon;
            }
            image.enabled = true;
        }
        
        public void RemoveItem()
        {
            image.sprite = null;
            image.enabled = false;
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

        public bool CanAcceptItem(ItemStats items)
        {
            return !item || (items == item && InvCount < item.ItemLimit);
        }
    }
}
