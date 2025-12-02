using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemDefinition))]
public class ItemDefinitionEditor : Editor
{
    private const float CellSize = 24f;

    SerializedProperty widthProp;
    SerializedProperty heightProp;
    SerializedProperty shapeMaskProp;

    private void OnEnable()
    {
        widthProp = serializedObject.FindProperty("Width");
        heightProp = serializedObject.FindProperty("Height");
        shapeMaskProp = serializedObject.FindProperty("ShapeMask");
    }

    public override void OnInspectorGUI()
    {
        // Make sure we’re synced with the object
        serializedObject.Update();

        // Draw normal inspector for all fields (including ShapeMask list)
        EditorGUILayout.LabelField("Item Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        DrawShapeMaskEditor();

        // Write back any changes
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawShapeMaskEditor()
    {
        if (widthProp == null || heightProp == null || shapeMaskProp == null)
            return;

        int width = Mathf.Max(1, widthProp.intValue);
        int height = Mathf.Max(1, heightProp.intValue);
        int neededSize = width * height;

        // Force the ShapeMask array size to match Width * Height
        if (shapeMaskProp.arraySize != neededSize)
        {
            shapeMaskProp.arraySize = neededSize;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Shape Mask Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Grid is Width × Height.\n" +
            "Checked cells = occupied shape.\n" +
            "Leave cells unchecked to create L/T/other shapes.",
            MessageType.Info
        );

        // Quick actions
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Fill (Rectangle)", GUILayout.Width(130)))
        {
            for (int i = 0; i < neededSize; i++)
            {
                shapeMaskProp.GetArrayElementAtIndex(i).boolValue = true;
            }
        }
        if (GUILayout.Button("Clear", GUILayout.Width(80)))
        {
            for (int i = 0; i < neededSize; i++)
            {
                shapeMaskProp.GetArrayElementAtIndex(i).boolValue = false;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField($"Grid: {width} x {height}");

        // Draw the grid: y = row, x = column
        // index = x + y * width
        for (int y = 0; y < height; y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                var cellProp = shapeMaskProp.GetArrayElementAtIndex(index);
                bool value = cellProp.boolValue;

                bool newValue = GUILayout.Toggle(
                    value,
                    GUIContent.none,
                    GUILayout.Width(CellSize),
                    GUILayout.Height(CellSize)
                );

                if (newValue != value)
                {
                    cellProp.boolValue = newValue;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
