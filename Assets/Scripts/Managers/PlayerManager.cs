using UnityEngine;
using System.Collections.Generic;
using MyProjectF.Assets.Scripts.Cards;
using MyProjectF.Assets.Scripts.Player;
using MyProjectF.Assets.Scripts.Effects;
using MyProjectF.Assets.Scripts.Managers;

/// <summary>
/// Manages player initialization, energy usage, and application of card effects.
/// Singleton for handling player-related actions in the game.
/// </summary>
public class PlayerManager : SceneSingleton<PlayerManager>
{

    [SerializeField] private GameObject playerPrefab;

    /// <summary>
    /// Initializes the player by instantiating the playerPrefab under PlayerCanvas.
    /// </summary>
    public void InitializePlayer()
    {
        if (playerPrefab == null)
        {
            Logger.LogError("PlayerManager: playerPrefab is null! Please assign it in the Inspector.", this);
            return;
        }

        GameObject playerCanvas = GameObject.Find("PlayerCanvas");
        if (playerCanvas == null)
        {
            Logger.LogError("PlayerCanvas GameObject not found in scene!", this);
            return;
        }

        GameObject playerObject = Instantiate(playerPrefab, playerCanvas.transform, false);
        if (playerObject == null)
        {
            Logger.LogError("Failed to instantiate playerPrefab!", this);
            return;
        }

        PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Logger.LogError("playerPrefab does not have a PlayerStats component.", this);
            return;
        }

        BattleManager.Instance.RegisterPlayerEvents(playerStats);

        playerStats.ResetEnergy();
        playerStats.ResetArmor();

    }

    /// <summary>
    /// Checks if the player has enough energy to play the specified card.
    /// </summary>
    public bool CanPlayCard(Card card)
    {
        int playerEnergy = PlayerStats.Instance.energy;
        bool canPlay = playerEnergy >= card.energyCost;

        return canPlay;
    }

    /// <summary>
    /// Uses energy when a card is played.
    /// </summary>
    public void UseCard(Card card)
    {
        if (!CanPlayCard(card))
        {
            Logger.LogWarning("⚠️ Not enough energy to play this card.", this);
            return;
        }

        PlayerStats.Instance.UseEnergy(card.energyCost);;
    }

    /// <summary>
    /// Applies the effect of a card to a target (enemy, self, or allies).
    /// </summary>
    public void ApplyCardEffect(Enemy targetEnemy, EffectData effect, Card card)
    {
        if (effect == null)
        {
            Logger.LogError("❌ EffectData is null.", this);
            return;
        }

        var caster = PlayerStats.Instance;

        switch (card.targetType)
        {
            case Card.TargetType.SingleEnemy:
                if (targetEnemy != null)
                {
                    effect.ApplyEffect(caster, targetEnemy);
                    Logger.Log($"🎯 Applied '{card.cardName}' to enemy '{targetEnemy.name}'.", this);
                }
                else
                {
                    Logger.LogError("Card requires an enemy target, but none was provided.", this);
                }
                break;

            case Card.TargetType.AllEnemies:
                foreach (Enemy enemy in EnemyManager.Instance.GetActiveEnemies())
                {
                    effect.ApplyEffect(caster, enemy);
                }
                Logger.Log($"🌪️ Applied '{card.cardName}' to all enemies.", this);
                break;

            case Card.TargetType.Self:
                effect.ApplyEffect(caster, caster);
                Logger.Log($"🧘 Applied '{card.cardName}' to self.", this);
                break;

            case Card.TargetType.AllAllies:
                foreach (PlayerStats ally in GetAllies())
                {
                    effect.ApplyEffect(caster, ally);
                }
                Logger.Log($"🤝 Applied '{card.cardName}' to all allies.", this);
                break;

            default:
                Logger.LogWarning("❓ Unknown card target type.", this);
                break;
        }
    }

    /// <summary>
    /// Returns a list of all ally PlayerStats. Currently includes only the main player.
    /// </summary>
    public List<PlayerStats> GetAllies()
    {
        List<PlayerStats> allies = new();

        if (PlayerStats.Instance != null)
        {
            allies.Add(PlayerStats.Instance);
        }

        // Add more allies here if applicable in the future

        return allies;
    }
}
