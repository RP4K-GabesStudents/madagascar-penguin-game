using UnityEngine;

namespace InventorySystem.Model
{
    /// <summary>
    /// Non-networked anchor follower for held items used without NGO (a local
    /// Inventory spawning plain GameObjects). Begins following on OnEnable and
    /// ticks the pose in the chosen update mode.
    ///
    /// For networked items, use the NetworkBehaviour follower in
    /// InventorySystem.Netcode instead, which begins on OnNetworkSpawn.
    /// </summary>
    [DisallowMultipleComponent]
    public class AnchorFollower : MonoBehaviour
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

        private void OnEnable()
        {
            _anchor ??= new AnchorConstraint(gameObject, positionOffset, rotationOffset, holdPhysics);
            _anchor.Attach();
        }

        private void OnDisable() => _anchor?.Detach();

        private void Update()      { if (updateMode == AnchorUpdateMode.Update) _anchor?.Tick(); }
        private void LateUpdate()  { if (updateMode == AnchorUpdateMode.LateUpdate) _anchor?.Tick(); }
        private void FixedUpdate() { if (updateMode == AnchorUpdateMode.FixedUpdate) _anchor?.Tick(); }
    }
}