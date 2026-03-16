using System;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.Task
{
    public class KillTask<T> : ITask where T : ITaskObservable
    {
        public KillTask(bool optional, string taskName, int amount)
        {
            Optional = optional;
            TaskName = taskName;
            _requiredAmount = amount;

            TaskObservableEventBus<T>.OnActionSubmitted += CheckEnemyDeath;
        }
        
        private int _requiredAmount;
        private int _killCount;

        public void UpdateAmountRequired(int newAmount)
        {
            _requiredAmount = newAmount;
            if (_killCount >= _requiredAmount)
                OnComplete?.Invoke();
        }

        private void CheckEnemyDeath(EActionType actionType)
        {
            if (actionType != EActionType.Kill) return;
            
            _killCount++;
            Debug.Log($"Kill registered [{_killCount}/{_requiredAmount}]");

            if (_killCount >= _requiredAmount)
                OnComplete?.Invoke();
        }

        public bool Optional { get; }
        public event Action OnComplete;
        public string TaskName { get; }

        public bool IsComplete() => _killCount >= _requiredAmount;

        public void Dispose()
        {
            TaskObservableEventBus<T>.OnActionSubmitted -= CheckEnemyDeath;
        }
    }
}