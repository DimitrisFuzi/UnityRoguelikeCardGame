/// <summary>
/// Contract for enemy AI: execution and next-intent prediction.
/// </summary>
public interface IEnemyAI
{
    /// <summary>Called when it's the enemy's turn to act.</summary>
    void ExecuteTurn();

    void SetPlayerStats(CharacterStats player);
    void SetIntentIcons(UnityEngine.Sprite attack, UnityEngine.Sprite buff);
    void InitializeAI();

    /// <summary>Return the locked next-turn intent (do not recompute).</summary>
    EnemyIntent PredictNextIntent();

    EnemyIntent GetCurrentIntent();
    void SetEnemyDisplay(EnemyDisplay display);
}
