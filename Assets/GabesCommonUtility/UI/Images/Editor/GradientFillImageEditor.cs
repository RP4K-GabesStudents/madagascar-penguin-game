using UnityEditor;
using UnityEngine;

namespace UI.Images.Editor
{
    [CustomEditor(typeof(GradientFillImage))]
    [CanEditMultipleObjects]
    public class GradientFillImageEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Polling now equals "component enabled": an enabled, active component runs Update()
            // every frame. A disabled component does no per-frame work, so only warn when enabled.
            var component = (GradientFillImage)target;
            bool polling = component != null && component.enabled && component.gameObject.activeInHierarchy;

            if (polling)
            {
                EditorGUILayout.HelpBox(
                    "This component is enabled, so it polls Image.fillAmount every frame in Update(). " +
                    "That's wasteful at scale.\n\n" +
                    "Cheaper options:\n" +
                    " \u2022 Disable this component and call SetFill(value) when the fill changes.\n" +
                    " \u2022 Drive the value from a Slider and use its onValueChanged event.",
                    MessageType.Warning);

                EditorGUILayout.Space();
            }

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}