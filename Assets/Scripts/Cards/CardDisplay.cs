using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Effects; // ✅ For accessing card effects like DamageEffect or ArmorEffect

/// <summary>
/// Responsible for updating the visual representation of a card on the UI.
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
    public Image CardCoverImage; // Image for card type (Attack, Guard, Tactic)
    public Image RarityGemImage; // Image for rarity gem (Common, Uncommon, etc.)

    [Header("Dynamic Sprites")]
    public Sprite attackSprite;
    public Sprite guardSprite;
    public Sprite tacticSprite;

    public Sprite commonGemSprite;
    public Sprite uncommonGemSprite;
    public Sprite rareGemSprite;
    public Sprite legendaryGemSprite;

    void Start()
    {
        // Show error if cardData is not assigned
        if (cardData == null)
        {
            Debug.LogError($"❌ cardData is NULL on CardDisplay ({gameObject.name}) on start!");
        }
        else
        {
            // Populate the UI with card information
            UpdateCardDisplay();
        }
    }

    /// <summary>
    /// Updates all card-related UI fields with current data from cardData.
    /// </summary>
    public void UpdateCardDisplay()
    {
        if (cardData != null)
        {
            // Basic UI data
            nameText.text = cardData.cardName;
            manaCostText.text = cardData.energyCost.ToString();

            // Prepare to fill placeholders like {damage} or {armor}
            int damage = 0;
            int armor = 0;
            int cards = 0;
            int energy = 0;
            int hpLost = 0;
            int aoeDamage = 0;
            int healthSet = 0;

            // ✅ Read and parse card effects to extract values
            foreach (EffectData effect in cardData.GetCardEffects())
            {
                if (effect is DamageEffect damageEffect)
                {
                    damage = damageEffect.damageAmount;
                }
                else if (effect is ArmorEffect armorEffect)
                {
                    armor = armorEffect.armorAmount;
                }
                else if (effect is DrawCardEffect drawCardEffect)
                {
                    cards = drawCardEffect.cardsToDraw;
                }
                else if (effect is GainEnergyEffect gainEnergyEffect)
                {
                    energy = gainEnergyEffect.energyAmount;
                }
                else if (effect is LoseHealthEffect loseHealthEffect)
                {
                    hpLost = loseHealthEffect.healthLoss;
                }
                else if (effect is AOEDamageEffect aoeDamageEffect)
                {
                    aoeDamage = aoeDamageEffect.damageAmount;
                }
                else if (effect is SetHealthEffect setHealthEffect)
                {
                    healthSet = setHealthEffect.newHealth;
                }
            }

            // ✅ Replace placeholders in the description with actual values
            string finalDescription = cardData.cardDescription;
            finalDescription = finalDescription.Replace("{damage}", damage > 0 ? damage.ToString() : "-");
            finalDescription = finalDescription.Replace("{armor}", armor > 0 ? armor.ToString() : "-");
            finalDescription = finalDescription.Replace("{cards}", cards > 0 ? cards.ToString() : "-");
            finalDescription = finalDescription.Replace("{energy}", energy > 0 ? energy.ToString() : "-");
            finalDescription = finalDescription.Replace("{hpLost}", hpLost > 0 ? hpLost.ToString() : "-");
            finalDescription = finalDescription.Replace("{aoeDamage}", aoeDamage > 0 ? aoeDamage.ToString() : "-");
            finalDescription = finalDescription.Replace("{healthSet}", healthSet > 0 ? healthSet.ToString() : "-");

            // Update UI elements with card data
            descriptionText.text = finalDescription;
            CardImage.sprite = cardData.cardSprite;

            // Dynamically assign the correct sprites for card type and rarity
            UpdateCardCoverImage();
            UpdateRarityGemImage();
        }
    }

    /// <summary>
    /// Updates the card type image based on the card's type.
    /// </summary>
    private void UpdateCardCoverImage()
    {
        if (CardCoverImage == null) return;

        switch (cardData.cardType)
        {
            case Card.CardType.Attack:
                CardCoverImage.sprite = attackSprite;
                break;
            case Card.CardType.Guard:
                CardCoverImage.sprite = guardSprite;
                break;
            case Card.CardType.Tactic:
                CardCoverImage.sprite = tacticSprite;
                break;
            default:
                Debug.LogWarning($"❓ Unknown card type: {cardData.cardType}");
                break;
        }
    }

    /// <summary>
    /// Updates the rarity gem image based on the card's rarity.
    /// </summary>
    private void UpdateRarityGemImage()
    {
        if (RarityGemImage == null) return;

        switch (cardData.cardRarity)
        {
            case Card.CardRarity.Common:
                RarityGemImage.sprite = commonGemSprite;
                break;
            case Card.CardRarity.Uncommon:
                RarityGemImage.sprite = uncommonGemSprite;
                break;
            case Card.CardRarity.Rare:
                RarityGemImage.sprite = rareGemSprite;
                break;
            case Card.CardRarity.Legendary:
                RarityGemImage.sprite = legendaryGemSprite;
                break;
            default:
                Debug.LogWarning($"❓ Unknown card rarity: {cardData.cardRarity}");
                break;
        }
    }
}