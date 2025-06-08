using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(DeckManager))]
public class DeckManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get the target DeckManager instance
        DeckManager deckManager = (DeckManager)target;

        // Add a button to draw the next card
        if (GUILayout.Button("Draw Next Card"))
        {
            // Find the HandManager instance in the scene
            HandManager handManager = Object.FindFirstObjectByType<HandManager>();

            // Check if the HandManager instance is found
            if (handManager != null)
            {
                // Draw a card using the DeckManager
                deckManager.DrawCard();
            }
            else
            {
                // Display an error message if HandManager is not found
                EditorGUILayout.HelpBox("HandManager not found in the scene.", MessageType.Error);
            }
        }
    }
}
#endif