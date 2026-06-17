using System;
using UnityEngine;

namespace UI.Effects
{
    /// <summary>
    /// A visual state with a transition, as a plain serializable class.
    /// Effects do not tick themselves: a UIEffectPlayer hosts a polymorphic
    /// [SerializeReference] list of these, drives Tick only while an effect
    /// is mid-transition or sustaining, and sleeps otherwise. An effect that
    /// plays once and settles costs zero after it lands.
    ///
    /// Progress runs 0 (idle) to 1 (active); Apply maps the curved blend to
    /// visuals. Oscillating effects read OscClock inside Apply and report
    /// SustainWhileActive to keep receiving ticks at full blend.
    /// </summary>
    [Serializable]
    public abstract class UIEffect
    {
        private GameObject _owner;
        private float _progress;
        private bool _targetActive;
        private bool _locked;
        private float _clock;
        private bool _ticking;          // already in the player's running set

        public bool IsActive => _targetActive;
        protected GameObject Owner => _owner;
        protected float OscClock => _clock;

        protected abstract float Duration { get; }
        protected abstract AnimationCurve Curve { get; }
        protected abstract void Apply(float blend);

        /// <summary>True keeps ticks coming while fully active (continuous motion).</summary>
        protected virtual bool SustainWhileActive => false;

        /// <summary>Capture originals, resolve targets. Owner is the player's GameObject.</summary>
        protected virtual void OnInit() { }

        /// <summary>Hook for effects that care about the flip itself (e.g. event invokers).</summary>
        protected virtual void OnStateChanged(bool active) { }

        public void Init(GameObject owner)
        {
            _owner = owner;
            OnInit();
        }

        public void Lock(bool state) => _locked = state;

        /// <summary>
        /// Set the target state. Returns true if this effect now needs ticking
        /// (the player uses this to manage its running set).
        /// </summary>
        public bool SetState(bool active, bool bypassLock = false)
        {
            if (_locked && !bypassLock) return false;
            if (_targetActive == active) return false;

            _targetActive = active;
            if (active) _clock = 0f;
            OnStateChanged(active);
            return true;
        }

        /// <summary>Jump straight to a state, no transition.</summary>
        public void SetStateInstant(bool active)
        {
            _targetActive = active;
            _progress = active ? 1f : 0f;
            if (active) _clock = 0f;
            Apply(Sample(_progress));
        }

        /// <summary>
        /// Advance one frame. Returns true to keep ticking, false when settled
        /// with no sustained motion (the player drops it from the running set).
        /// Only the player calls this.
        /// </summary>
        public bool Tick(float deltaTime)
        {
            float goal = _targetActive ? 1f : 0f;

            if (!Mathf.Approximately(_progress, goal))
            {
                float dur = Mathf.Max(Duration, 0.0001f);
                _progress = Mathf.MoveTowards(_progress, goal, deltaTime / dur);
            }

            _clock += deltaTime;
            Apply(Sample(_progress));

            return !Mathf.Approximately(_progress, goal) || (_targetActive && SustainWhileActive);
        }

        /// <summary>
        /// Curve sample that tolerates a missing curve (treats it as linear 0..1).
        /// An inert effect whose preset never resolved has a null Curve; this
        /// keeps Tick/SetStateInstant from throwing and taking down the player's
        /// whole Update loop.
        /// </summary>
        private float Sample(float progress)
        {
            var c = Curve;
            return c != null ? c.Evaluate(progress) : progress;
        }

        // Running-set bookkeeping, owned by the player.
        internal bool MarkTicking() { if (_ticking) return false; _ticking = true; return true; }
        internal void ClearTicking() => _ticking = false;
    }
}