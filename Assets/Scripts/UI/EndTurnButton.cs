using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;

    private void Start()
    {
        if (endTurnButton == null)
            endTurnButton = GetComponent<Button>();

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(EndTurn);
        else
            Debug.LogError("EndTurnButton: No Button component assigned.");
    }

    private void EndTurn()
    {
        Debug.Log("⏭️ End Turn Button Pressed");
        TurnManager.Instance?.EndPlayerTurn();
    }
}
