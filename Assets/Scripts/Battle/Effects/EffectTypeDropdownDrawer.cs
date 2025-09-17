using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Effects;

[CustomPropertyDrawer(typeof(EffectData), true)]
public class EffectTypeDropdownDrawer : PropertyDrawer
{
    // Draws the property GUI in the inspector
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Calculate rect for the dropdown and for the properties below
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight - 2);

        // Get all non-abstract subclasses of EffectData to populate dropdown
        Type[] effectTypes = typeof(EffectData).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(EffectData)) && !t.IsAbstract)
            .ToArray();

        // Get type names for display in dropdown
        string[] typeNames = effectTypes.Select(t => t.Name).ToArray();

        int selectedIndex = -1;

        // Find the current selected type index in the dropdown
        if (property.managedReferenceValue != null)
        {
            Type currentType = property.managedReferenceValue.GetType();
            selectedIndex = Array.IndexOf(effectTypes, currentType);
        }

        // Draw dropdown and check for changes
        int newIndex = EditorGUI.Popup(dropdownRect, "Effect Type", selectedIndex, typeNames);
        if (newIndex != selectedIndex && newIndex >= 0)
        {
            // Instantiate the newly selected EffectData type
            property.managedReferenceValue = Activator.CreateInstance(effectTypes[newIndex]);
            property.serializedObject.ApplyModifiedProperties();
        }

        // Draw the property fields of the selected EffectData instance below dropdown
        if (property.managedReferenceValue != null)
        {
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none, true);
        }

        EditorGUI.EndProperty();
    }

    // Calculate the height needed for this property drawer, including dropdown and property fields
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 4; // Space for dropdown and margin

        if (property.managedReferenceValue != null)
        {
            // Add height of the serialized property fields for the effect instance
            height += EditorGUI.GetPropertyHeight(property, true);
        }

        return height;
    }
}
