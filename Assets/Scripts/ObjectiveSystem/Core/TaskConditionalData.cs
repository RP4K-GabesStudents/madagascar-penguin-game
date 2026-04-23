using System;
using UnityEngine;

namespace ObjectiveSystem.Core
{
    /// <summary>
    /// Serializable data container for a task conditional.
    /// Subclass this (not ITaskConditional directly) to define a new conditional type.
    /// Unity serializes these via [SerializeReference] on Task — no ScriptableObject wrappers needed.
    /// At runtime, call BuildConditional() to get a live ITaskConditional instance.
    /// </summary>
    [Serializable]
    public abstract class TaskConditionalData
    {
        [SerializeField] protected string taskName = "New Task";
        [SerializeField] protected bool optional;

        public string TaskName => taskName;
        public bool Optional => optional;

        /// <summary>
        /// Construct and return a live ITaskConditional from this data snapshot.
        /// Called at runtime when the Task is activated.
        /// </summary>
        public abstract ITaskConditional BuildConditional();

        /// <summary>
        /// Human-readable summary shown in the Task inspector list.
        /// </summary>
        public abstract string EditorLabel { get; }
    }
}
