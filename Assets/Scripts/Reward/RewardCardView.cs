using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards; // ��� ��� ���� Card

public class RewardCardView : MonoBehaviour
{
    [Header("Hook up your thumbnail prefab & parent")]
    public Transform thumbnailParent;        // ���� ����������� ���� ���� �����
    public GameObject cardThumbnailPrefab;   // �� ���� ��� prefab (�������� CardDisplay)
    public TMP_Text debugLabel;              // ����������� label ��� �� ������� �� �����
    public Button button;                    // �� ������ �������� (��� root)

    [HideInInspector] public RewardDefinition def;

    private System.Action<RewardCardView> onChosen;
    private GameObject spawnedThumb;

    public void Setup(RewardDefinition def, System.Action<RewardCardView> onChosen)
    {
        this.def = def;
        this.onChosen = onChosen;

        if (debugLabel) debugLabel.text = def.GetCardName();

        // �������� ����� ����� instantiates
        if (spawnedThumb) Destroy(spawnedThumb);

        // ������ �� thumbnail ��� ���� �� ���������� Card asset
        if (cardThumbnailPrefab && thumbnailParent)
        {
            spawnedThumb = Instantiate(cardThumbnailPrefab, thumbnailParent);

            var display = spawnedThumb.GetComponentInChildren<CardDisplay>(true);
            if (display != null && def.cardAsset != null)
            {
                // ������� ��������� �� Card ��� CardDisplay
                display.SetData((Card)def.cardAsset);
            }
            else
            {
                Debug.LogWarning("[Reward] RewardCardView: ��� ������� CardDisplay ��� CardThumbnail � ������ cardAsset.");
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
