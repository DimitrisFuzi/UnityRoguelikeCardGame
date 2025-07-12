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
    public Image CardTypeImage;

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
                else if(effect is DrawCardEffect drawCardEffect)
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
        }
    }
}
