#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Sequencing.Essential.Editor
{
    [CustomEditor(typeof(LoadSceneSequence))]
    public class LoadSceneSequenceEditor : UnityEditor.Editor
    {
        private SerializedProperty _nextProp;
        private SerializedProperty _loadModeProp;
        #if UNITY_NETCODE_GAMEOBJECTS
        private SerializedProperty _isLocalProp;        // root-level; null when netcode absent
        #endif
        private SerializedProperty _scenesToLoadProp;
        private SerializedProperty _scenesToUnloadProp;

        private void OnEnable()
        {
            _nextProp = serializedObject.FindProperty("next");
            _loadModeProp = serializedObject.FindProperty("loadMode");
#if UNITY_NETCODE_GAMEOBJECTS
            _isLocalProp = serializedObject.FindProperty("isLocal"); // may be null
#endif
            _scenesToLoadProp = serializedObject.FindProperty("scenesToLoad");
            _scenesToUnloadProp = serializedObject.FindProperty("scenesToUnload");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Guard: if a required field was renamed/removed, fall back to the default
            // inspector instead of throwing an NRE inside PropertyField. _isLocalProp
            // is intentionally excluded: it's absent when netcode isn't installed.
            if (_nextProp == null || _loadModeProp == null ||
                _scenesToLoadProp == null || _scenesToUnloadProp == null)
            {
                EditorGUILayout.HelpBox(
                    "LoadSceneSequenceEditor: a serialized field is missing. " +
                    "Falling back to the default inspector.", MessageType.Warning);
                DrawDefaultInspector();
                return;
            }

            EditorGUILayout.PropertyField(_nextProp);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_loadModeProp);

            // Root-level networked/local choice for the whole operation. The field is
            // always serialized, so this always draws. (When netcode isn't installed
            // the value is simply ignored at runtime.)
            #if UNITY_NETCODE_GAMEOBJECTS
            if (_isLocalProp != null)
            {
                bool networked = !_isLocalProp.boolValue;
                networked = EditorGUILayout.Toggle(
                    new GUIContent("Networked",
                        "On: the server drives this load/unload for all clients.\n" +
                        "Off: every peer loads/unloads these scenes locally."),
                    networked);
                _isLocalProp.boolValue = !networked;
            }
            else
            {
                // Field genuinely missing (renamed?) — surface it instead of hiding.
                EditorGUILayout.HelpBox(
                    "Could not find the 'isLocal' field on LoadSceneSequence.",
                    MessageType.Warning);
            }
            #endif

            var loadMode =
                (UnityEngine.SceneManagement.LoadSceneMode)_loadModeProp.enumValueIndex;

            EditorGUILayout.Space();

            if (loadMode == UnityEngine.SceneManagement.LoadSceneMode.Single)
            {
                EditorGUILayout.LabelField("Scenes To Load", EditorStyles.boldLabel);

                // Force exactly one element, committing the resize this frame before
                // drawing it so we never hand PropertyField a half-initialized element.
                if (_scenesToLoadProp.arraySize != 1)
                {
                    _scenesToLoadProp.arraySize = 1;
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }

                var element = _scenesToLoadProp.GetArrayElementAtIndex(0);
                if (element != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(element, new GUIContent("Scene"), true);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.HelpBox("Single mode only allows one scene to load.", MessageType.Info);
            }
            else // Additive: use Unity's default array UI (foldout, drag-reorder, +/-).
            {
                EditorGUILayout.PropertyField(_scenesToLoadProp, new GUIContent("Scenes To Load"), true);
                EditorGUILayout.PropertyField(_scenesToUnloadProp, new GUIContent("Scenes To Unload"), true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif