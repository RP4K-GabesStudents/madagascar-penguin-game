using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    /// <summary>
    /// Tracks elapsed time. Wire up Tick(Time.deltaTime) from a MonoBehaviour or TaskManager.
    /// CompleteWithin: succeeds if another conditional fires before the timer expires.
    ///                 Use OnComplete with ETaskState.Failed to detect timeout.
    /// SurviveFor:     succeeds after the timer elapses with no interruption.
    /// </summary>
    public class TimeConditional : ITaskConditional
    {
        public TimeConditional(bool optional, string taskName,
            bool surviveMode, float seconds)
        {
            Optional = optional;
            TaskName = taskName;
            _surviveMode = surviveMode;
            _duration = seconds;
        }

        private readonly bool _surviveMode;
        private readonly float _duration;
        private float _elapsed;
        private bool _finished;

        public bool Optional { get; }
        public string TaskName { get; }
        ETaskState currentState { get; set; } = ETaskState.Active;
        public event Action OnComplete;

        public float Progress => Mathf.Clamp01(_elapsed / _duration);
        public float Remaining => Mathf.Max(0f, _duration - _elapsed);

        /// <summary>Advance the timer. Call from TaskManager or a MonoBehaviour Update.</summary>
        public void Tick(float deltaTime)
        {
            if (_finished) return;

            _elapsed += deltaTime;

            if (_elapsed < _duration) return;

            _finished = true;

            if (_surviveMode)
            {
                currentState = ETaskState.Successful;
            }
            else
            {
                // CompleteWithin mode: time ran out → fail
                currentState = ETaskState.Failed;
            }

            OnComplete?.Invoke();
        }

        /// <summary>
        /// Call this from a CompleteWithin context when the player finishes in time.
        /// </summary>
        public void MarkSucceeded()
        {
            if (_finished) return;
            _finished = true;
            currentState = ETaskState.Successful;
            OnComplete?.Invoke();
        }

        public string GetDescription() =>
            _surviveMode
                ? $"Survive {_duration}s ({Remaining:F1}s left)"
                : $"Complete within {_duration}s ({Remaining:F1}s left)";

        public ETaskState GetCurrentState()
        {
            return currentState;
        }
        
        public void Dispose() { }
    }
}
