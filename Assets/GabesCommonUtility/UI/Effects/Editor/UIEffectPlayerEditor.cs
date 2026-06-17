using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effects.Editor
{
    [CustomEditor(typeof(UIEffectPlayer))]
    [CanEditMultipleObjects]
    public class UIEffectPlayerEditor : UnityEditor.Editor
    {
        private static List<Type> _effectTypes;

        // One SerializedObject per target so we can read/write each independently
        // while still letting Unity render mixed-value state on shared slots.
        private SerializedObject[] _sos;
        private SerializedProperty[] _lists;

        private void OnEnable()
        {
            RebuildTargetObjects();

            _effectTypes ??= AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(t => typeof(UIEffect).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition)
                .OrderBy(t => t.Name)
                .ToList();
        }

        private void RebuildTargetObjects()
        {
            _sos = targets.Select(t => new SerializedObject(t)).ToArray();
            _lists = _sos.Select(so => so.FindProperty("effects")).ToArray();
        }

        private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly a)
        {
            try { return a.GetTypes(); }
            catch (System.Reflection.ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
        }

        public override void OnInspectorGUI()
        {
            foreach (var so in _sos) so.Update();

            int shared = SharedSlotCount();        // leading slots where all targets agree on type
            int maxLen = _lists.Max(l => l.arraySize);

            for (int i = 0; i < shared; i++)
                DrawSharedSlot(i);

            if (shared < maxLen)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    targets.Length == 1
                        ? null
                        : "Some objects have additional or differently-typed effects beyond this point. " +
                          "They're edited together only where the type and order match. Reorder or add to align them.",
                    MessageType.Info);
            }

            EditorGUILayout.Space();
            DrawAddButton();

            if (GUILayout.Button("Clear All Effects"))
                ForEachList(l => l.ClearArray());

            foreach (var so in _sos) so.ApplyModifiedProperties();
        }

        // Count leading slots where every target has the same managed reference type.
        private int SharedSlotCount()
        {
            int min = _lists.Min(l => l.arraySize);
            int n = 0;
            for (int i = 0; i < min; i++)
            {
                string t0 = _lists[0].GetArrayElementAtIndex(i).managedReferenceFullTypename;
                bool allMatch = _lists.All(l =>
                    l.GetArrayElementAtIndex(i).managedReferenceFullTypename == t0);
                if (!allMatch) break;
                n++;
            }
            return n;
        }

        private void DrawSharedSlot(int i)
        {
            var first = _lists[0].GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            string typeName = first.managedReferenceFullTypename.Split('.', ' ').LastOrDefault() ?? "Effect";
            first.isExpanded = EditorGUILayout.Foldout(first.isExpanded, $"{i}: {typeName}", true);

            if (GUILayout.Button("\u25B2", GUILayout.Width(24)) && i > 0) MoveAll(i, i - 1);
            if (GUILayout.Button("\u25BC", GUILayout.Width(24)) && i < _lists[0].arraySize - 1) MoveAll(i, i + 1);
            if (GUILayout.Button("\u2715", GUILayout.Width(24)))
            {
                DeleteAllAt(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            if (first.isExpanded)
                DrawSharedChildren(i, first);

            EditorGUILayout.EndVertical();
        }

        // Walk the effect's serialized children and draw each field once, binding
        // to all targets so Unity shows mixed values and edits propagate.
        private void DrawSharedChildren(int slot, SerializedProperty firstElement)
        {
            var iter = firstElement.Copy();
            var end = iter.GetEndProperty();
            bool enterChildren = true;

            EditorGUI.indentLevel++;
            while (iter.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iter, end))
            {
                enterChildren = false;
                string relPath = RelativePath(firstElement, iter);

                // Gather the same field on every target.
                var props = new SerializedProperty[_lists.Length];
                bool ok = true;
                for (int t = 0; t < _lists.Length; t++)
                {
                    var el = _lists[t].GetArrayElementAtIndex(slot);
                    props[t] = el.FindPropertyRelative(relPath);
                    if (props[t] == null) { ok = false; break; }
                }
                if (!ok) continue;

                // showMixedValue makes the dash appear when targets disagree.
                bool mixed = AnyDiffer(props);
                EditorGUI.showMixedValue = mixed;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(props[0], true);
                if (EditorGUI.EndChangeCheck())
                {
                    // Push the first target's new value to the rest.
                    for (int t = 1; t < _lists.Length; t++)
                        _sos[t].CopyFromSerializedProperty(props[0]);
                }
                EditorGUI.showMixedValue = false;
            }
            EditorGUI.indentLevel--;
        }

        private static string RelativePath(SerializedProperty root, SerializedProperty child)
        {
            // child.propertyPath starts with root.propertyPath + "."
            return child.propertyPath.Substring(root.propertyPath.Length + 1);
        }

        private static bool AnyDiffer(SerializedProperty[] props)
        {
            for (int t = 1; t < props.Length; t++)
                if (!SerializedProperty.DataEquals(props[0], props[t]))
                    return true;
            return false;
        }

        private void DrawAddButton()
{
    if (!GUILayout.Button("Add Effect", GUILayout.Height(24))) return;

    var menu = new GenericMenu();
    foreach (var type in _effectTypes)
    {
        var captured = type;
        menu.AddItem(new GUIContent(captured.Name), false, () =>
        {
            for (int t = 0; t < _lists.Length; t++)
            {
                var l = _lists[t];
                int idx = l.arraySize;
                l.arraySize++;

                var instance = Activator.CreateInstance(captured);
                EditorAutoFill(instance, (_sos[t].targetObject as Component)?.gameObject);

                l.GetArrayElementAtIndex(idx).managedReferenceValue = instance;
            }
            foreach (var so in _sos) so.ApplyModifiedProperties();
        });
    }
    menu.ShowAsContext();
}

// Edit-time mirror of each effect's runtime fallback: grab the host RectTransform
// and the matching default SO so the new effect shows populated immediately.
private static void EditorAutoFill(object effect, GameObject host)
{
    if (effect == null) return;
    var rect = host ? host.GetComponent<RectTransform>() : null;

    switch (effect)
    {
        case UIScaleEffect:
            AssignField(effect, "target", rect);
            AssignDefault(effect, "animationData", AnimationEffectDefaults.ScaleKey);
            break;
        case UIColorEffect:
            AssignField(effect, "graphic", host ? host.GetComponent<Graphic>() : null);
            AssignDefault(effect, "animationData", AnimationEffectDefaults.ColorKey);
            break;
        case UIMoveEffect:
            AssignField(effect, "target", rect);
            AssignDefault(effect, "animationData", AnimationEffectDefaults.MoveKey);
            break;
        case UIRotateEffect:
            AssignField(effect, "target", rect);
            AssignDefault(effect, "animationData", AnimationEffectDefaults.RotateKey);
            break;
    }
}

private static void AssignField(object obj, string field, UnityEngine.Object value)
{
    if (!value) return;
    var f = obj.GetType().GetField(field,
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    if (f != null && f.FieldType.IsInstanceOfType(value)) f.SetValue(obj, value);
}

private static void AssignDefault(object obj, string field, string resourcesKey)
{
    var f = obj.GetType().GetField(field,
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    if (f == null) return;
    var asset = Resources.Load(resourcesKey, f.FieldType);
    if (asset) f.SetValue(obj, asset);
}

        private void MoveAll(int from, int to)   => ForEachList(l => l.MoveArrayElement(from, to));
        private void DeleteAllAt(int i)          => ForEachList(l => l.DeleteArrayElementAtIndex(i));

        private void ForEachList(Action<SerializedProperty> op)
        {
            for (int t = 0; t < _lists.Length; t++)
            {
                op(_lists[t]);
                _sos[t].ApplyModifiedProperties();
                _sos[t].Update();
            }
        }
    }
}