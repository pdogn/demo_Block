using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShapeTemplate))]
public class ShapeEditor : UnityEditor.Editor
{
    private const int GridSize = 5;
    private const float CellSize = 25f;
    private SerializedProperty rowsProp;

    private void OnEnable()
    {
        rowsProp = serializedObject.FindProperty("rows");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Label("Shape Grid (5x5)", EditorStyles.boldLabel);

        for (int row = 0; row < GridSize; row++)
        {
            SerializedProperty rowProp = rowsProp.GetArrayElementAtIndex(row)
                                                 .FindPropertyRelative("cells");

            GUILayout.BeginHorizontal();

            for (int col = 0; col < GridSize; col++)
            {
                SerializedProperty cellProp = rowProp.GetArrayElementAtIndex(col);
                bool oldVal = cellProp.boolValue;

                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = oldVal ? Color.green : Color.gray;

                bool newVal = GUILayout.Toggle(oldVal, GUIContent.none, "Box",
                    GUILayout.Width(CellSize), GUILayout.Height(CellSize));

                GUI.backgroundColor = prev;

                if (newVal != oldVal)
                    cellProp.boolValue = newVal;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        // Draw remaining fields except rows
        DrawPropertiesExcluding(serializedObject, "rows");

        serializedObject.ApplyModifiedProperties();
    }
}
