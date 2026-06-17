using Game.Characters.CapabilitySystem.CapabilityStats;
using Game.InventorySystem;
using Game.Objects;
using InventorySystem.Core;
using InventorySystem.Model;
using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.Penguin
{
    /// <summary>
    /// Finds what the player is looking at, hovers/highlights it, and on E
    /// interacts. For a pickup it forwards the request to the server, which
    /// runs the inventory math authoritatively and despawns the world object
    /// if it was fully absorbed.
    ///
    /// What changed from the old version: pickup no longer keeps a live Item
    /// and reparents it. The world object is an IWorldItem (data only); on
    /// pickup the server calls InventoryTransfer.TryPickup, the bag stores
    /// id+count, and the world object despawns when its Count hits zero. The
    /// held visual is later (re)created by EquipCapability on equip.
    /// </summary>
    public class InteractionCapability : BaseCapability, IInputSubscriber
    {
        private IInteractable _interactable;
        private InteractionCapabilityStats _stats;

        [SerializeField] private bool showDebugRays = true;

        private Camera _interactionCamera;

        protected override void OnBound()
        {
            base.OnBound();

            _stats = genericStats as InteractionCapabilityStats;
            if (_stats == null)
                Debug.LogAssertion($"Wrong stats assigned to object {name}, expected {typeof(InteractionCapabilityStats)}, but retrieved {genericStats?.GetType()}.", gameObject);

            _interactionCamera = Camera.main ? Camera.main : FindObjectOfType<Camera>();
            if (_interactionCamera == null)
                Debug.LogError($"[InteractionCapability] No camera found. Tag your camera 'MainCamera'. Object: {gameObject.name}");
        }

        public override bool CanExecute() => IsAlive(_interactable);

        protected override void Execute()
        {
            // _interactable is an interface ref; a despawned Unity object leaves
            // the managed ref non-null but throws on member access. Check the
            // underlying Object is still alive, and drop a stale ref if not.
            if (!IsAlive(_interactable)) { _interactable = null; return; }

            _interactable.OnInteract(_owner);
            _owner.animator.SetTrigger(StaticUtilities.InteractAnimID);

            // Pickup: the interactable is also a world item. Ask the server to
            // absorb it into our inventory. Note we resolve the IWorldItem from
            // the same object; an interactable that is not an item just runs
            // its OnInteract above and we stop here.
            if (_interactable is Component c && c.TryGetComponent(out IWorldItem worldItem))
            {
                var netObj = c.GetComponent<NetworkObject>();
                if (netObj != null) RequestPickup_Rpc(netObj.NetworkObjectId);
            }

            if (!_interactable.CanHover()) HandleHovering(null);
        }

        // True only if the interactable exists AND its underlying Unity object
        // hasn't been destroyed (Unity's overloaded bool check on the Object).
        private static bool IsAlive(IInteractable interactable) =>
            interactable is UnityEngine.Object o && o;

        /// <summary>
        /// Server-authoritative pickup. Resolves the target world object,
        /// runs the shared inventory math, and despawns it if fully taken.
        /// Partial pickups (a stack bigger than the bag's room) leave the
        /// world object alive with a reduced Count.
        /// </summary>
        [Rpc(SendTo.Server)]
        private void RequestPickup_Rpc(ulong worldObjectId)
        {
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(worldObjectId, out NetworkObject netObj))
                return;

            if (!netObj.TryGetComponent(out IWorldItem worldItem)) return;

            // Authoritative gate: a held (equipped) item is the same prefab as a
            // world pickup, so without this check a player could re-absorb their
            // own held item (duplicating it) or grab another player's (stealing).
            // The hover gate in WorldItem.CanHover is only UX; THIS is what
            // actually prevents it, since it runs on the server.
            if (netObj.TryGetComponent(out WorldItem wi) && wi.IsHeld) return;

            // The bag is on this player's hierarchy. Resolve it the same way
            // InventoryCapability does, via the interface so it works for both
            // the local and networked backends.
            IInventory inventory = GetComponentInParent<IInventory>();
            if (inventory == null) return;

            // TryPickup shrinks worldItem.Count by however much fit.
            if (!inventory.TryPickup(worldItem)) return;

            if (worldItem.Count <= 0) netObj.Despawn();
        }

        private void LateUpdate() => CheckForInteractable();

        private void CheckForInteractable()
        {
            if (_interactionCamera == null)
            {
                _interactionCamera = Camera.main;
                if (_interactionCamera == null) return;
            }

            Ray cameraRay = _interactionCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            Vector3 origin = cameraRay.origin;
            Vector3 direction = cameraRay.direction;

            if (showDebugRays)
                Debug.DrawRay(origin, direction * _stats.InteractionDistance, Color.cyan, 0.1f);

            bool byPass = Physics.Raycast(origin, direction, out var byPassed, _stats.InteractionDistance, _stats.CombinedLayers);
            if (byPass)
            {
                if (showDebugRays) Debug.DrawLine(origin, byPassed.point, Color.yellow, 0.1f);

                bool directHit = ((1 << byPassed.collider.gameObject.layer) & _stats.InteractionLayers) != 0;
                if (directHit)
                {
                    if (showDebugRays) Debug.DrawLine(origin, byPassed.point, Color.red, 0.1f);

                    Rigidbody rb = byPassed.rigidbody;
                    if (rb && rb.TryGetComponent(out IInteractable interactable))
                    {
                        HandleHovering(interactable);
                        return;
                    }
                }
                else
                {
                    bool interactHit = Physics.SphereCast(origin, _stats.InteractionRadius, direction, out RaycastHit hitInfo, _stats.InteractionDistance, _stats.InteractionLayers);
                    if (interactHit)
                    {
                        if (showDebugRays) Debug.DrawLine(origin, hitInfo.point, Color.green, 0.1f);

                        Rigidbody rb = hitInfo.rigidbody;
                        if (rb && rb.TryGetComponent(out IInteractable interactable))
                        {
                            HandleHovering(interactable);
                            return;
                        }
                    }
                }
            }
            HandleHovering(null);
        }

        private void HandleHovering(IInteractable interactable)
        {
            if (_interactable == interactable) return;
            // Only drive hover-end on the OLD one if its Unity object is still
            // alive; a despawned item (e.g. just picked up) would throw.
            if (IsAlive(_interactable)) _interactable.OnHoverEndDriver();
            if (interactable != null && interactable.CanHover()) interactable.OnHoverDriver();
            _interactable = interactable;
        }

        public void BindControls(GameControls controls)
        {
            controls.Player.Interact.performed += _ => TryExecute();
        }

        private void OnDrawGizmos()
        {
            _stats ??= genericStats as InteractionCapabilityStats;
            if (_stats == null) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            Ray cameraRay = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            Vector3 origin = cameraRay.origin;
            Vector3 direction = cameraRay.direction;

            bool byPass = Physics.Raycast(origin, direction, out RaycastHit byPassed, _stats.InteractionDistance, _stats.CombinedLayers);
            if (byPass)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origin, byPassed.point);
                Gizmos.DrawWireSphere(byPassed.point, 0.05f);

                bool directHit = ((1 << byPassed.collider.gameObject.layer) & _stats.InteractionLayers) != 0;
                if (directHit)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(byPassed.point, 0.15f);
                    return;
                }

                bool interactHit = Physics.SphereCast(origin, _stats.InteractionRadius, direction, out RaycastHit hitInfo, _stats.InteractionDistance, _stats.InteractionLayers);
                if (interactHit)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(origin, hitInfo.point);
                    Gizmos.DrawWireSphere(hitInfo.point, _stats.InteractionRadius);
                    return;
                }

                Vector3 reach = origin + direction * _stats.InteractionDistance;
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                Gizmos.DrawWireSphere(reach, _stats.InteractionRadius);
                Gizmos.DrawLine(origin, reach);
            }
            else
            {
                Vector3 reach = origin + direction * _stats.InteractionDistance;
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(origin, reach);
            }
        }
    }
}