using UnityEngine;
using InventorySystem.Core;

namespace InventorySystem.Model
{
    /// <summary>
    /// When the manual follower evaluates the anchor pose. Mirrors the
    /// update-mode choice a follow camera exposes.
    ///   - LateUpdate: after all Update + the Animator have written bone poses.
    ///     Correct for following an ANIMATED bone (e.g. a mouth/head). Default.
    ///   - Update: before animation for the frame; use only for a static anchor
    ///     that isn't driven by the Animator.
    ///   - FixedUpdate: for anchors driven by physics.
    /// </summary>
    public enum AnchorUpdateMode
    {
        LateUpdate,
        Update,
        FixedUpdate
    }

    /// <summary>
    /// Makes a held item visually track the player's anchor (e.g. an animated
    /// mouth bone) WITHOUT reparenting, by copying the anchor's world pose each
    /// frame in code. This replaces a ParentConstraint specifically so the
    /// evaluation TIMING is controllable: Unity's ParentConstraint evaluates in
    /// an internal PreLateUpdate pass that you cannot reorder, which causes a
    /// frame of lag behind fast bone motion. Manual pose-copy in the chosen
    /// callback gives camera-style update-mode control instead.
    ///
    /// Written once as a plain helper (not a component). Both the local and the
    /// networked follower own one and drive Tick() from their chosen Unity
    /// callback. Composition, not inheritance, so each follower extends whatever
    /// base it needs (MonoBehaviour vs NetworkBehaviour).
    /// </summary>
    /// <summary>
    /// How the held item's root Rigidbody behaves while held.
    ///   - Kinematic: the body is made kinematic and the pose-follow drives it
    ///     (clean, no solver fighting). Default for a weapon glued to a bone.
    ///   - Dynamic: the body is left dynamic and the pose-follow is SUPPRESSED,
    ///     so physics rests the item in the mouth instead of teleporting it.
    /// Either way the body's prior isKinematic/useGravity are captured on attach
    /// and restored on detach (drop), so the item returns to its world state.
    /// </summary>
    public enum HoldPhysicsMode
    {
        Kinematic,
        Dynamic
    }

    public sealed class AnchorConstraint
    {
        private readonly Transform _self;
        private readonly Vector3 _positionOffset;
        private readonly Quaternion _rotationOffset;
        private readonly HoldPhysicsMode _holdMode;
        private readonly Rigidbody _body; // root body only; child bodies untouched
        private readonly Vector3 _worldScale; // intended world scale, captured pre-parenting

        private Transform _anchor;
        private bool _active;

        // Captured at attach, restored at detach.
        private bool _capturedKinematic;
        private bool _capturedUseGravity;
        private bool _hasCapturedState;

        public AnchorConstraint(GameObject owner, Vector3 positionOffset, Vector3 rotationOffset,
                                HoldPhysicsMode holdMode)
        {
            _self = owner.transform;
            _positionOffset = positionOffset;
            _rotationOffset = Quaternion.Euler(rotationOffset);
            _holdMode = holdMode;
            // Root body only: GetComponent (not InChildren) so a tentacle
            // weapon's child Rigidbodies keep simulating on their own.
            _body = owner.GetComponent<Rigidbody>();
            // The item's authored scale, taken from localScale so it's
            // independent of whatever parent it's under at construction time
            // (on remote clients it may already be parented to a scaled root,
            // which would corrupt a lossyScale read). Treated as the world scale
            // the item should display at; ApplyWorldScale keeps it there under
            // any parent.
            _worldScale = _self.localScale;
        }

        /// <summary>True once a valid anchor is bound and following is on.</summary>
        public bool Active => _active;

        /// <summary>
        /// Find the anchor up the hierarchy and begin following. No-op if no
        /// IHeldItemAnchor is found (e.g. a dropped world item with no holder),
        /// leaving the item wherever it currently sits. Safe to call again.
        /// </summary>
        public void Attach()
        {
            _anchor = FindAnchor();
            _active = _anchor != null;
            if (!_active) return;

            CaptureAndApplyHoldPhysics();
            ApplyWorldScale(); // undo any squash from parenting to a scaled root

            // Only snap to the anchor if we're driving the pose. In Dynamic mode
            // physics owns the transform, so we leave it where it is.
            if (_holdMode == HoldPhysicsMode.Kinematic) Apply();
        }

        /// <summary>
        /// Stop following and restore the Rigidbody to its pre-hold state, so a
        /// dropped item resumes the physics behaviour it had in the world.
        /// </summary>
        public void Detach()
        {
            _active = false;
            _anchor = null;
            RestoreHoldPhysics();
        }

        /// <summary>
        /// Drive this from the follower's chosen Unity callback (matching the
        /// AnchorUpdateMode). Cheap no-op when inactive or the anchor died.
        /// In Dynamic hold mode this does nothing: physics owns the transform.
        /// </summary>
        public void Tick()
        {
            if (!_active) return;
            if (_anchor == null) { _active = false; return; } // anchor destroyed
            if (_holdMode == HoldPhysicsMode.Dynamic) return;  // physics-driven
            Apply();
        }

        private void CaptureAndApplyHoldPhysics()
        {
            if (_body == null || _hasCapturedState) return;

            _capturedKinematic = _body.isKinematic;
            _capturedUseGravity = _body.useGravity;
            _hasCapturedState = true;

            if (_holdMode == HoldPhysicsMode.Kinematic)
            {
                // Stop residual motion, then hand the pose to the follow.
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.isKinematic = true;
            }
            // Dynamic mode: leave the body as-is so it falls/rests in the mouth.
        }

        private void RestoreHoldPhysics()
        {
            if (_body == null || !_hasCapturedState) return;

            _body.isKinematic = _capturedKinematic;
            _body.useGravity = _capturedUseGravity;
            _hasCapturedState = false;
        }

        private void ApplyWorldScale()
        {
            Transform p = _self.parent;
            if (p == null)
            {
                _self.localScale = _worldScale;
                return;
            }

            // localScale needed so that localScale * parent.lossyScale == target.
            Vector3 ps = p.lossyScale;
            _self.localScale = new Vector3(
                Mathf.Approximately(ps.x, 0f) ? _worldScale.x : _worldScale.x / ps.x,
                Mathf.Approximately(ps.y, 0f) ? _worldScale.y : _worldScale.y / ps.y,
                Mathf.Approximately(ps.z, 0f) ? _worldScale.z : _worldScale.z / ps.z);
        }

        private void Apply()
        {
            // Follow the anchor's position and rotation only; never inherit its
            // scale. Mixamo bones carry non-unit scale, and pulling rotation out
            // of a scaled localToWorldMatrix also skews it, so we use the
            // anchor's clean world rotation directly and offset in that space.
            Quaternion worldRot = _anchor.rotation * _rotationOffset;
            Vector3 worldPos = _anchor.position + _anchor.rotation * _positionOffset;
            _self.SetPositionAndRotation(worldPos, worldRot);
        }

        // The held item parents to the player ROOT, but the IHeldItemAnchor
        // (InventoryCapability) lives on a child branch of the root (e.g. a
        // "Capabilities" object), not on an ancestor of the item. So an upward
        // walk alone misses it. We instead climb to the top of this item's own
        // hierarchy (the player root it was parented under) and search that
        // root's children. Scoping to _self.root keeps it unambiguous: we only
        // ever look inside the one player this item belongs to, never globally.
        private Transform FindAnchor()
        {
            // First try the cheap upward walk (covers the case where the anchor
            // IS on an ancestor, e.g. directly on the root).
            var holder = _self.GetComponentInParent<IHeldItemAnchor>();
            if (holder != null) return holder.AnchorTransform;

            // Otherwise search down from the top of this item's hierarchy.
            holder = _self.root.GetComponentInChildren<IHeldItemAnchor>(true);
            return holder?.AnchorTransform;
        }
    }
}