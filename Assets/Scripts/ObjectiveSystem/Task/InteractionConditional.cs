using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    public class InteractionConditional : ITaskConditional
    {
        public InteractionConditional(bool optional, string taskName,
            string observableTypeName, int requiredInteractions = 1)
        {
            Optional = optional;
            TaskName = taskName;
            _observableTypeName = observableTypeName;
            _required = requiredInteractions;

            // Subscribe via reflection since T is not known at compile time here.
            // If you prefer a generic version like KillTaskConditional<T>, the data class
            // can use MakeGenericType the same way KillConditionalData does.
            TaskObservableEventBus<IInteractObservable>.OnActionSubmitted += OnAction;
        }

        private readonly string _observableTypeName;
        private readonly int _required;
        private int _count;

        public bool Optional { get; }
        public string TaskName { get; }
        ETaskState currentState { get; set; } = ETaskState.Active;
        public event Action OnComplete;

        private void OnAction(EActionType actionType)
        {
            if (actionType != EActionType.Interact) return;

            _count++;
            Debug.Log($"[InteractionConditional:{TaskName}] {_count}/{_required}");

            if (_count >= _required)
            {
                currentState = ETaskState.Successful;
                OnComplete?.Invoke();
            }
        }

        public ETaskState GetCurrentState()
        {
            return currentState;
        }

        public string GetDescription() => $"Interact {_count}/{_required}";

        public void Dispose() =>
            TaskObservableEventBus<IInteractObservable>.OnActionSubmitted -= OnAction;
    }
}
