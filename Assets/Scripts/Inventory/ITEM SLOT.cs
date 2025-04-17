using System;
using TMPro;
using UnityEngine;
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
            item = newItem;
            image.sprite = newItem.Icon;
        }
        
        public void RemoveItem()
        {
            image.sprite = null;
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
