using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrawPileUI : MonoBehaviour
{
    public TMP_Text drawPileText;

    void Start()
    {
        if (DeckManager.Instance != null)
        {
            UpdateDrawPileUI(); // Ενημέρωση UI στην αρχή
        }
        else
        {
            Debug.LogError("❌ Το DeckManager δεν βρέθηκε!");
        }
    }
    public void UpdateDrawPileUI()
    {
        if (DeckManager.Instance != null)
        {
            drawPileText.text = DeckManager.Instance.GetDrawPileCount().ToString();
        }
    }

    void OnEnable()
    {
        DeckManager.OnDrawPileChanged += UpdateDrawPileUI; // Εγγραφή στο event για ενημέρωση UI
        
    }
    void OnDisable()
    {
        DeckManager.OnDrawPileChanged -= UpdateDrawPileUI; // Αποεγγραφή από το event
    }
}
