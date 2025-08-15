using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards; // για τον τύπο Card

public class RewardCardView : MonoBehaviour
{
    [Header("Hook up your thumbnail prefab & parent")]
    public Transform thumbnailParent;        // κενό αντικείμενο μέσα στην κάρτα
    public GameObject cardThumbnailPrefab;   // το δικό σου prefab (περιέχει CardDisplay)
    public TMP_Text debugLabel;              // προαιρετικό label για να βλέπεις το όνομα
    public Button button;                    // το κουμπί επιλογής (στο root)

    [HideInInspector] public RewardDefinition def;

    private System.Action<RewardCardView> onChosen;
    private GameObject spawnedThumb;

    public void Setup(RewardDefinition def, System.Action<RewardCardView> onChosen)
    {
        this.def = def;
        this.onChosen = onChosen;

        if (debugLabel) debugLabel.text = def.GetCardName();

        // καθάρισε τυχόν παλιά instantiates
        if (spawnedThumb) Destroy(spawnedThumb);

        // Φτιάξε το thumbnail και δέσε το πραγματικό Card asset
        if (cardThumbnailPrefab && thumbnailParent)
        {
            spawnedThumb = Instantiate(cardThumbnailPrefab, thumbnailParent);

            var display = spawnedThumb.GetComponentInChildren<CardDisplay>(true);
            if (display != null && def.cardAsset != null)
            {
                // Δίνουμε απευθείας το Card στο CardDisplay
                display.SetData((Card)def.cardAsset);
            }
            else
            {
                Debug.LogWarning("[Reward] RewardCardView: Δεν βρέθηκε CardDisplay στο CardThumbnail ή λείπει cardAsset.");
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
