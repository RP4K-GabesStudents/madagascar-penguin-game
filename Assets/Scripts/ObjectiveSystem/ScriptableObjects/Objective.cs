using System;
using System.Collections.Generic;
using System.Text;
using ObjectiveSystem.Core;
using UnityEngine;

namespace ObjectiveSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ObjectiveSystem/Task", fileName = "New Task")]
    public class Objective : ScriptableObject
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
            CompletionTasks.Clear();
            FailedTasks.Clear();

            foreach (var data in CompletionConditions)
            {
                if (data != null)
                {
                    var task = data.BuildConditional();
                    task.OnUpdate = OnUpdate!.Invoke;
                    CompletionTasks.Add(task);
                }
            }
            
            foreach (var data in FailureConditions)
            {
                if (data != null)
                {
                    var task = data.BuildConditional();
                    task.OnUpdate = OnUpdate!.Invoke;
                    FailedTasks.Add(task);
                }
            }
            
        }

        public void RebuildText()
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
    }
}
