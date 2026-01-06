using UnityEditor;
using UnityEngine;

namespace Game.Characters.CapabilitySystem.Capabilities.AI.Detection.Editor
{

#if UNITY_EDITOR
    [CustomEditor(typeof(Detector))]
    public class DetectorEditor : UnityEditor.Editor
    {
        private SerializedProperty gizmoSettingsProperty;
        private bool showGizmoSettings = true;

        private void OnEnable()
        {
            gizmoSettingsProperty = serializedObject.FindProperty("gizmoSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            
            // Gizmo settings foldout
            showGizmoSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGizmoSettings, "Gizmo Settings");
            
            if (showGizmoSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(gizmoSettingsProperty, true);
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button("Reset to Default Colors"))
                {
                    Detector detector = (Detector)target;
                    detector.gizmoSettings = new DetectorGizmoSettings();
                    EditorUtility.SetDirty(target);
                }
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}