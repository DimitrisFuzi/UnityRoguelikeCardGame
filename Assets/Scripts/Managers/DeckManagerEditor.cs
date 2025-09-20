#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for the DeckManager class.
/// Adds a button in the inspector to draw a card for debugging purposes.
/// </summary>
[CustomEditor(typeof(DeckManager))]
public class DeckManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DeckManager deckManager = (DeckManager)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Draw Next Card"))
        {
            HandManager handManager = Object.FindFirstObjectByType<HandManager>();

            if (handManager != null)
            {
                HandManager.Instance.StartCoroutine(DeckManager.Instance.DrawCardAsync().AsCoroutine());
                Logger.Log("Card drawn via inspector.");
            }
            else
            {
                EditorGUILayout.HelpBox("HandManager not found in the scene.", MessageType.Error);
                Logger.LogError("DeckManagerEditor: HandManager not found in the scene.");
            }
        }
    }
}
#endif
