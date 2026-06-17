using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Effects
{
    /// <summary>EventSystem selection -> player. Pair with UIHoverSelect to drive from mouse hover.</summary>
    [RequireComponent(typeof(UIEffectPlayer))]
    public class UISelectEffects : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private UIEffectPlayer player;

        // Editor: wire the player the moment the component is added or values change.
        private void Reset() => AutoAssign();
        private void OnValidate() { if (!player) AutoAssign(); }
        private void AutoAssign() { if (!player) player = GetComponent<UIEffectPlayer>(); }

        private void Awake() { if (!player) player = GetComponent<UIEffectPlayer>(); } // runtime safety net

        public void OnSelect(BaseEventData e) => player.Play();
        public void OnDeselect(BaseEventData e) => player.Stop();
    }
}