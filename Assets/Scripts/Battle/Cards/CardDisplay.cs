using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Effects;

/// <summary>
/// Updates the visual representation of a card on UI (texts, sprites, placeholders).
/// </summary>
public class CardDisplay : MonoBehaviour
{
    [Header("Card Data")]
    public Card cardData;

    [Header("UI References")]
    public Image CardImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text manaCostText;
    public Image CardCoverImage;
    public Image RarityGemImage;

    [Header("Dynamic Sprites")]
    public Sprite attackSprite;
    public Sprite guardSprite;
    public Sprite tacticSprite;

    public Sprite commonGemSprite;
    public Sprite uncommonGemSprite;
    public Sprite rareGemSprite;
    public Sprite legendaryGemSprite;

    private void Start()
    {
        if (cardData == null)
        {
            Debug.LogWarning($"CardDisplay: 'cardData' is NULL on {gameObject.name}.");
        }
        else
        {
            UpdateCardDisplay();
        }
    }

    /// <summary>Set card data and refresh UI.</summary>
    public void SetData(Card card)
    {
        cardData = card;
        UpdateCardDisplay();
    }

    /// <summary>Alias for tools that expect Refresh().</summary>
    public void Refresh() => UpdateCardDisplay();

    /// <summary>Fill text/sprites using current cardData (including placeholder values).</summary>
    public void UpdateCardDisplay()
    {
        if (cardData == null) return;

        if (nameText) nameText.text = cardData.cardName;
        if (manaCostText) manaCostText.text = cardData.energyCost.ToString();

        // Extract values from effects for placeholders
        int damage = 0, armor = 0, cards = 0, energy = 0, hpLost = 0, aoeDamage = 0, healthSet = 0;

        var effects = cardData.GetCardEffects();
        if (effects != null)
        {
            foreach (var effect in effects)
            {
                switch (effect)
                {
                    case DamageEffect e: damage = e.damageAmount; break;
                    case ArmorEffect e: armor = e.armorAmount; break;
                    case DrawCardEffect e: cards = e.cardsToDraw; break;
                    case GainEnergyEffect e: energy = e.energyAmount; break;
                    case LoseHealthEffect e: hpLost = e.healthLoss; break;
                    case AOEDamageEffect e: aoeDamage = e.damageAmount; break;
                    case SetHealthEffect e: healthSet = e.newHealth; break;
                }
            }
        }

        string finalDescription = cardData.cardDescription ?? string.Empty;
        finalDescription = finalDescription.Replace("{damage}", damage > 0 ? damage.ToString() : "-");
        finalDescription = finalDescription.Replace("{armor}", armor > 0 ? armor.ToString() : "-");
        finalDescription = finalDescription.Replace("{cards}", cards > 0 ? cards.ToString() : "-");
        finalDescription = finalDescription.Replace("{energy}", energy > 0 ? energy.ToString() : "-");
        finalDescription = finalDescription.Replace("{hpLost}", hpLost > 0 ? hpLost.ToString() : "-");
        finalDescription = finalDescription.Replace("{aoeDamage}", aoeDamage > 0 ? aoeDamage.ToString() : "-");
        finalDescription = finalDescription.Replace("{healthSet}", healthSet > 0 ? healthSet.ToString() : "-");

        if (descriptionText) descriptionText.text = finalDescription;
        if (CardImage) CardImage.sprite = cardData.cardSprite;

        UpdateCardCoverImage();
        UpdateRarityGemImage();
    }

    private void UpdateCardCoverImage()
    {
        if (CardCoverImage == null || cardData == null) return;

        switch (cardData.cardType)
        {
            case Card.CardType.Attack: CardCoverImage.sprite = attackSprite; break;
            case Card.CardType.Guard: CardCoverImage.sprite = guardSprite; break;
            case Card.CardType.Tactic: CardCoverImage.sprite = tacticSprite; break;
            default: Debug.LogWarning($"CardDisplay: Unknown card type {cardData.cardType}"); break;
        }
    }

    private void UpdateRarityGemImage()
    {
        if (RarityGemImage == null || cardData == null) return;

        switch (cardData.cardRarity)
        {
            case Card.CardRarity.Common: RarityGemImage.sprite = commonGemSprite; break;
            case Card.CardRarity.Uncommon: RarityGemImage.sprite = uncommonGemSprite; break;
            case Card.CardRarity.Rare: RarityGemImage.sprite = rareGemSprite; break;
            case Card.CardRarity.Legendary: RarityGemImage.sprite = legendaryGemSprite; break;
            default: Debug.LogWarning($"CardDisplay: Unknown rarity {cardData.cardRarity}"); break;
        }
    }

#if UNITY_EDITOR
    // Optional: live-refresh in the Editor when the asset changes
    private void OnValidate()
    {
        if (!Application.isPlaying && cardData != null)
            UpdateCardDisplay();
    }
#endif
}
