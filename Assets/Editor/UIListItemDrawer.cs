using UnityEngine;
using UnityEditor;
using SaturnGame.UI;

[CustomPropertyDrawer(typeof(UIListItem))]
public class UIListItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel++;

        SerializedProperty subtitleType = property.FindPropertyRelative("subtitleType");
        SerializedProperty itemType = property.FindPropertyRelative("itemType");
        SerializedProperty title = property.FindPropertyRelative("title");
        SerializedProperty color = property.FindPropertyRelative("color");

        EditorGUILayout.PropertyField(subtitleType);
        EditorGUILayout.PropertyField(itemType);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(color);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(title);

        switch (subtitleType.enumValueIndex)
        {
            case (int)UIListItem.SubtitleTypes.Static:
            {
                SerializedProperty subtitle = property.FindPropertyRelative("subtitle");
                EditorGUILayout.PropertyField(subtitle);
                break;
            }
            case (int)UIListItem.SubtitleTypes.Dynamic:
            {
                SerializedProperty settingsBinding = property.FindPropertyRelative("settingsBinding");
                EditorGUILayout.PropertyField(settingsBinding);
                break;
            }
        }

        EditorGUILayout.Space();


        switch (itemType.enumValueIndex)
        {
            case (int)UIListItem.ItemTypes.SubMenu:
            {
                SerializedProperty nextScreen = property.FindPropertyRelative("nextScreen");
                EditorGUILayout.PropertyField(nextScreen);
                break;
            }
            case (int)UIListItem.ItemTypes.ValueSetter:
            {
                SerializedProperty settingsParameter = property.FindPropertyRelative("settingsParameter");
                SerializedProperty settingsValue = property.FindPropertyRelative("settingsValue");

                EditorGUILayout.PropertyField(settingsParameter);
                EditorGUILayout.PropertyField(settingsValue);
                break;
            }
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
}
