using UnityEngine;
using UnityEngine.InputSystem;

namespace InventorySystem.UI
{
    /// <summary>
    /// A GridInventory that adds single-slot selection and scroll-cycling.
    /// Generation, pooling, binding, and per-slot refresh all come from the
    /// base; this only owns "which slot is selected" and keeps that visual in
    /// sync. Selection is local client state, so it needs no networking even
    /// when bound to a NetworkInventory.
    ///
    /// Input (new Input System):
    ///   - Scroll: a Value action (read as a float). Sign of the value steps
    ///     selection; Invert Scroll flips the direction.
    ///   - Select: a Button action carrying up to 10 bindings, one per slot in
    ///     order (e.g. keys 1,2,...,9,0). The binding that fires picks the slot.
    ///     If Auto Number Keys is on, each bound slot's hotkey label is filled
    ///     from that binding's display string; slots past the binding count get
    ///     "". Bindings are rebindable at runtime via RebindSelectKey().
    /// </summary>
    public class HotBar : GridInventory
    {
        [Header("Input")]
        [Tooltip("Value action read as a float. Its sign steps the selection by one slot.")]
        [SerializeField] private InputActionReference scrollAction;
        [Tooltip("Button action with up to 10 bindings, one per slot in order (e.g. 1..9,0).")]
        [SerializeField] private InputActionReference selectAction;

        [Header("Options")]
        [Tooltip("Flip scroll direction.")]
        [SerializeField] private bool invertScroll;
        [Tooltip("Fill each slot's hotkey label from the matching Select binding (first 10 slots). Slots without a binding show \"\".")]
        [SerializeField] private bool autoNumberKeys = true;

        private int _selected;
        private bool _hasSelection;
        public int SelectedIndex => _selected;

        /// <summary>Raised when the selected index actually changes (not on re-select of the same slot).</summary>
        public event System.Action<int> SelectionChanged;

        public bool InvertScroll
        {
            get => invertScroll;
            set => invertScroll = value;
        }

        // ---- lifecycle ----

        protected override void RefreshAll()
        {
            base.RefreshAll();
            // Slots are active and the inventory is bound by now, so binding
            // display strings resolve correctly. This also covers rebinds.
            RefreshHotKeyLabels();
        }

        private void OnEnable()
        {
            if (scrollAction != null && scrollAction.action != null)
            {
                scrollAction.action.performed += OnScroll;
                scrollAction.action.Enable();
            }

            if (selectAction != null && selectAction.action != null)
            {
                selectAction.action.performed += OnSelect;
                selectAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (scrollAction != null && scrollAction.action != null)
                scrollAction.action.performed -= OnScroll;

            if (selectAction != null && selectAction.action != null)
                selectAction.action.performed -= OnSelect;
        }

        // ---- input callbacks ----

        private void OnScroll(InputAction.CallbackContext ctx)
        {
            float v = ctx.ReadValue<float>();
            if (Mathf.Approximately(v, 0f)) return;

            int step = v > 0f ? 1 : -1;
            if (invertScroll) step = -step;
            Scroll(step);
        }

        private void OnSelect(InputAction.CallbackContext ctx)
        {
            // Map the control that fired back to a slot index. -1 means no
            // binding matched (e.g. a stray binding with no slot), in which
            // case we do nothing rather than mis-selecting slot 0.
            int slot = SlotForControl(ctx.action, ctx.control);
            if (slot >= 0) Select(slot);
        }

        /// <summary>
        /// Slot index whose (non-composite) binding matches the given control,
        /// or -1 if none. Slot order follows binding order, composites skipped.
        /// </summary>
        private static int SlotForControl(InputAction action, InputControl control)
        {
            if (action == null || control == null) return -1;

            int slot = 0;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isComposite || action.bindings[i].isPartOfComposite)
                    continue;

                if (InputControlPath.Matches(action.bindings[i].effectivePath, control))
                    return slot;

                slot++;
            }
            return -1;
        }

        // ---- selection ----

        /// <summary>Select an absolute slot index.</summary>
        public void Select(int index)
        {
            if ((uint)index >= (uint)ActiveCount) return;
            if (_hasSelection && index == _selected) return;

            if (_hasSelection && (uint)_selected < (uint)ActiveCount)
                Slots[_selected].SetSelected(false);

            bool changed = !_hasSelection || index != _selected;
            _selected = index;
            _hasSelection = true;
            Slots[_selected].SetSelected(true);
            if (changed) SelectionChanged?.Invoke(_selected);
        }

        /// <summary>Scroll by a delta with wrapping (handles steps &gt; 1 and negatives).</summary>
        public void Scroll(int delta)
        {
            if (delta == 0 || ActiveCount == 0) return;
            int next = ((_selected + delta) % ActiveCount + ActiveCount) % ActiveCount;
            Select(next);
        }

        protected override void RefreshSlot(int index)
        {
            base.RefreshSlot(index);

            // Re-apply selection after a render so the highlight survives a
            // content change on the selected slot. Instant: no animation on
            // a data refresh, only on an actual selection change.
            if (index == _selected && (uint)index < (uint)ActiveCount)
                Slots[index].SetSelected(true, instant: true);
        }

        // ---- hotkey labels ----

        /// <summary>
        /// Push hotkey labels into the slots. Slot i gets the display string of
        /// the i-th (non-composite) Select binding; slots without a binding, or
        /// when auto-numbering is off, get "".
        /// </summary>
        public void RefreshHotKeyLabels()
        {
            for (int i = 0; i < SlotCount; i++)
                Slots[i].SetHotKey(LabelForSlot(i));
        }

        private string LabelForSlot(int slot)
        {
            if (!autoNumberKeys || selectAction == null || selectAction.action == null)
                return "";

            var action = selectAction.action;
            int bindingIndex = ResolveBindingIndex(action, slot);
            return bindingIndex >= 0 ? action.GetBindingDisplayString(bindingIndex) : "";
        }

        // ---- optional binding setup ----

        /// <summary>
        /// Opt-in: populate the Select action with up to 10 keyboard number-key
        /// bindings (1,2,...,9,0) in slot order, if it has none. Lets a project
        /// drop the HotBar in without wiring bindings in the .inputactions asset.
        /// Call before the action is used (e.g. right after assigning the
        /// reference, before OnEnable). No-op if the action already has bindings
        /// or the reference is unset. Refreshes labels afterward.
        ///
        /// NOTE: this calls AddBinding on the referenced action, which mutates
        /// the shared InputAction asset for the session. Intended for actions
        /// dedicated to this HotBar. Don't point it at a shared action you also
        /// author bindings on elsewhere.
        /// </summary>
        /// <param name="count">How many number keys to bind (1-10). Clamped.</param>
        public void EnsureNumberKeyBindings(int count = 10)
        {
            if (selectAction == null || selectAction.action == null) return;

            var action = selectAction.action;

            // Only generate if there are no non-composite bindings already, so
            // we never fight a designer-authored set or duplicate on replay.
            if (ResolveBindingIndex(action, 0) >= 0) return;

            count = Mathf.Clamp(count, 1, 10);

            bool wasEnabled = action.enabled;
            action.Disable();

            // Keys read 1..9 then 0, matching a keyboard's number row order.
            for (int slot = 0; slot < count; slot++)
            {
                int key = slot == 9 ? 0 : slot + 1;
                action.AddBinding($"<Keyboard>/{key}");
            }

            if (wasEnabled) action.Enable();
            RefreshHotKeyLabels();
        }

        // ---- runtime rebinding ----

        /// <summary>
        /// Interactively rebind the Select binding for a given slot (0-based,
        /// counting only non-composite bindings). Call onComplete to refresh UI.
        /// </summary>
        public void RebindSelectKey(int slot, System.Action onComplete = null)
        {
            if (selectAction == null || selectAction.action == null) return;

            var action = selectAction.action;
            int bindingIndex = ResolveBindingIndex(action, slot);
            if (bindingIndex < 0) return;

            bool wasEnabled = action.enabled;
            action.Disable();

            action.PerformInteractiveRebinding(bindingIndex)
                .OnComplete(op =>
                {
                    op.Dispose();
                    if (wasEnabled) action.Enable();
                    RefreshHotKeyLabels();
                    onComplete?.Invoke();
                })
                .Start();
        }

        private static int ResolveBindingIndex(InputAction action, int slot)
        {
            int seen = 0;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isComposite || action.bindings[i].isPartOfComposite)
                    continue;

                if (seen == slot) return i;
                seen++;
            }
            return -1;
        }
    }
}