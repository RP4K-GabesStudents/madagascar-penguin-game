using System;
using System.Collections.Generic;
using System.Linq;
using ObjectiveSystem.Core;
using UnityEditor;
using UnityEngine;

namespace ObjectiveSystem.ScriptableObjects.Editor
{
    [CustomEditor(typeof(Objective))]
    public class TaskEditor : UnityEditor.Editor
    {
        // All concrete TaskConditionalData subtypes found via reflection
        private static List<Type> _conditionalTypes;
        private static string[] _typeNames;

        private int _completionTypeIndex;
        private int _failureTypeIndex;

        private SerializedProperty _preText;
        private SerializedProperty _curText;
        private SerializedProperty _completionConditions;
        private SerializedProperty _failureConditions;

        private static readonly Color CompletionColor = new(0.55f, 0.9f, 0.55f, 1f);
        private static readonly Color FailureColor    = new(0.95f, 0.5f, 0.5f, 1f);
        private static readonly Color HeaderColor     = new(0.18f, 0.18f, 0.18f, 1f);

        // ─── Unity Lifecycle ────────────────────────────────────────────────

        private void OnEnable()
        {
            _preText              = serializedObject.FindProperty("preText");
            _curText              = serializedObject.FindProperty("_curText");
            _completionConditions = serializedObject.FindProperty("CompletionConditions");
            _failureConditions    = serializedObject.FindProperty("FailureConditions");

            RefreshConditionalTypes();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTaskHeader();
            EditorGUILayout.Space(6);
            DrawTextSection();
            EditorGUILayout.Space(10);
            DrawConditionalList(_completionConditions, "Completion Conditions", CompletionColor, ref _completionTypeIndex);
            EditorGUILayout.Space(6);
            DrawConditionalList(_failureConditions, "Failure Conditions", FailureColor, ref _failureTypeIndex);

            serializedObject.ApplyModifiedProperties();
        }

        // ─── Drawing ────────────────────────────────────────────────────────

        private void DrawTaskHeader()
        {
            var task = (Objective)target;
            EditorGUILayout.LabelField(task.name, EditorStyles.boldLabel);
        }

        private void DrawTextSection()
        {
            EditorGUILayout.LabelField("Display Text", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_preText, new GUIContent("Pre-text Template",
                "Use {C0}, {C1}... for CompletionConditions and {F0}, {F1}... for FailureConditions"));

            using (new EditorGUI.DisabledGroupScope(true))
                EditorGUILayout.PropertyField(_curText, new GUIContent("Resolved Text"));

            if (GUILayout.Button("Rebuild Text", GUILayout.Height(22)))
            {
                serializedObject.ApplyModifiedProperties();
                ((Objective)target).RebuildText();
                serializedObject.Update();
            }
        }

        private void DrawConditionalList(SerializedProperty listProp, string label,
            Color accentColor, ref int selectedTypeIndex)
        {
            // Header bar
            var headerRect = EditorGUILayout.GetControlRect(false, 24);
            EditorGUI.DrawRect(headerRect, HeaderColor);
            headerRect.x += 4;
            GUI.Label(headerRect, label, EditorStyles.whiteBoldLabel);

            EditorGUILayout.Space(2);

            // List items
            if (listProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No conditionals. Add one below.", MessageType.None);
            }
            else
            {
                for (int i = 0; i < listProp.arraySize; i++)
                {
                    DrawConditionalEntry(listProp, i, accentColor);
                }
            }

            EditorGUILayout.Space(4);

            // Add bar
            DrawAddBar(listProp, accentColor, ref selectedTypeIndex);
        }

        private void DrawConditionalEntry(SerializedProperty listProp, int index, Color accentColor)
        {
            var entryProp = listProp.GetArrayElementAtIndex(index);
            if (entryProp.managedReferenceValue == null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"[{index}] (null)", EditorStyles.miniLabel);
                    if (GUILayout.Button("✕", GUILayout.Width(22)))
                        listProp.DeleteArrayElementAtIndex(index);
                }
                return;
            }

            // Accent strip
            var stripRect = GUILayoutUtility.GetLastRect();
            stripRect.x      = 0;
            stripRect.width  = 3;
            stripRect.height = EditorGUIUtility.singleLineHeight + 4;
            EditorGUI.DrawRect(stripRect, accentColor);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var data = entryProp.managedReferenceValue as TaskConditionalData;
                string typeName = entryProp.managedReferenceValue.GetType().Name;
                string editorLabel = data?.EditorLabel ?? typeName;
                
                using (new EditorGUILayout.HorizontalScope())
                {

                    EditorGUILayout.LabelField($"[{index}]  {typeName}", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    // Move up / down
                    using (new EditorGUI.DisabledGroupScope(index == 0))
                        if (GUILayout.Button("▲", GUILayout.Width(22))) MoveElement(listProp, index, index - 1);
                    using (new EditorGUI.DisabledGroupScope(index == listProp.arraySize - 1))
                        if (GUILayout.Button("▼", GUILayout.Width(22))) MoveElement(listProp, index, index + 1);

                    // Remove
                    var prevColor = GUI.color;
                    GUI.color = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("✕", GUILayout.Width(22)))
                    {
                        listProp.DeleteArrayElementAtIndex(index);
                        GUI.color = prevColor;
                        return;
                    }
                    GUI.color = prevColor;
                    
                }

                // Description preview
                if (data != null)
                {
                    var prevColor = GUI.color;
                    GUI.color = Color.gray;
                    EditorGUILayout.LabelField(editorLabel, EditorStyles.miniLabel);
                    GUI.color = prevColor;
                }

                EditorGUILayout.Space(2);
                // Draw all child properties
                DrawChildProperties(entryProp);
            }
        }

        private void DrawAddBar(SerializedProperty listProp, Color accentColor, ref int selectedTypeIndex)
        {
            if (_conditionalTypes == null || _conditionalTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("No TaskConditionalData subtypes found in project.", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, _typeNames, GUILayout.ExpandWidth(true));

                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = accentColor;
                if (GUILayout.Button("+ Add", GUILayout.Width(60)))
                {
                    var newInstance = Activator.CreateInstance(_conditionalTypes[selectedTypeIndex]);
                    listProp.arraySize++;
                    var newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                    newElement.managedReferenceValue = newInstance;
                }
                GUI.backgroundColor = prevColor;
            }
        }

        // ─── Helpers ────────────────────────────────────────────────────────

        /// <summary>Draw all serialized child fields of a managed reference property.</summary>
        private static void DrawChildProperties(SerializedProperty parentProp)
        {
            var iterator = parentProp.Copy();
            var end = parentProp.GetEndProperty();

            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }
        }

        private static void MoveElement(SerializedProperty list, int from, int to)
        {
            list.MoveArrayElement(from, to);
        }

        /// <summary>Find all non-abstract subclasses of TaskConditionalData via reflection.</summary>
        private static void RefreshConditionalTypes()
        {
            _conditionalTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => !t.IsAbstract &&
                            !t.IsGenericType &&
                            typeof(TaskConditionalData).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();

            _typeNames = _conditionalTypes.Select(t => t.Name
                    .Replace("ConditionalData", "")
                    .Replace("Data", ""))
                .ToArray();
        }
    }
}
