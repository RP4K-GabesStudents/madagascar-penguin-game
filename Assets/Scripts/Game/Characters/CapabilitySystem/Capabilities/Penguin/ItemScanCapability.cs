using System.Collections;
using System.Net.Sockets;
using System.Numerics;
using Game.Characters.CapabilitySystem.CapabilityStats.PenguinAbilityCapabilityStats;
using Inventory;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

namespace Game.Characters.CapabilitySystem.Capabilities
{
    public class ItemScanCapability : BaseCapability, IInputSubscriber
    {
        private ItemScanCapabilitySTats _stats;
        private bool _isOnCooldown;
        [SerializeField] private Transform castOrigin;
        private Item _item;
        public override bool CanExecute()
        {
            return !_isOnCooldown;
        }

        protected override void Execute()
        {
            var detected = Physics.SphereCast(castOrigin.position, _stats.DetectRange, castOrigin.forward, out RaycastHit hit, _stats.MaxRange, StaticUtilities.InteractableLayer);
            if (detected)
            {
               //highlight item
            }
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
