using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscardPileUI : MonoBehaviour
{
    public TMP_Text discardPileText;

    void Start()
    {
        if (DeckManager.Instance != null)
        {
            UpdateDiscardPileUI(); // Ενημέρωση UI στην αρχή
        }
        else
        {
            Debug.LogError("❌ Το DeckManager δεν βρέθηκε!");
        }
    }
    public void UpdateDiscardPileUI()
    {
        if (DeckManager.Instance != null)
        {
            discardPileText.text = DeckManager.Instance.GetDiscardPileCount().ToString();
        }
    }
    void OnEnable()
    {
        DeckManager.OnDiscardPileChanged += UpdateDiscardPileUI; // Εγγραφή στο event για ενημέρωση UI
    }
    void OnDisable()
    {
        DeckManager.OnDiscardPileChanged -= UpdateDiscardPileUI; // Αποεγγραφή από το event
    }
}
