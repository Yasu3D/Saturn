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

        SerializedProperty visibilityType = property.FindPropertyRelative("VisibilityType");
        EditorGUILayout.PropertyField(visibilityType);
        EditorGUILayout.Space();
        
        switch (visibilityType.enumValueIndex)
        {
            case (int)UIListItem.VisibilityTypes.Always: break;
            case (int)UIListItem.VisibilityTypes.Equals:
            {
                SerializedProperty conditionParameter = property.FindPropertyRelative("ConditionParameter");
                SerializedProperty conditionValue = property.FindPropertyRelative("ConditionValue");

                EditorGUILayout.LabelField("Only display this Item when Parameter = Value");
                EditorGUILayout.PropertyField(conditionParameter);
                EditorGUILayout.PropertyField(conditionValue);
                EditorGUILayout.Space();
                break;
            }
        }

        SerializedProperty subtitleType = property.FindPropertyRelative("SubtitleType");
        SerializedProperty itemType = property.FindPropertyRelative("ItemType");
        SerializedProperty title = property.FindPropertyRelative("Title");
        SerializedProperty color = property.FindPropertyRelative("Color");

        EditorGUILayout.PropertyField(itemType);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(color);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(title);

        // Subtitle type
        switch (itemType.enumValueIndex)
        {
            case (int)UIListItem.ItemTypes.SubMenu:
            {
                EditorGUILayout.PropertyField(subtitleType);
                break;
            }
            case (int)UIListItem.ItemTypes.ValueSetter:
            {
                // For ValueSetter items, the subtitle type is always static
                subtitleType.enumValueIndex = (int)UIListItem.SubtitleTypes.Static;
                break;
            }
        }

        // Subtitle
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

        // List action (next screen or setting params)
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
                SerializedProperty settingsType = property.FindPropertyRelative("SettingsType");

                EditorGUILayout.PropertyField(sprite);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(settingsParameter);
                // In an ideal world, this can be automatically read from the actual setting...
                EditorGUILayout.PropertyField(settingsType);
                switch (settingsType.enumValueIndex)
                {
                    case (int)UIListItem.ValueType.Int:
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("SettingsValueInt"));
                        break;
                    }
                    case (int)UIListItem.ValueType.Enum:
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("SettingsValueEnum"));
                        break;
                    }
                }

                break;
            }
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
}
