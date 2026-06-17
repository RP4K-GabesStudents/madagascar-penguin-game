using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace UI.Effects.ScriptableObjects
{
    /// <summary>
    /// Shared transition fields for every UI effect preset. Concrete presets add
    /// the per-effect target data (scale size, gradient, move offset, rotation).
    /// </summary>
    public abstract class AnimationDataSoBase : ScriptableObject
    {
        [SerializeField] protected float transitionDuration = 0.3f;
        [SerializeField] protected AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public float TransitionDuration => transitionDuration;
        public AnimationCurve TransitionCurve => transitionCurve;
    }
}