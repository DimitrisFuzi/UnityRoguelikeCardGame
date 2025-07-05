using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the enemy's next intended action to the player.
/// </summary>
public class IntentDisplay : MonoBehaviour
{
    [SerializeField] private Image intentIcon;
    [SerializeField] private Text intentText;

    public void SetIntent(EnemyIntent intent)
    {
        if (intent == null) return;

        intentIcon.sprite = intent.Icon;
        intentText.text = intent.Description;
    }

    public void ClearIntent()
    {
        intentIcon.sprite = null;
        intentText.text = string.Empty;
    }
}
