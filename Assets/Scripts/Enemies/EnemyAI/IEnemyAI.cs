// IEnemyAI.cs
/// <summary>
/// Interface that defines the basic behavior of any enemy AI.
/// Allows for turn-based decision making and intent prediction.
/// </summary>
public interface IEnemyAI
{
    /// <summary>
    /// Called when it's the enemy's turn to act.
    /// </summary>
    void ExecuteTurn();

    

    void SetPlayerStats(CharacterStats player);

    void SetIntentIcons(UnityEngine.Sprite attack, UnityEngine.Sprite buff);
    void InitializeAI();
    EnemyIntent PredictNextIntent();
    EnemyIntent GetCurrentIntent();
    void SetEnemyDisplay(EnemyDisplay display);
}