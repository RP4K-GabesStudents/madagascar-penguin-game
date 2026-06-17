using System.Collections.Generic;
using InventorySystem.Core;
using UnityEngine;

namespace InventorySystem.UI
{
    /// <summary>
    /// Generates and manages a grid of ItemSlot views bound to an IInventory.
    /// Works against Inventory and NetworkInventory identically (it only speaks
    /// the contract) and against either generation mode:
    ///   - Edit time: click Generate in the inspector; slots are baked into the
    ///     prefab/scene as real children. At runtime these win: the grid uses
    ///     exactly the authored count and never grows to capacity.
    ///   - Runtime: if no slots were baked, Bind() reconciles the pool to the
    ///     inventory's capacity, instantiating only the shortfall.
    ///
    /// UI optimizations: per-slot refresh (never a full rebuild on a single
    /// change), and pooling (extra slots are deactivated and reused, not
    /// destroyed) so resizing or rebinding causes no instantiate/GC churn.
    /// Designed to be subclassed: a HotBar adds selection/scroll on top without
    /// touching generation or binding.
    /// </summary>
    public class GridInventory : MonoBehaviour
    {
        [SerializeField] private ItemSlot slotPrefab;
        [Tooltip("Where slots are parented. Defaults to this transform. Put a GridLayoutGroup here for layout.")]
        [SerializeField] private Transform slotParent;
        [Tooltip("How many slots Generate creates in the editor. Runtime uses the bound inventory's capacity only if no slots were baked.")]
        [SerializeField, Min(0)] private int editorPreviewCount = 20;

        // Serialized so editor-generated slots persist as references; managed by
        // the custom editor, hidden from the default inspector.
        [SerializeField, HideInInspector] private List<ItemSlot> slots = new();

        private IInventory _inventory;
        private int _active;

        // When slots were baked in the editor, they are authoritative: the grid
        // stays at the authored count and never reconciles to capacity.
        private bool _authored;

        public IReadOnlyList<ItemSlot> Slots => slots;
        public int SlotCount => slots.Count;
        public int ActiveCount => _active;
        public int EditorPreviewCount => editorPreviewCount;

        protected Transform SlotParent => slotParent ? slotParent : (slotParent = transform);

        protected virtual void Awake()
        {
            PruneNulls();
            _active = slots.Count; // pre-generated slots are all active
        }

        protected virtual void OnDestroy() => Unbind();

        // ---- binding ----

        public void Bind(IInventory inventory)
        {
            Unbind();
            _inventory = inventory;
            if (_inventory == null) return;

            // Pre-authored slots win. Only grow to capacity when nothing was baked.
            _authored = slots.Count > 0;
            if (_authored)
                _active = slots.Count;
            else
                EnsureSlotCount(_inventory.Capacity);

            _inventory.OnSlotChanged += RefreshSlot;
            RefreshAll();
        }

        public void Unbind()
        {
            if (_inventory != null) _inventory.OnSlotChanged -= RefreshSlot;
            _inventory = null;
        }

        protected virtual void RefreshAll()
        {
            for (int i = 0; i < _active; i++) RefreshSlot(i);
        }

        protected virtual void RefreshSlot(int index)
        {
            if (_inventory == null) return;

            // Capacity can change under us: NetworkInventory fills on spawn and
            // entries replicate in over several frames. Authored grids ignore
            // this and stay at the count baked in the editor.
            if (!_authored && _inventory.Capacity != _active) EnsureSlotCount(_inventory.Capacity);
            if ((uint)index >= (uint)_active) return;

            var cell = _inventory[index];
            slots[index].Render(cell.Definition, cell.Count);
        }

        // ---- pool management (shared by editor and runtime) ----

        /// <summary>
        /// Reconcile the pool to exactly count active slots: grow by
        /// instantiating the shortfall, shrink by deactivating the surplus.
        /// Nothing is destroyed, so repeated resizes are allocation-free.
        /// </summary>
        public void EnsureSlotCount(int count)
        {
            PruneNulls();
            while (slots.Count < count) slots.Add(InstantiateSlot());

            for (int i = 0; i < slots.Count; i++)
                slots[i].gameObject.SetActive(i < count);

            _active = count;
        }

        /// <summary>Exact rebuild for authoring: destroys all and creates count fresh.</summary>
        public void Generate(int count)
        {
            Clear();
            for (int i = 0; i < count; i++) slots.Add(InstantiateSlot());
            _active = count;
        }

        public void Clear()
        {
            for (int i = slots.Count - 1; i >= 0; i--)
                if (slots[i]) DestroyObject(slots[i].gameObject);
            slots.Clear();
            _active = 0;
        }

        private void PruneNulls()
        {
            for (int i = slots.Count - 1; i >= 0; i--)
                if (!slots[i]) slots.RemoveAt(i);
        }

        private ItemSlot InstantiateSlot()
        {
#if UNITY_EDITOR
            // In the editor, keep generated slots linked to the slot prefab
            // (nested prefab) so prefab edits propagate to every grid.
            if (!Application.isPlaying)
            {
                var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(slotPrefab.gameObject, SlotParent);
                return go.GetComponent<ItemSlot>();
            }
#endif
            return Instantiate(slotPrefab, SlotParent);
        }

        private static void DestroyObject(Object o)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) { DestroyImmediate(o); return; }
#endif
            Destroy(o);
        }
    }
}