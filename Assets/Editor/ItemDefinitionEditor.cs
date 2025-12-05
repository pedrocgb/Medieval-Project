#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(Item), true)]
public class ItemDefinitionEditor : OdinEditor
{
    private const float CellSize = 24f;

    private SerializedProperty _widthProp;
    private SerializedProperty _heightProp;
    private SerializedProperty _shapeMaskProp;

    protected override void OnEnable()
    {
        base.OnEnable();

        _widthProp = serializedObject.FindProperty("_width");
        _heightProp = serializedObject.FindProperty("_height");
        _shapeMaskProp = serializedObject.FindProperty("_shapeMask");
    }

    public override void OnInspectorGUI()
    {
        if (serializedObject == null)
            return;

        // Sync with the serialized object
        serializedObject.Update();

        // Draw the Odin-powered inspector (respects FoldoutGroup, ShowIf, etc.)
        base.OnInspectorGUI();

        EditorGUILayout.Space(12);
        DrawShapeMaskEditor();

        // Apply any modified properties (including changes from Odin & from our grid)
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawShapeMaskEditor()
    {
        if (_widthProp == null || _heightProp == null || _shapeMaskProp == null)
            return;

        int width = Mathf.Max(1, _widthProp.intValue);
        int height = Mathf.Max(1, _heightProp.intValue);
        int neededSize = width * height;

        // Ensure array size matches Width * Height
        if (_shapeMaskProp.arraySize != neededSize)
        {
            _shapeMaskProp.arraySize = neededSize;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Shape Mask Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Grid is Width × Height.\n" +
            "Checked cells = occupied shape.\n" +
            "Leave cells unchecked to create L/T/other shapes.",
            MessageType.Info
        );

        // Quick actions row
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Fill (Rectangle)", GUILayout.Width(130)))
        {
            for (int i = 0; i < neededSize; i++)
            {
                _shapeMaskProp.GetArrayElementAtIndex(i).boolValue = true;
            }
        }

        if (GUILayout.Button("Clear", GUILayout.Width(80)))
        {
            for (int i = 0; i < neededSize; i++)
            {
                _shapeMaskProp.GetArrayElementAtIndex(i).boolValue = false;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField($"Grid: {width} x {height}");

        // Draw the grid: y = row, x = column -> index = x + y * width
        for (int y = 0; y < height; y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                var cellProp = _shapeMaskProp.GetArrayElementAtIndex(index);
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
#endif
