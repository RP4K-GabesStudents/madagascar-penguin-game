using InventorySystem.Core;
using TMPro;
using UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
    /// <summary>
    /// Pure view for one slot: renders a definition + count, and reflects
    /// selection by playing an optional UIEffectPlayer. The slot has no idea
    /// what the selection looks like, or whether any effect exists at all; it
    /// just calls Play/Stop if a player is present. What selection means
    /// visually is authored on the prefab, not decided here.
    /// </summary>
    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image rarity;
        [SerializeField] private TextMeshProUGUI itemCount;
        [SerializeField] private TextMeshProUGUI slotHotKey;

        [Tooltip("Optional. Played when this slot is selected, stopped when deselected. " +
                 "Leave unset for a slot with no selection visual.")]
        [SerializeField] private UIEffectPlayer selectionEffect;

        private void Awake()
        {
            if (!selectionEffect) selectionEffect = GetComponent<UIEffectPlayer>();
        }

        public void Render(ItemStats def, int count)
        {
            bool has = def != null && count > 0;

            icon.sprite = has ? def.Icon : null;
            icon.enabled = has;
            if (rarity) rarity.enabled = has; // map EItemRarity -> color here when ready

            int shown = has ? Mathf.Clamp(count, 0, def.StackSize) : 0;
            itemCount.text = shown.ToString();
            itemCount.enabled = shown > 1;
        }

        public void SetHotKey(string label)
        {
            if (slotHotKey) slotHotKey.text = label ?? "";
        }

        public void SetSelected(bool selected, bool instant = false)
        {
            if (!selectionEffect) return; // no effect authored: selection is silent

            if (instant) selectionEffect.SetAllInstant(selected);
            else if (selected) selectionEffect.Play();
            else selectionEffect.Stop();
        }
    }
}