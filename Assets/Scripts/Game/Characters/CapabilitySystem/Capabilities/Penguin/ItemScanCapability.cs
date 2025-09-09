using System.Collections;
using Game.Characters.CapabilitySystem.CapabilityStats.Penguin.PenguinAbilityCapabilityStats;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using Game.Inventory;
using Game.Objects;
using Inventory;
using Managers;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin
{
    public class ItemScanCapability : BaseCapability, IInputSubscriber
    {
        private ItemScanCapabilitySTats _stats;
        private bool _isOnCooldown;
        private readonly Collider[] _items = new Collider[10];
        private Highlight _highlight;
        public override bool CanExecute()
        {
            return !_isOnCooldown;
        }

        protected override void Execute()
        {
            StartCoroutine(Effect());
            StartCoroutine(CoolDown());
        }

        private IEnumerator Effect()
        {
            int itemAmount = Physics.OverlapSphereNonAlloc(transform.position, _stats.Radius, _items, StaticUtilities.InteractableLayer);
            for (int i = 0; i < itemAmount; i++)
            {
                Collider cur = _items[i];
                Rigidbody rb = cur.attachedRigidbody;
                if (rb && rb.TryGetComponent(out IInteractable interactable) || cur.TryGetComponent(out interactable))
                {
                    interactable.OnHover();
                }
            }
            yield return new WaitForSeconds(_stats.HighlightTime);
            _highlight.enabled = false;
        }

        private IEnumerator CoolDown()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(_stats.Cooldown);
            _isOnCooldown = false;
        }

        public void BindControls(GameControls controls)
        {
            base.OnBound();
            _stats = genericStats as ItemScanCapabilitySTats;
        }
    }
}
