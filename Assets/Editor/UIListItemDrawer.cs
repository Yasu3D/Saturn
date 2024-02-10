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
        SerializedProperty subtitle = property.FindPropertyRelative("Subtitle");
        SerializedProperty color = property.FindPropertyRelative("Color");
        SerializedProperty type = property.FindPropertyRelative("Type");

        EditorGUILayout.PropertyField(title);
        EditorGUILayout.PropertyField(subtitle);
        EditorGUILayout.PropertyField(color);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(type);

        if (type.enumValueIndex == (int)UIListItem.ItemType.SubMenu)
        {
            SerializedProperty nextScreen = property.FindPropertyRelative("NextScreen");
            EditorGUILayout.PropertyField(nextScreen);
        }

        else if (type.enumValueIndex == (int)UIListItem.ItemType.ValueSetter)
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
