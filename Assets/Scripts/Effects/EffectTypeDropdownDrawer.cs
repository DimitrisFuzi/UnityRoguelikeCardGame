using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Effects;

[CustomPropertyDrawer(typeof(EffectData), true)]
public class EffectTypeDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight - 2);

        Type[] effectTypes = typeof(EffectData).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(EffectData)) && !t.IsAbstract)
            .ToArray();

        string[] typeNames = effectTypes.Select(t => t.Name).ToArray();

        int selectedIndex = -1;
        if (property.managedReferenceValue != null)
        {
            Type currentType = property.managedReferenceValue.GetType();
            selectedIndex = Array.IndexOf(effectTypes, currentType);
        }

        int newIndex = EditorGUI.Popup(dropdownRect, "Effect Type", selectedIndex, typeNames);
        if (newIndex != selectedIndex && newIndex >= 0)
        {
            property.managedReferenceValue = Activator.CreateInstance(effectTypes[newIndex]);
            property.serializedObject.ApplyModifiedProperties();
        }

        if (property.managedReferenceValue != null)
        {
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none, true);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 4;
        if (property.managedReferenceValue != null)
        {
            height += EditorGUI.GetPropertyHeight(property, true);
        }
        return height;
    }
}
