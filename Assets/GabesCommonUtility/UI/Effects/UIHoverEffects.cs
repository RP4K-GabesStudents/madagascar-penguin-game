using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Effects
{
    /// <summary>
    /// Hover -> player. Drives the UIEffectPlayer on pointer enter/exit.
    /// A disabled Selectable suppresses hover; no Selectable means always on.
    /// </summary>
    [RequireComponent(typeof(UIEffectPlayer))]
    public class UIHoverEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UIEffectPlayer player;
        private Selectable _selectable;

        // Editor: wire the player the moment the component is added or values change.
        private void Reset() => AutoAssign();
        private void OnValidate() { if (!player) AutoAssign(); }
        private void AutoAssign() { if (!player) player = GetComponent<UIEffectPlayer>(); }

        private void Awake()
        {
            if (!player) player = GetComponent<UIEffectPlayer>(); // runtime safety net
            _selectable = GetComponent<Selectable>();
        }

        private bool CanFire => _selectable == null || _selectable.IsInteractable();

        public void OnPointerEnter(PointerEventData e) { if (CanFire) player.Play(); }
        public void OnPointerExit(PointerEventData e) => player.Stop(); // always release
    }
}