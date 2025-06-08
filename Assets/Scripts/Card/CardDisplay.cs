using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Effects; // ✅ Προσθήκη για πρόσβαση στο EffectData

public class CardDisplay : MonoBehaviour
{
    public Card cardData;
    public Image CardImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text manaCostText;
    public Image CardTypeImage;

    void Start()
    {
        if (cardData == null)
        {
            Debug.LogError($"❌ cardData is NULL on CardDisplay ({gameObject.name}) on start!");
        }
        else
        {
            UpdateCardDisplay();
        }
    }

    public void UpdateCardDisplay()
    {
        if (cardData != null)
        {
            nameText.text = cardData.cardName;
            manaCostText.text = cardData.energyCost.ToString();

            int damage = 0;
            int armor = 0;

            // ✅ Ανάγνωση των effects της κάρτας
            foreach (EffectData effect in cardData.GetCardEffects())
            {
                if (effect is DamageEffect damageEffect)
                {
                    damage = damageEffect.damageAmount; // ✅ Παίρνουμε το damage αν υπάρχει
                }
                else if (effect is ArmorEffect armorEffect)
                {
                    armor = armorEffect.armorAmount; // ✅ Παίρνουμε το armor αν υπάρχει
                }
            }

            // ✅ Αντικατάσταση των placeholder values στο description
            string finalDescription = cardData.cardDescription;
            finalDescription = finalDescription.Replace("{damage}", damage > 0 ? damage.ToString() : "-");
            finalDescription = finalDescription.Replace("{armor}", armor > 0 ? armor.ToString() : "-");

            descriptionText.text = finalDescription;
            CardImage.sprite = cardData.cardSprite;
            CardTypeImage.sprite = cardData.cardTypeSprite;
        }
    }
}
