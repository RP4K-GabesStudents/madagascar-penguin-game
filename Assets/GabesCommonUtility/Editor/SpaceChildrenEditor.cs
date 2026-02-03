using UnityEditor;
using UnityEngine;

namespace GabesCommonUtility.Editor
{
    public class SpaceChildrenEditor : UnityEditor.Editor
    {
        [MenuItem("GameObject/Space Children Horizontal", false, 0)]
        private static void SpaceChildrenHorizontal(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            if (selectedObject == null) return;
    
            SpaceChildrenWindow.ShowWindow(selectedObject, SpaceChildrenWindow.SpacingMode.Horizontal);
        }
    
        [MenuItem("GameObject/Space Children Vertical", false, 0)]
        private static void SpaceChildrenVertical(MenuCommand menuCommand)
        {
            GameObject selectedObject = menuCommand.context as GameObject;
            if (selectedObject == null) return;
    
            SpaceChildrenWindow.ShowWindow(selectedObject, SpaceChildrenWindow.SpacingMode.Vertical);
        }
    
        [MenuItem("GameObject/Space Children Horizontal", true)]
        [MenuItem("GameObject/Space Children Vertical", true)]
        private static bool ValidateSpaceChildren()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.transform.childCount > 0;
        }
    }
    
    public class SpaceChildrenWindow : EditorWindow
    {
        public enum SpacingMode
        {
            Horizontal,
            Vertical
        }
    
        private static GameObject targetObject;
        private static SpacingMode mode;
        private float spacing = 1.0f;
    
        public static void ShowWindow(GameObject obj, SpacingMode spacingMode)
        {
            targetObject = obj;
            mode = spacingMode;
    
            SpaceChildrenWindow window = GetWindow<SpaceChildrenWindow>(true, "Space Children", true);
            window.minSize = new Vector2(300, 100);
            window.maxSize = new Vector2(300, 100);
            window.ShowUtility();
        }
    
        private void OnGUI()
        {
            if (targetObject == null)
            {
                Close();
                return;
            }
    
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Spacing {targetObject.name}'s children {mode.ToString().ToLower()}", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
    
            spacing = EditorGUILayout.FloatField("Spacing:", spacing);
    
            EditorGUILayout.Space(10);
    
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
    
            if (GUILayout.Button("Cancel", GUILayout.Width(80)))
            {
                Close();
            }
    
            if (GUILayout.Button("Apply", GUILayout.Width(80)))
            {
                ApplySpacing();
                Close();
            }
    
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    
        private void ApplySpacing()
        {
            if (targetObject == null) return;
    
            Undo.RegisterCompleteObjectUndo(targetObject.transform, "Space Children");
    
            Transform[] children = new Transform[targetObject.transform.childCount];
            for (int i = 0; i < targetObject.transform.childCount; i++)
            {
                children[i] = targetObject.transform.GetChild(i);
            }
    
            for (int i = 0; i < children.Length; i++)
            {
                Undo.RegisterCompleteObjectUndo(children[i], "Space Children");
    
                Vector3 newPosition = children[i].localPosition;
    
                if (mode == SpacingMode.Horizontal)
                {
                    newPosition.x = i * spacing;
                }
                else // Vertical
                {
                    newPosition.y = i * spacing;
                }
    
                children[i].localPosition = newPosition;
            }
        }
    }
}
