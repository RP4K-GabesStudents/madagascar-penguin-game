using Unity.Netcode;
using UnityEngine;
using InventorySystem.Model;

namespace InventorySystem.Netcode
{
    /// <summary>
    /// Networked anchor follower for held items under NGO. Begins following on
    /// OnNetworkSpawn (per client), because under netcode the parent hierarchy
    /// and the player's anchor aren't reliably resolved at OnEnable; the object
    /// is only meaningfully placed once it has spawned. Ticks the pose in the
    /// chosen update mode.
    ///
    /// The follow runs on every client independently, tracking the locally
    /// animated bone, so the held item sits in the mouth identically everywhere
    /// with nothing to sync. Because it never reparents onto the bone, the item
    /// stays a free NetworkObject under the player root and drops in place if
    /// the owner disconnects.
    ///
    /// Pose logic lives in AnchorConstraint (netcode-free, in Model); this type
    /// owns the spawn-time trigger and the per-frame tick. Composition, not
    /// inheritance, so it's free to extend NetworkBehaviour.
    /// </summary>
    [DisallowMultipleComponent]
    public class HeldItemAnchorFollower : NetworkBehaviour
    {
        [Tooltip("When the pose is evaluated. LateUpdate for an animated bone.")]
        [SerializeField] private AnchorUpdateMode updateMode = AnchorUpdateMode.LateUpdate;
        [Tooltip("Kinematic: follow drives the pose. Dynamic: physics rests it in the anchor instead.")]
        [SerializeField] private HoldPhysicsMode holdPhysics = HoldPhysicsMode.Kinematic;
        [Tooltip("Position offset from the anchor, in anchor-local space.")]
        [SerializeField] private Vector3 positionOffset;
        [Tooltip("Rotation offset from the anchor, in euler degrees.")]
        [SerializeField] private Vector3 rotationOffset;

        private AnchorConstraint _anchor;

        public override void OnNetworkSpawn()
        {
            _anchor ??= new AnchorConstraint(gameObject, positionOffset, rotationOffset, holdPhysics);
            // Try now in case we're already parented; otherwise the parent-change
            // hook below picks it up. Under NGO the item is parented to the
            // player root AFTER Spawn() (so after this), and that parenting
            // replicates to remote clients asynchronously, so spawn alone is too
            // early to find the anchor.
            _anchor.Attach();
        }

        // NGO calls this on every client whenever the parent changes, including
        // the initial server parenting and its async replication to clients.
        // That's the moment the IHeldItemAnchor becomes reachable up the
        // hierarchy, so (re)bind the follow here.
        public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
        {
            _anchor ??= new AnchorConstraint(gameObject, positionOffset, rotationOffset, holdPhysics);
            if (parentNetworkObject != null) _anchor.Attach();
            else _anchor.Detach(); // unparented (e.g. dropped): stop following
        }

        public override void OnNetworkDespawn() => _anchor?.Detach();

        private void Update()      { if (updateMode == AnchorUpdateMode.Update) _anchor?.Tick(); }
        private void LateUpdate()  { if (updateMode == AnchorUpdateMode.LateUpdate) _anchor?.Tick(); }
        private void FixedUpdate() { if (updateMode == AnchorUpdateMode.FixedUpdate) _anchor?.Tick(); }
    }
}