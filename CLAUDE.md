# KoKo Krunch - Strawberry Catch & Win

Unity 2D arcade game (portrait 1080x1920) built with Unity 2022+ and URP 17.3.0.

## Quick Reference

- **Engine:** Unity 2022+ with URP 17.3.0
- **Input System:** New Input System (`com.unity.inputsystem` 1.18.0) - NEVER use old `UnityEngine.Input`
- **Target:** Standalone (Mac/Windows), Android, iOS
- **Resolution:** Portrait 1080x1920 (reference 450x800 in CanvasScaler)

## Project Structure

```
Assets/
  Scripts/
    Data/          # ScriptableObjects & data models (GameConfig, PlayerData, RuntimeGameSettings)
    Gameplay/      # Core mechanics (PlayerCatcher, ItemSpawner, FallingItem)
    Managers/      # Persistent singletons (GameManager, DataManager, AudioManager, SettingsManager, ScreensaverManager, AdminExitManager)
    UI/            # Per-scene UI controllers (LandingUI, NameInputUI, InstructionUI, GameUI, ResultUI, LeaderboardUI, AdminUI)
    Utils/         # Utilities (SceneLoader, ScreenSetup, PlaceholderSpriteGenerator, WindowsTouchKeyboard)
  Scenes/Game/     # 7 scenes: Landing, NameInput, Instruction, Game, Result, Leaderboard, Admin
  Prefabs/
    Items/         # Strawberry, KokoKrunchPack1, KokoKrunchPack2
    Player/        # Catcher
    UI/            # LeaderboardEntryRow, AdminTableRow
  Images/          # All game image assets (PNG)
  Fonts/           # Custom fonts + SDF assets (BebasNeue-Bold, CCWhatchamacallit, KitchenCupboard)
  Audio/           # BGM/ and SFX/ subdirectories
  Editor/          # SceneSetupEditor (automated project setup tool)
  Resources/       # GameConfig.asset (loaded at runtime)
```

## Architecture

- **Bootstrap:** `PersistentManagersBootstrap` uses `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` to create all managers as DontDestroyOnLoad
- **Events:** GameManager fires `OnScoreChanged`, `OnLivesChanged`, `OnTimeChanged`, `OnGameOver`
- **Data:** JSON persistence at `Application.persistentDataPath` (leaderboard.json, game_settings.json)
- **Namespaces:** `KoKoKrunch.Data`, `KoKoKrunch.Gameplay`, `KoKoKrunch.Managers`, `KoKoKrunch.UI`, `KoKoKrunch.Utils`

## Rules

See `.claude/rules/` for detailed coding and project rules.
