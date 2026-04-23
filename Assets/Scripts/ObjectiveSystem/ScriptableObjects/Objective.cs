using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ObjectiveSystem/Task", fileName = "New Task")]
    public class Objective : ScriptableObject //<< TODO, create an object based on this... Derive it, and then we can clear RAM as needed because we have to clone anyways
    {
     

        // Serialized via [SerializeReference] so Unity stores polymorphic subtype data inline.
        // At runtime, call BuildConditionals() to get live ITaskConditional instances.
        [SerializeReference] public List<TaskConditionalData> CompletionConditions = new();
        [SerializeReference] public List<TaskConditionalData> FailureConditions = new();

        // Populated at runtime from the data above — not serialized.
        [NonSerialized] public readonly List<ITaskConditional> CompletionTasks = new();
        [NonSerialized] public readonly List<ITaskConditional> FailedTasks = new();

        public readonly List<IReward> Rewards = new();

        public string Name => name;
        public event Action OnUpdate;

        /// <summary>
        /// Instantiate all conditionals from their serialized data.
        /// Call this when activating the task (e.g. from TaskManager).
        /// </summary>
        public void BuildConditionals()
        {
            CurrentStatus = EObjectiveStatus.Active;
            
            CompletionTasks.Clear();
            FailedTasks.Clear();

            foreach (var data in CompletionConditions)
            {
                if (data != null)
                {
                    var task = data.BuildConditional();
                    task.OnUpdate = Tick;
                    CompletionTasks.Add(task);
                }
            }
            
            foreach (var data in FailureConditions)
            {
                if (data != null)
                {
                    var task = data.BuildConditional();
                    task.OnUpdate = Tick;
                    FailedTasks.Add(task);
                }
            }
            
        }

        private void Tick()
        {
            OnUpdate?.Invoke();

            bool hasCompleted = true;
            foreach (var task in CompletionTasks)
            {
               if (task.Optional) continue;
               if (task.GetCurrentState() == ETaskState.Active) hasCompleted = false;
               else if (task.GetCurrentState() == ETaskState.Failed)
               {
                   CurrentStatus = EObjectiveStatus.Fail;
                   //OnComplete?.Invoke();
                   return;
               }
            }
            
            foreach (var task in FailedTasks)
            {
                if (task.Optional) continue;
                if (task.GetCurrentState() == ETaskState.Active) hasCompleted = false;
                else if (task.GetCurrentState() == ETaskState.Failed)
                {
                    CurrentStatus = EObjectiveStatus.Fail;
                    //OnComplete?.Invoke();
                    return;
                }
            }
            
            if(hasCompleted) CurrentStatus = EObjectiveStatus.Success;
            //OnComplete?.Invoke();
        }

        public void RebuildText() // EDITOR ONLY
        {
            // Format: {C0} in {F0} without {F1}
            // Resolves conditional descriptions into _curText.
            if (string.IsNullOrEmpty(preText)) return;

            StringBuilder sb = new();
            for (var i = 0; i < preText.Length; i++)
            {
                var t = preText[i];
                if (t != '{')
                {
                    sb.Append(t);
                    continue;
                }

                i++;
                if (i >= preText.Length) break;

                List<TaskConditionalData> curList = (preText[i] | 32) switch
                {
                    'c' => CompletionConditions,
                    'f' => FailureConditions,
                    _ => null
                };

                if (curList == null)
                {
                    Debug.LogError($"[Task] Unknown list token '{preText[i]}' in preText on '{name}'", this);
                    return;
                }

                StringBuilder num = new();
                while (++i < preText.Length && preText[i] != '}')
                    num.Append(preText[i]);

                if (!int.TryParse(num.ToString(), out int id) || id >= curList.Count || curList[id] == null)
                {
                    Debug.LogWarning($"[Task] Invalid conditional reference index {num} in '{name}'", this);
                    sb.Append($"[?]");
                    continue;
                }

                sb.Append(curList[id].EditorLabel);
            }

            _curText = sb.ToString();
        }

        public void RebuildTextGame()
        {
            // Format: {C0} in {F0} without {F1}
            // Resolves conditional descriptions into _curText.
            if (string.IsNullOrEmpty(preText)) return;

            StringBuilder sb = new();
            for (var i = 0; i < preText.Length; i++)
            {
                var t = preText[i];
                if (t != '{')
                {
                    sb.Append(t);
                    continue;
                }

                i++;
                if (i >= preText.Length) break;

                List<ITaskConditional> curList = (preText[i] | 32) switch
                {
                    'c' => CompletionTasks,
                    'f' => FailedTasks,
                    _ => null
                };

                if (curList == null)
                {
                    Debug.LogError($"[Task] Unknown list token '{preText[i]}' in preText on '{name}'", this);
                    return;
                }

                StringBuilder num = new();
                while (++i < preText.Length && preText[i] != '}')
                    num.Append(preText[i]);

                if (!int.TryParse(num.ToString(), out int id) || id >= curList.Count || curList[id] == null)
                {
                    Debug.LogWarning($"[Task] Invalid conditional reference index {num} in '{name}'", this);
                    sb.Append($"[?]");
                    continue;
                }

                sb.Append(curList[id].GetDescription());
            }

            _curText = sb.ToString();
        }

        private void OnValidate() => RebuildText();

        [SerializeField, TextArea] private string preText;
        [SerializeField] private string _curText;

        public string CurrentText
        {
            get
            {
                if (string.IsNullOrEmpty(_curText))
                    RebuildText();
                return _curText;
            }
        }

        public void Update()
        {
            float deltaTime = Time.deltaTime;
            foreach(ITaskConditional conditions in CompletionTasks)
                conditions.Update(deltaTime);
            foreach(ITaskConditional conditions in FailedTasks)
                conditions.Update(deltaTime);
        }

        public EObjectiveStatus CurrentStatus { get; private set; }

        public enum EObjectiveStatus
        {
            Active,
            Success,
            Fail
        }
    }
}
