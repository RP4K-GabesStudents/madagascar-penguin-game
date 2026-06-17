using System;
using UnityEngine;

namespace UI.Effects
{
    /// <summary>
    /// The shared back-and-forth term any effect can embed: a sine swing
    /// scaled by the effect's blend, so motion fades in on activate and fades
    /// out to exactly zero on deactivate (zero swing IS the resting pose, so
    /// returning to normal needs no special handling).
    ///
    /// Amplitude units are the embedding effect's: degrees for rotation,
    /// scale/pixels for scale, pixels for move, gradient position for color.
    /// </summary>
    [Serializable]
    public struct UIOscillation
    {
        [SerializeField] private bool enabled;
        [SerializeField, Min(0f)] private float frequency;
        [SerializeField] private float amplitude;

        public bool Enabled => enabled;

        /// <summary>Current swing offset. 0 when disabled or blend is 0.</summary>
        public float Evaluate(float clock, float blend)
        {
            if (!enabled) return 0f;
            return Mathf.Sin(clock * frequency * 2f * Mathf.PI) * amplitude * blend;
        }
    }
}
