using System;
using UnityEngine;

namespace UI.Effects.ScriptableObjects
{
    /// <summary>
    /// The shared back-and-forth term any effect can embed: a sine swing
    /// mapped into an explicit [min, max] range, scaled by the effect's blend
    /// so motion fades in on activate and fades out to exactly zero on
    /// deactivate (zero swing IS the resting pose, so returning to normal
    /// needs no special handling).
    ///
    /// Range units are the embedding effect's: degrees for rotation,
    /// scale/pixels for scale, pixels for move, gradient position for color.
    /// The swing is an offset added on top of whatever baseline the effect
    /// already holds. Set min/max either side of 0 for a symmetric wobble, or
    /// keep them on one side for a one-directional pulse.
    ///
    /// Lives on the effect's AnimationData ScriptableObject, not on the effect
    /// instance, so every object sharing a preset shares one copy of these
    /// fields.
    /// </summary>
    [Serializable]
    public struct UIOscillation
    {
        [SerializeField] private bool enabled;
        [Tooltip("Cycles per second.")]
        [SerializeField, Min(0f)] private float speed;
        [Tooltip("Low end of the swing, as an offset from the effect's baseline.")]
        [SerializeField] private float min;
        [Tooltip("High end of the swing, as an offset from the effect's baseline.")]
        [SerializeField] private float max;

        public bool Enabled => enabled;

        /// <summary>
        /// Current swing offset. 0 when disabled or blend is 0. The sine
        /// [-1, 1] is remapped into [min, max], then scaled by blend so the
        /// motion fades in and out with the effect.
        /// </summary>
        public float Evaluate(float clock, float blend)
        {
            if (!enabled) return 0f;

            float sin01 = (Mathf.Sin(clock * speed * 2f * Mathf.PI) + 1f) * 0.5f;
            return Mathf.LerpUnclamped(min, max, sin01) * blend;
        }
    }
}
