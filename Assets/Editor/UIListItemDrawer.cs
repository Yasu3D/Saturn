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

        switch (itemType.enumValueIndex)
        {
            case (int)UIListItem.ItemTypes.SubMenu:
            {
                EditorGUILayout.Space();
                
                SerializedProperty nextScreen = property.FindPropertyRelative("NextScreen");
                EditorGUILayout.PropertyField(nextScreen);
                break;
            }
            case (int)UIListItem.ItemTypes.ValueSetter:
            {
                SerializedProperty sprite = property.FindPropertyRelative("Sprite");
                SerializedProperty settingsParameter = property.FindPropertyRelative("SettingsParameter");
                SerializedProperty settingsValue = property.FindPropertyRelative("SettingsValue");

                EditorGUILayout.PropertyField(sprite);
                
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(settingsParameter);
                EditorGUILayout.PropertyField(settingsValue);
                break;
            }
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
}
