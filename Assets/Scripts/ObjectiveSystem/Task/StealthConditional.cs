using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    public class StealthConditional : ITaskConditional
    {
        public StealthConditional(bool optional, string taskName,
            bool failOnFirstSpot = true, int allowedSpotCount = 0)
        {
            Optional = optional;
            TaskName = taskName;
            _failOnFirstSpot = failOnFirstSpot;
            _allowedSpotCount = allowedSpotCount;

            TaskObservableEventBus<ISpottedObservable>.OnActionSubmitted += OnAction;
        }

        private readonly bool _failOnFirstSpot;
        private readonly int _allowedSpotCount;
        private int _spotCount;

        public bool Optional { get; }
        public string TaskName { get; }
        ETaskState currentState { get; set; } = ETaskState.Active;
        public event Action OnComplete;

        private void OnAction(EActionType actionType)
        {
            if (actionType != EActionType.Spotted) return;

            _spotCount++;
            Debug.Log($"[StealthConditional:{TaskName}] Spotted x{_spotCount}");

            bool shouldFail = _failOnFirstSpot || _spotCount > _allowedSpotCount;
            if (!shouldFail) return;

            currentState = ETaskState.Failed;
            OnComplete?.Invoke(); // Callers should check CurrentState to determine fail vs. success
        }

        public string GetDescription() =>
            _failOnFirstSpot ? "Not spotted" : $"Spotted ≤ {_allowedSpotCount}x ({_spotCount} so far)";

        public void Dispose() =>
            TaskObservableEventBus<ISpottedObservable>.OnActionSubmitted -= OnAction;
        
        public ETaskState GetCurrentState()
        {
            return currentState;
        }
    }
    
    
}
