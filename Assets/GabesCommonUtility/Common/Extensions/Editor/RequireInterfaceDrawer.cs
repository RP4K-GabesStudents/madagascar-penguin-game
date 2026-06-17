using UnityEditor;
using UnityEngine;

namespace Common.Extensions.Editor
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "RequireInterface needs an object field");
                return;
            }

            var required = ((RequireInterfaceAttribute)attribute).InterfaceType;

            EditorGUI.BeginProperty(position, label, property);

            // fieldInfo.FieldType (Behaviour) keeps the object picker scoped to behaviours.
            var assigned = EditorGUI.ObjectField(position, label,
                property.objectReferenceValue, fieldInfo.FieldType, true);

            if (assigned == null || required.IsInstanceOfType(assigned))
            {
                property.objectReferenceValue = assigned;
            }
            else if (assigned is GameObject go && go.TryGetComponent(required, out var component))
            {
                property.objectReferenceValue = component;
            }
            else
            {
                Debug.LogWarning($"{assigned.name} does not implement {required.Name}.");
            }

            EditorGUI.EndProperty();
        }
    }
}