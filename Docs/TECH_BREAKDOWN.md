# TECH BREAKDOWN — Turn-Based Deckbuilder Prototype (Unity)

This document summarizes the gameplay architecture and implementation decisions for this prototype.  
Focus: **turn flow**, **data-driven cards/effects**, **enemy intent telegraphing**, and **reward selection**.

---

## Project entry points (scenes)
- `Assets/Scenes/BattleBoss1`
  - Used to showcase enemy intent variety + boss behavior.
  - Note: boss victory leads to the Victory scene (no reward in this flow).
- `Assets/Scenes/Battle1`
  - Battle flow that transitions into the Reward scene.

---

## High-level architecture (folders)
Primary code lives under `Assets/Scripts/`:

- `Assets/Scripts/Managers/`
  - Orchestrates battle state, turns, deck/hand, enemy actions, and scene flow.
- `Assets/Scripts/Battle/`
  - Gameplay domain code: cards, effects, enemies, player stats.
- `Assets/Scripts/Reward/`
  - Post-battle reward scene: roll card choices, display, select, add to deck.

---

## Turn flow & input gating
### Battle state ownership
- `Assets/Scripts/Managers/BattleManager.cs`
  - Tracks battle state (`START`, `PLAYER_TURN`, `ENEMY_TURN`, `WON`, `LOST`)
  - Locks/unlocks player input through `IsPlayerInputLocked`
  - On victory: calls `SceneFlowManager.Instance.LoadNextAfterBattle()`

### Turn sequencing
- `Assets/Scripts/Managers/TurnManager.cs`
  - `StartPlayerTurn()`
    - Resets player energy/armor
    - Unlocks input
    - Triggers draw via `HandManager.Instance.DrawCardsForTurn()`
  - `EndPlayerTurn() → EndPlayerTurnRoutine()`
    - Locks input immediately
    - Waits for drawing to finish (`HandManager.IsDrawing`)
    - Discards the hand (`HandManager.DiscardHandRoutine(animated: true)`)
    - Runs enemy actions (`EnemyManager.PerformEnemyActionsCoroutine()`)
    - Predicts and displays next intents for enemies (`PredictNextIntent()`)

### Hand draw/discard
- `Assets/Scripts/Managers/HandManager.cs`
  - Draws cards asynchronously, sets `IsDrawing` during operations
  - Maintains hand layout (fan) and discards with animation
  - Discard behavior:
    - End turn discards unplayed cards (even if they are normally “exhaust after use”)

---

## Card system (data → interaction → resolution)
### Card data
- `Assets/Scripts/Battle/Cards/Card.cs`
  - Cards are `ScriptableObject` assets:
    - cost (`energyCost`), rarity, sprite, target type, exhaust flag
  - Effects are stored as:
    - `[SerializeReference] List<EffectData> effects`
  - This supports a data-driven pipeline with multiple effect types per card.

### Card interaction + targeting (UI)
- `Assets/Scripts/Battle/Cards/CardMovement.cs`
  - Responsible for card hover, drag, and “play zone” logic
  - Validates input through `Blocked()`:
    - `BattleManager.IsPlayerInputLocked`
    - `TurnManager.IsPlayerTurn`
    - `HandManager.IsDrawing`
    - `isInHand`
  - For `SingleEnemy` targeting:
    - Uses UI raycast to detect `Enemy` under cursor (`GetEnemyUnderCursor()`)

### Card play + effect execution
- `CardMovement.ApplyEffectsInSequence()` performs:
  1) Spend energy (`PlayerManager.UseCard(cardData)`)
  2) Remove card from hand (`HandManager.RemoveCardFromHand(...)`)
  3) Resolve each `EffectData` in order
     - If effect implements `ICoroutineEffect`, it runs as a coroutine

### Energy usage + target routing
- `Assets/Scripts/Managers/PlayerManager.cs`
  - `CanPlayCard(Card)` checks current energy
  - `UseCard(Card)` consumes energy
  - `ApplyCardEffect(...)` contains routing logic for targets:
    - `SingleEnemy`, `AllEnemies`, `Self`, `AllAllies`

---

## Effects pipeline
- `Assets/Scripts/Battle/Effects/EffectData.cs`
  - Base type for effects used in cards (polymorphic via `SerializeReference`)
- `Assets/Scripts/Battle/Effects/ICoroutineEffect.cs`
  - Optional coroutine contract for effects that need sequencing / timing

---

## Enemy AI & intent telegraphing
### Enemy initialization + AI attachment
- `Assets/Scripts/Battle/Enemies/Enemy.cs`
  - `InitializeEnemy(EnemyData, EnemyDisplay)`:
    - initializes stats + UI
    - attaches AI component based on `EnemyAIType` enum (`AttachAI(...)`)
    - wires intent icon sprites into AI via `SetIntentIcons(...)`
    - calls `InitializeAI()` and `UpdateIntentDisplay()`

### AI contract + intent types
- `Assets/Scripts/Battle/Enemies/EnemyAI/IEnemyAI.cs`
  - `ExecuteTurn()`, `PredictNextIntent()`, `GetCurrentIntent()`
- `Assets/Scripts/Battle/Enemies/EnemyAI/EnemyIntent.cs`
  - intent payload shown to the player (type, description, value, icon)

### Intent UI
- `Assets/Scripts/Battle/Enemies/EnemyAI/IntentDisplay.cs`
  - updates icon + text based on `EnemyIntent`

### When intents update
- On enemy action:
  - `Enemy.PerformAction()` executes AI, then updates the displayed next intent.
- At end of enemy turn:
  - `TurnManager` iterates enemies and sets next intent display using `PredictNextIntent()`.

---

## Reward flow (Battle1 → Reward scene)
- `Assets/Scripts/Reward/RewardSceneController.cs`
  - Ensures reward pool is populated (`RewardPool.PopulateFromDatabase()`)
  - Rolls 3 card choices (`pool.RollCardChoices(3, seed)`)
  - Spawns `RewardCardView` entries
  - On selection:
    - disables other options
    - animates selected card
    - adds card to deck (`PlayerDeck.AddCardToDeck(cardName)`)
- `Assets/Scripts/Reward/RewardCardView.cs`
  - Builds a clickable reward card UI using an existing `CardDisplay` thumbnail prefab

---

## Known limitations (prototype tradeoffs)
- Card interaction and card resolution are currently coupled:
  - `CardMovement` handles both UI interaction and effect resolution orchestration.
  - This made iteration faster, but a next step would be extracting a dedicated “card resolver” service to decouple UI from rules.

---

## If I continued (next refactor targets)
- Separate card-resolution logic from UI (`CardMovement`) into a gameplay service (e.g., `CardPlayResolver`)
- Add a lightweight debug menu to spawn specific cards/enemies for quick validation
- Add small automated validation hooks (e.g., sanity checks for EffectData configuration)