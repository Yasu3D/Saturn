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

        SerializedProperty title = property.FindPropertyRelative("Title");
        SerializedProperty subtitleType = property.FindPropertyRelative("SubtitleType");
        SerializedProperty color = property.FindPropertyRelative("Color");
        SerializedProperty itemType = property.FindPropertyRelative("ItemType");

        EditorGUILayout.PropertyField(title);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(subtitleType);

        if (subtitleType.enumValueIndex == (int)UIListItem.SubtitleTypes.Text)
        {
            SerializedProperty subtitle = property.FindPropertyRelative("Subtitle");
            EditorGUILayout.PropertyField(subtitle);
        }

        if (subtitleType.enumValueIndex == (int)UIListItem.SubtitleTypes.Binding)
        {
            SerializedProperty binding = property.FindPropertyRelative("Binding");
            EditorGUILayout.PropertyField(binding);
        }

        EditorGUILayout.PropertyField(color);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(itemType);

        if (itemType.enumValueIndex == (int)UIListItem.ItemTypes.SubMenu)
        {
            SerializedProperty nextScreen = property.FindPropertyRelative("NextScreen");
            EditorGUILayout.PropertyField(nextScreen);
        }

        else if (itemType.enumValueIndex == (int)UIListItem.ItemTypes.ValueSetter)
        {
            SerializedProperty parameter = property.FindPropertyRelative("Paramter");
            SerializedProperty value = property.FindPropertyRelative("Value");

            EditorGUILayout.PropertyField(parameter);
            EditorGUILayout.PropertyField(value);
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
}
