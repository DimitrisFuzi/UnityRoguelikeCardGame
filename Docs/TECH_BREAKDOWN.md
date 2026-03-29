# TECH BREAKDOWN — Turn-Based Deckbuilder Prototype (Unity)

This document summarizes the gameplay architecture and implementation for this prototype.
Focus: **turn flow**, **data-driven cards/effects**, **enemy intent telegraphing**, and **reward selection**.

---

## Project entry points (scenes)
- `Assets/Scenes/BattleBoss1`
  - Boss fight used to showcase intent variety.
  - Victory leads to the Victory scene (no reward in this boss flow).
- `Assets/Scenes/Battle1`
  - Battle flow that transitions into the Reward scene.

---

## Architecture overview (folders)
Primary code lives under `Assets/Scripts/`:

- `Assets/Scripts/Managers/`
  - Battle state + turn sequencing + hand/deck flow.
- `Assets/Scripts/Battle/`
  - Cards, effects, enemies, player stats.
- `Assets/Scripts/Reward/`
  - Reward choice generation + UI + “add to deck”.

Pattern notes:
- Scene-level singletons are used for orchestration (`SceneSingleton<T>`).
- Content is data-driven via ScriptableObjects for cards and enemies.

---

## Battle flow & input gating
### Battle state
- `Assets/Scripts/Managers/BattleManager.cs`
  - Owns battle state (`START`, `PLAYER_TURN`, `ENEMY_TURN`, `WON`, `LOST`)
  - Locks/unlocks input (`IsPlayerInputLocked`)
  - Victory triggers `SceneFlowManager.Instance.LoadNextAfterBattle()`

### Turn sequencing
- `Assets/Scripts/Managers/TurnManager.cs`
  - `StartPlayerTurn()`
    - Resets energy/armor
    - Unlocks input
    - Starts draw (`HandManager.DrawCardsForTurn()`)
  - `EndPlayerTurnRoutine()`
    - Locks input immediately
    - Waits for draw to finish (`HandManager.IsDrawing`)
    - Discards hand (`HandManager.DiscardHandRoutine(animated: true)`)
    - Runs enemy actions coroutine (`EnemyManager.PerformEnemyActionsCoroutine()`)
    - Predicts and displays next intents (`PredictNextIntent()`)

### Hand layout + draw/discard
- `Assets/Scripts/Managers/HandManager.cs`
  - Tracks `IsDrawing` during draw animations
  - Updates a fan layout and saves original transforms (`CardMovement.SaveOriginalTransform()`)
  - End-turn discards unplayed cards with animation

---

## Cards: data → interaction → resolution
### Card data (ScriptableObject)
- `Assets/Scripts/Battle/Cards/Card.cs`
  - Stores cost/rarity/sprite/targeting
  - Polymorphic effects list:
    - `[SerializeReference] List<EffectData> effects`

### Card interaction + targeting
- `Assets/Scripts/Battle/Cards/CardMovement.cs`
  - Hover/drag/play-zone logic + DOTween feedback
  - Blocks interaction if:
    - input locked (`BattleManager.IsPlayerInputLocked`)
    - not player turn (`TurnManager.IsPlayerTurn == false`)
    - drawing (`HandManager.IsDrawing`)
    - card not in hand (`isInHand == false`)
  - For single-enemy targeting:
    - UI raycast under cursor (`GetEnemyUnderCursor()`)

### Resolution order (when a card is played)
- `CardMovement.ApplyEffectsInSequence()`
  1) Spend energy via `PlayerManager.UseCard(cardData)`
  2) Remove card from hand (`HandManager.RemoveCardFromHand(...)`)
  3) Apply each `EffectData` in order
     - If effect implements `ICoroutineEffect`, runs a coroutine effect routine

### Energy + target routing
- `Assets/Scripts/Managers/PlayerManager.cs`
  - `CanPlayCard()` / `UseCard()` for energy validation and spending
  - `ApplyCardEffect(...)` contains routing for:
    - `SingleEnemy`, `AllEnemies`, `Self`, `AllAllies`

---

## Effects pipeline
- `Assets/Scripts/Battle/Effects/EffectData.cs`
  - Base class for effect implementations used in `Card.effects`
- `Assets/Scripts/Battle/Effects/ICoroutineEffect.cs`
  - Optional interface for effects that need timed sequencing

---

## Enemies: AI + intent telegraphing
### Enemy data
- `Assets/Scripts/Battle/Enemies/EnemyData.cs`
  - ScriptableObject holding stats, visuals, AI type (`EnemyAIType`), and intent icons

### Runtime enemy + AI attachment
- `Assets/Scripts/Battle/Enemies/Enemy.cs`
  - `InitializeEnemy(...)` sets stats/UI and attaches AI based on `EnemyAIType`
  - AI is attached as a component and exposed via `IEnemyAI EnemyAI`
  - Intent update points:
    - On initialization (`UpdateIntentDisplay()`)
    - After executing an enemy action (`PerformAction()` calls `ExecuteTurn()` then `UpdateIntentDisplay()`)

### Intent UI update
- `EnemyDisplay.SetIntent(EnemyIntent ...)` (called from `Enemy.cs` and `TurnManager`)

---

## Reward flow (Battle1 → Reward scene)
- `Assets/Scripts/Reward/RewardSceneController.cs`
  - Populates reward pool if empty (`RewardPool.PopulateFromDatabase()`)
  - Rolls 3 choices (`pool.RollCardChoices(3, seed)`)
  - Spawns `RewardCardView` entries
  - On selection:
    - disables other choices
    - animates selected card
    - adds card to deck via `PlayerDeck.AddCardToDeck(cardName)`
    - continues via `SceneFlowManager.LoadNextAfterBattle()` (fallback to Victory)

- `Assets/Scripts/Reward/RewardCardView.cs`
  - Builds a clickable card choice and binds data into an existing `CardDisplay` thumbnail

---

## Known limitations (prototype tradeoffs)
- Card UI + card-resolution orchestration are currently coupled:
  - `CardMovement` handles both interaction and sequencing (`ApplyEffectsInSequence()`).
  - This reduced moving parts during iteration, but a scalable next step would be moving resolution into a dedicated gameplay service (e.g., a `CardPlayResolver`).

---

## If I continued (next refactor targets)
- Decouple card UI from rules: move effect application out of `CardMovement`
- Add debug hooks to spawn specific cards/enemies for quick verification
- Add simple validation checks for `EffectData` configs (nulls, missing targets, etc.)
