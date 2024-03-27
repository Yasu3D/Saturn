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

        SerializedProperty subtitleType = property.FindPropertyRelative("SubtitleType");
        SerializedProperty itemType = property.FindPropertyRelative("ItemType");
        SerializedProperty title = property.FindPropertyRelative("Title");
        SerializedProperty color = property.FindPropertyRelative("Color");

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
                SerializedProperty subtitle = property.FindPropertyRelative("Subtitle");
                EditorGUILayout.PropertyField(subtitle);
                break;
            }
            case (int)UIListItem.SubtitleTypes.Dynamic:
            {
                SerializedProperty settingsBinding = property.FindPropertyRelative("SettingsBinding");
                EditorGUILayout.PropertyField(settingsBinding);
                break;
            }
        }

        EditorGUILayout.Space();


        switch (itemType.enumValueIndex)
        {
            case (int)UIListItem.ItemTypes.SubMenu:
            {
                SerializedProperty nextScreen = property.FindPropertyRelative("NextScreen");
                EditorGUILayout.PropertyField(nextScreen);
                break;
            }
            case (int)UIListItem.ItemTypes.ValueSetter:
            {
                SerializedProperty settingsParameter = property.FindPropertyRelative("SettingsParameter");
                SerializedProperty settingsValue = property.FindPropertyRelative("SettingsValue");

                EditorGUILayout.PropertyField(settingsParameter);
                EditorGUILayout.PropertyField(settingsValue);
                break;
            }
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
}
