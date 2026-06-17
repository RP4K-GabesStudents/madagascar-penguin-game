using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace InventorySystem.UI.EditorTools
{
    /// <summary>
    /// Adds Generate / Clear buttons so a designer builds the grid in place at
    /// edit time. Generated slots are real children baked into the prefab or
    /// scene, recorded for Undo. CustomEditor inheritFromParent is true so a
    /// HotBar subclass gets the same buttons for free.
    /// </summary>
    [CustomEditor(typeof(GridInventory), true)]
    public class GridInventoryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var grid = (GridInventory)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generated slots", grid.SlotCount.ToString());

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"Generate ({grid.EditorPreviewCount})", GUILayout.Height(24)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Generate Inventory Slots");
                    grid.Generate(grid.EditorPreviewCount);
                    MarkDirty(grid);
                }

                if (GUILayout.Button("Clear", GUILayout.Height(24)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Clear Inventory Slots");
                    grid.Clear();
                    MarkDirty(grid);
                }
            }
        }

        private static void MarkDirty(GridInventory grid)
        {
            EditorUtility.SetDirty(grid);
            if (!Application.isPlaying && grid.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(grid.gameObject.scene);
        }
    }
}
