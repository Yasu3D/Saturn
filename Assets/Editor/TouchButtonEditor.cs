using SaturnGame.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(TouchButton), true)]
[CanEditMultipleObjects]
public class TouchButtonEditor : ButtonEditor
{
    private SerializedProperty position;
    private SerializedProperty size;

    protected override void OnEnable()
    {
        base.OnEnable();
        position = serializedObject.FindProperty("position");
        size = serializedObject.FindProperty("size");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
        EditorGUILayout.PropertyField(position);
        EditorGUILayout.PropertyField(size);
        serializedObject.ApplyModifiedProperties();
    }
}
