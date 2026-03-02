using GabesCommonUtility.Game;
using UnityEditor;
using UnityEngine;

namespace GabesCommonUtility.GabesCommonUtility.Game.Editor
{
    [CustomEditor(typeof(CircleRotationPlacement))]
    [CanEditMultipleObjects]
    public class CircleRotationPlacementEditor : UnityEditor.Editor
    {
        private SerializedProperty _useIncrementalAngle, _totalAngle, _incrementalAngle;
        private SerializedProperty _radius, _individualOffset, _rotationAxis, _layoutMode, _faceCenter, _tilt;

        private void OnEnable()
        {
            _useIncrementalAngle = serializedObject.FindProperty("useIncrementalAngle");
            _totalAngle = serializedObject.FindProperty("totalAngle");
            _incrementalAngle = serializedObject.FindProperty("incrementalAngle");
            _radius = serializedObject.FindProperty("radius");
            _individualOffset = serializedObject.FindProperty("individualOffset");
            _rotationAxis = serializedObject.FindProperty("rotationAxis");
            _layoutMode = serializedObject.FindProperty("layoutMode");
            _faceCenter = serializedObject.FindProperty("faceCenter");
            _tilt = serializedObject.FindProperty("tilt");
        }

        public override void OnInspectorGUI()
        {
            CircleRotationPlacement script = (CircleRotationPlacement)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(_radius);
            EditorGUILayout.PropertyField(_individualOffset);
            
            EditorGUILayout.PropertyField(_rotationAxis);
            EditorGUILayout.PropertyField(_layoutMode);
            
            EditorGUILayout.PropertyField(_faceCenter);
            EditorGUILayout.Slider(_tilt, -90f, 90f, new GUIContent("Tilt Angle"));

            EditorGUILayout.Space();
            
            // Randomize Button
            if (GUILayout.Button("Randomize Child Order"))
            {
                Undo.RegisterFullObjectHierarchyUndo(script.gameObject, "Randomize Circle Order");
                script.RandomizeChildren();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Angle Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useIncrementalAngle);

            if (_useIncrementalAngle.boolValue)
                EditorGUILayout.PropertyField(_incrementalAngle);
            else
                EditorGUILayout.PropertyField(_totalAngle);

            if (serializedObject.ApplyModifiedProperties())
            {
                script.FormatCircle();
            }

            EditorGUILayout.Space(10);
            
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Generate New Spline Object", GUILayout.Height(30)))
            {
                script.GenerateSpline();
            }
            GUI.backgroundColor = Color.white;
        }
        
        private void OnSceneGUI()
        {
            CircleRotationPlacement script = (CircleRotationPlacement)target;
            if (script == null) return;

            EditorGUI.BeginChangeCheck();
            Vector3 worldRadiusPos = script.transform.TransformPoint(script.radius);
            
            Handles.color = Color.cyan;
            Vector3 newWorldPos = Handles.PositionHandle(worldRadiusPos, script.transform.rotation);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Change Circle Radius");
                script.radius = script.transform.InverseTransformPoint(newWorldPos);
                script.FormatCircle();
            }

            Handles.Label(worldRadiusPos, $"Radius: {script.radius.magnitude:F2}");
        }
    }
}