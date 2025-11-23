using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ShapeTemplate))]
public class ShapeEditor : UnityEditor.Editor
{
    private const int GridSize = 5;
    private const float CellSize = 25f;

    public override void OnInspectorGUI()
    {
        ShapeTemplate shape = (ShapeTemplate)target;

        GUILayout.Label("Shape Grid (5x5)", EditorStyles.boldLabel);

        // Draw 5x5 grid
        for (int row = 0; row < GridSize; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < GridSize; col++)
            {
                bool oldVal = shape.rows[row].cells[col];
                bool newVal = GUILayout.Toggle(oldVal, GUIContent.none, "Button", GUILayout.Width(CellSize), GUILayout.Height(CellSize));

                if (newVal != oldVal)
                {
                    Undo.RecordObject(shape, "Edit Shape Cell");
                    shape.rows[row].cells[col] = newVal;
                    EditorUtility.SetDirty(shape);
                }
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        DrawDefaultInspector(); // Keep other fields (score, chance, etc.)
    }
}
