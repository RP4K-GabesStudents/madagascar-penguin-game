using System;
using ObjectiveSystem.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ObjectiveSystem.Task
{
    public class SceneChangeConditional : ITaskConditional
    {
        public SceneChangeConditional(bool optional, string taskName, string targetSceneName = "")
        {
            Optional = optional;
            TaskName = taskName;
            _targetSceneName = targetSceneName;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private readonly string _targetSceneName;

        public bool Optional { get; }
        public string TaskName { get; }
        ETaskState currentState { get; set; } = ETaskState.Active;
        public event Action OnComplete;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool matches = string.IsNullOrEmpty(_targetSceneName) ||
                           scene.name.Equals(_targetSceneName, StringComparison.OrdinalIgnoreCase);

            if (!matches) return;

            currentState = ETaskState.Successful;
            OnComplete?.Invoke();
            Dispose();
        }
        
        public ETaskState GetCurrentState()
        {
            return currentState;
        }

        public string GetDescription() =>
            string.IsNullOrEmpty(_targetSceneName)
                ? "Enter any new scene"
                : $"Enter scene '{_targetSceneName}'";

        public void Dispose() =>
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
