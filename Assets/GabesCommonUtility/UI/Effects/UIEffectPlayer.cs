using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Effects
{
    /// <summary>
    /// Hosts a polymorphic list of UIEffect instances and is the only thing
    /// that ticks. Code and triggers talk to THIS, never to the effects:
    /// Play() / Stop() / Toggle().
    ///
    /// Ticking: a running set holds only effects that are mid-transition or
    /// sustaining. Update runs solely while that set is non-empty, and the
    /// component disables its own Update by emptying the set, so an element
    /// whose effects have all settled costs nothing. Effects that play once
    /// and idle drop out of the set the frame they land.
    /// </summary>
    public class UIEffectPlayer : MonoBehaviour
    {
        [SerializeReference] private List<UIEffect> effects = new();

        private readonly List<UIEffect> _running = new();
        private bool _initialized;

        private void Awake() => EnsureInit();

        private void OnEnable()
        {
            // If re-enabled with nothing pending, go back to sleep cleanly.
            // Done here, after Unity's own enable pass, rather than inside
            // EnsureInit where toggling 'enabled' races OnEnable.
            if (_initialized && _running.Count == 0) enabled = false;
        }

        private void EnsureInit()
        {
            if (_initialized) return;
            for (int i = 0; i < effects.Count; i++)
                effects[i]?.Init(gameObject);
            _initialized = true;
            // Don't touch 'enabled' here. Update self-sleeps when _running
            // empties, and Submit wakes it.
        }

        public void Play() => SetAll(true);
        public void Stop() => SetAll(false);

        public void SetAll(bool active, bool bypassLock = false)
        {
            EnsureInit();
            for (int i = 0; i < effects.Count; i++)
                Submit(effects[i], active, bypassLock);
        }

        /// <summary>Drive one effect by list index, if you want fine control.</summary>
        public void SetAt(int index, bool active, bool bypassLock = false)
        {
            EnsureInit();
            if ((uint)index >= (uint)effects.Count) return;
            Submit(effects[index], active, bypassLock);
        }

        public void Toggle()
        {
            EnsureInit();
            // Mirror the first effect's state; effects are normally driven together.
            bool target = effects.Count == 0 || !effects[0].IsActive;
            SetAll(target);
        }

        public void LockAll(bool state)
        {
            EnsureInit();
            for (int i = 0; i < effects.Count; i++) effects[i]?.Lock(state);
        }

        public void SetAllInstant(bool active)
        {
            EnsureInit();
            for (int i = 0; i < effects.Count; i++) effects[i]?.SetStateInstant(active);
            // Instant means no transition; only sustained-while-active effects keep ticking.
            _running.Clear();
            if (active)
                for (int i = 0; i < effects.Count; i++)
                    if (effects[i] != null && effects[i].Tick(0f) && effects[i].MarkTicking())
                        _running.Add(effects[i]);
            enabled = _running.Count > 0;
        }

        private void Submit(UIEffect effect, bool active, bool bypassLock)
        {
            if (effect == null) return;
            if (!effect.SetState(active, bypassLock)) return; // no change
            if (effect.MarkTicking())
            {
                _running.Add(effect);
                enabled = true;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = _running.Count - 1; i >= 0; i--)
            {
                var effect = _running[i];
                if (!effect.Tick(dt))
                {
                    effect.ClearTicking();
                    _running.RemoveAt(i);
                }
            }

            if (_running.Count == 0) enabled = false; // sleep until next Play/Stop
        }
    }
}