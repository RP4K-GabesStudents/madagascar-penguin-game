using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    public class KillConditional<T> : ITaskConditional where T : ITaskObservable
    {
        public KillConditional(bool optional, string taskName, int requiredAmount)
        {
            Optional = optional;
            TaskName = taskName;
            _requiredAmount = requiredAmount;

            TaskObservableEventBus<T>.OnActionSubmitted += OnAction;
        }

        private int _requiredAmount;
        private int _killCount;

        public bool Optional { get; }
        public string TaskName { get; }
        ETaskState currentState { get; set; } = ETaskState.Active;
        public event Action OnComplete;

        public void UpdateRequiredAmount(int newAmount)
        {
            _requiredAmount = newAmount;
            if (_killCount >= _requiredAmount)
                Complete();
        }

        public ETaskState GetCurrentState()
        {
            return currentState;
        }

        public string GetDescription() =>
            $"Kill {_killCount}/{_requiredAmount} {typeof(T).Name}";

        private void OnAction(EActionType actionType)
        {
            if (actionType != EActionType.Kill) return;

            _killCount++;
            Debug.Log($"[KillConditional:{TaskName}] {_killCount}/{_requiredAmount}");

            if (_killCount >= _requiredAmount)
                Complete();
        }

        private void Complete()
        {
            currentState = ETaskState.Successful;
            OnComplete?.Invoke();
        }

        public void Dispose() =>
            TaskObservableEventBus<T>.OnActionSubmitted -= OnAction;
    }
}
