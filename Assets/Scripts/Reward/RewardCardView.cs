using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards;

public class RewardCardView : MonoBehaviour
{
    [Header("Hook up your thumbnail prefab & parent")]
    public Transform thumbnailParent;        // κενό αντικείμενο μέσα στην κάρτα
    public GameObject cardThumbnailPrefab;   // prefab που περιέχει CardDisplay
    public TMP_Text debugLabel;              // optional label
    public Button button;                    // κουμπί επιλογής (στο root)

    [HideInInspector] public RewardDefinition def;

    private System.Action<RewardCardView> onChosen;
    private GameObject spawnedThumb;

    public void Setup(RewardDefinition def, System.Action<RewardCardView> onChosen)
    {
        this.def = def;
        this.onChosen = onChosen;

        if (debugLabel) debugLabel.text = def.cardData ? def.cardData.cardName : "(null)";

        if (spawnedThumb) Destroy(spawnedThumb);

        if (cardThumbnailPrefab && thumbnailParent)
        {
            spawnedThumb = Instantiate(cardThumbnailPrefab, thumbnailParent);

            var display = spawnedThumb.GetComponentInChildren<CardDisplay>(true);
            if (display != null && def.cardData != null)
            {
                // Προσαρμόσ’ το αν η μέθοδος ονομάζεται αλλιώς (π.χ. Bind / Init / Apply)
                display.SetData(def.cardData);
            }
            else
            {
                Debug.LogWarning("[Reward] RewardCardView: Δεν βρέθηκε CardDisplay ή λείπει cardData.");
            }
        }

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => this.onChosen?.Invoke(this));
            button.interactable = true;
        }
    }

    public void Interactable(bool value)
    {
        if (button) button.interactable = value;
    }
}
