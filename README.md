# KoKo Krunch - Strawberry Catch & Win

A casual arcade-style 2D game built in Unity where players catch falling strawberries and KoKo Krunch cereal boxes to earn points. Features a full game flow, leaderboard tracking, admin panel, screensaver, and secure exit mechanism.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Project Structure](#project-structure)
- [Scenes](#scenes)
- [Scripts & Systems](#scripts--systems)
- [Game Configuration](#game-configuration)
- [Architecture & Bootstrap](#architecture--bootstrap)
- [Features](#features)
- [Dependencies](#dependencies)
- [Setup Instructions](#setup-instructions)
- [Game Rules & Mechanics](#game-rules--mechanics)
- [Audio System](#audio-system)
- [Leaderboard System](#leaderboard-system)
- [Editor Tools](#editor-tools)
- [Build & Deployment](#build--deployment)
- [Troubleshooting](#troubleshooting)

---

## Project Overview

| | |
|---|---|
| **Product Name** | KoKo Krunch |
| **Unity Version** | 2022+ with URP 17.3.0 |
| **Rendering** | Universal Render Pipeline (2D) |
| **Target Platforms** | Standalone (Mac/Windows), Android, iOS |
| **Default Resolution** | 1080x1920 (portrait) |

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Data/                   # Game config and data models
│   │   ├── GameConfig.cs       # ScriptableObject with game parameters
│   │   ├── PlayerData.cs       # Leaderboard entry data structure
│   │   └── RuntimeGameSettings.cs # Runtime-modifiable settings (JSON)
│   ├── Managers/               # Persistent singleton managers
│   │   ├── PersistentManagersBootstrap.cs # Auto-initialization at startup
│   │   ├── GameManager.cs      # Game state, score, lives, timer
│   │   ├── DataManager.cs      # Leaderboard persistence (JSON/CSV)
│   │   ├── AudioManager.cs     # BGM and SFX playback
│   │   ├── SettingsManager.cs  # Runtime settings persistence
│   │   ├── ScreensaverManager.cs # Inactivity timeout screensaver
│   │   └── AdminExitManager.cs # Secure password-protected exit
│   ├── Gameplay/               # Game mechanics
│   │   ├── PlayerCatcher.cs    # Player input and movement
│   │   ├── ItemSpawner.cs      # Item spawning with difficulty scaling
│   │   └── FallingItem.cs      # Individual falling item behavior
│   ├── UI/                     # Scene UI controllers
│   │   ├── LandingUI.cs        # Start screen with hidden admin access
│   │   ├── NameInputUI.cs      # Player name entry
│   │   ├── InstructionUI.cs    # Game instructions display
│   │   ├── GameUI.cs           # In-game HUD (score, timer, lives)
│   │   ├── ResultUI.cs         # Game over results screen
│   │   ├── LeaderboardUI.cs    # Leaderboard display
│   │   ├── LeaderboardEntry.cs # Individual leaderboard row
│   │   └── AdminUI.cs          # Admin panel (tabs: Leaderboard & Settings)
│   └── Utils/                  # Utilities
│       ├── SceneLoader.cs      # Static scene navigation helpers
│       ├── ScreenSetup.cs      # Screen resolution configuration
│       ├── PlaceholderSpriteGenerator.cs # Runtime sprite generation
│       └── WindowsTouchKeyboard.cs # Windows touch keyboard support
├── Scenes/Game/                # 7 game scenes
├── Prefabs/
│   ├── Items/                  # Strawberry, KokoKrunchPack1, KokoKrunchPack2
│   ├── Player/                 # Catcher prefab
│   └── UI/                     # LeaderboardEntryRow, AdminTableRow
├── Resources/
│   └── GameConfig.asset        # Default game configuration
├── Audio/
│   ├── BGM/                    # Background music
│   └── SFX/                    # Sound effects
├── Images/                     # Game images
├── Sprites/Placeholder/        # Placeholder sprites
├── Fonts/                      # TextMesh Pro font assets
├── Settings/                   # URP pipeline settings
└── Editor/
    └── SceneSetupEditor.cs     # Automated scene & prefab generation tool
```

---

## Scenes

| # | Scene | Description |
|---|-------|-------------|
| 1 | **LandingScene** | Main menu with START button. Hidden admin access (10 taps on logo within 3s). |
| 2 | **NameInputScene** | Player name entry with validation. |
| 3 | **InstructionScene** | Game rules and scoring info. |
| 4 | **GameScene** | Main gameplay with HUD (score, timer, lives). |
| 5 | **ResultScene** | Final score display with Leaderboard / Play Again options. |
| 6 | **LeaderboardScene** | Top 20 scores with #1 player highlight. |
| 7 | **AdminScene** | Tabbed admin panel (Leaderboard data & Game Settings). |

---

## Scripts & Systems

### Data Layer

| Script | Purpose |
|--------|---------|
| **GameConfig** | ScriptableObject defining all default game parameters. Loaded from `Resources/`. |
| **RuntimeGameSettings** | Serializable mirror of GameConfig for runtime JSON persistence. Supports `FromConfig()` and `ApplyTo()` conversion. |
| **PlayerData** | Leaderboard entry: `playerName`, `score`, `timestamp`. |

### Managers

| Manager | Purpose |
|---------|---------|
| **GameManager** | Central game state. Tracks score, lives, timer. Fires events: `OnScoreChanged`, `OnLivesChanged`, `OnTimeChanged`, `OnGameOver`. |
| **DataManager** | Leaderboard CRUD. Saves/loads `leaderboard.json`. Supports CSV export and bulk clear. |
| **AudioManager** | Dual AudioSource system (BGM loop + SFX one-shot). Context-aware playback for menu/game. |
| **SettingsManager** | Persists runtime game settings to `game_settings.json`. Supports save, load, reset to defaults, and apply to active config. |
| **ScreensaverManager** | Activates after 40s of inactivity. Displays dark overlay. Touch dismisses and returns to Landing. Disabled during GameScene and AdminScene. |
| **AdminExitManager** | Tap top-right corner 5 times within 3s to trigger password popup. Password `kokokrushexit` exits the application. |

### Gameplay

| Script | Purpose |
|--------|---------|
| **PlayerCatcher** | Handles touch, mouse, and keyboard input. Moves catcher within screen bounds. Detects item catches via `OnTriggerEnter2D`. |
| **ItemSpawner** | Spawns falling items at random X positions. Progressively increases spawn rate and fall speed. |
| **FallingItem** | Falls at configurable speed. Calls `GameManager.AddScore()` on catch or `GameManager.LoseLife()` on miss. Self-destructs after either event. |

### UI Controllers

| Script | Purpose |
|--------|---------|
| **LandingUI** | START button + hidden admin button (10 taps in 3s). |
| **NameInputUI** | Name validation, Windows touch keyboard support. |
| **InstructionUI** | Static rules display with NEXT button. |
| **GameUI** | Live HUD subscribed to GameManager events. Triggers scene transition on game over. |
| **ResultUI** | Final score + navigation buttons (Leaderboard / Play Again). |
| **LeaderboardUI** | Scrollable leaderboard with top player highlight. |
| **AdminUI** | Two tabs: **Leaderboard** (table, export CSV, clear data) and **Settings** (11 editable parameters, save, reset). All UI elements are scene-based with SerializeField references. |

---

## Game Configuration

### Default Values

```
Scoring:
  Strawberry Points:       5
  KoKo Krunch Points:      10

Difficulty:
  Item Fall Speed:         4.0
  Max Fall Speed:          8.0
  Fall Speed Increase:     0.1 per spawn

Spawning:
  Initial Spawn Interval:  1.2s
  Minimum Spawn Interval:  0.5s
  Spawn Acceleration:      0.02s per spawn

Game Rules:
  Game Duration:           30s
  Max Lives:               3
  Max Leaderboard Entries: 20

Player:
  Move Speed:              8.0
```

### Configuration Flow

1. **GameConfig.asset** provides baseline defaults (loaded from `Resources/`)
2. **SettingsManager** loads any saved JSON overrides from `persistentDataPath`
3. **Admin Panel** allows runtime modification and persistence
4. **GameManager.StartGame()** applies active settings to gameplay

---

## Architecture & Bootstrap

### Auto-Initialization

All managers are created automatically before the first scene loads via `PersistentManagersBootstrap`:

```
[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]
├── GameManager (+ GameConfig from Resources)
├── DataManager
├── AudioManager (+ BGM/SFX AudioSources)
├── SettingsManager
├── ScreensaverManager
└── AdminExitManager
```

All managers live on a single `DontDestroyOnLoad` GameObject (`--- Managers ---`) and are accessible via static `Instance` properties.

### Event-Driven Communication

```
GameManager events → UI Controllers
  ├── OnScoreChanged  → GameUI (score text)
  ├── OnLivesChanged  → GameUI (heart icons)
  ├── OnTimeChanged   → GameUI (timer text)
  └── OnGameOver      → GameUI (scene transition)
```

### Data Persistence

| Data | File | Location |
|------|------|----------|
| Leaderboard | `leaderboard.json` | `Application.persistentDataPath` |
| Settings | `game_settings.json` | `Application.persistentDataPath` |
| CSV Export | `leaderboard_export.csv` | `Application.persistentDataPath` |

---

## Features

### Screensaver

- Triggers after **40 seconds** of no user input (touch, mouse, keyboard)
- Displays a fullscreen dark overlay with "Touch to continue"
- Any input dismisses the screensaver and returns to **Landing Page**
- Disabled during **GameScene** and **AdminScene**
- Sorting order 999 (renders above all other UI)

### Admin Exit (Password Protected)

- Tap the **top-right corner** of the screen (15% region) **5 times within 3 seconds**
- A password popup appears with a masked input field
- Enter password: **`kokokrushexit`**
- Correct password exits the application
- Cancel button dismisses the popup
- Incorrect password shows an error message

### Admin Panel (Hidden Access)

- From Landing Page: tap the logo area **10 times within 3 seconds**
- **Leaderboard Tab**: view all entries, export to CSV, clear all data
- **Settings Tab**: edit all 11 game parameters, save, or reset to defaults

### Difficulty Scaling

Items spawn faster and fall quicker as the game progresses:
- Spawn interval: 1.2s → 0.5s (decreases by 0.02s per spawn)
- Fall speed: 4.0 → 8.0 (increases by 0.1 per spawn)

### Item Distribution

| Item | Points | Drop Rate |
|------|--------|-----------|
| Strawberry | +5 | 40% |
| KoKo Krunch Pack 1 | +10 | 30% |
| KoKo Krunch Pack 2 | +10 | 30% |

---

## Dependencies

### Unity Packages

| Package | Version |
|---------|---------|
| Input System | 1.18.0 |
| Universal RP | 17.3.0 |
| 2D Animation | 13.0.4 |
| 2D Sprite | 1.0.0 |
| 2D SpriteShape | 13.0.0 |
| 2D Tilemap | 1.0.0 |
| TextMesh Pro | (built-in) |
| Timeline | 1.8.10 |

---

## Setup Instructions

### Automated Setup (Recommended)

1. Open the project in Unity Editor (2022+)
2. Go to **KoKo Krunch > Setup All Scenes and Prefabs** from the menu bar
3. Confirm the dialog
4. Wait for completion
5. Open `Assets/Scenes/Game/LandingScene.unity`
6. Press **Play** to test

### Manual Setup

1. Ensure `Assets/Resources/GameConfig.asset` exists (ScriptableObject)
2. Create prefabs in `Assets/Prefabs/` (Items, Player, UI)
3. Build all 7 scenes with proper UI hierarchy
4. Add all scenes to **File > Build Settings** in order:
   - LandingScene, NameInputScene, InstructionScene, GameScene, ResultScene, LeaderboardScene, AdminScene

---

## Game Rules & Mechanics

### Gameplay Flow

```
Landing → Name Input → Instructions → Game (30s) → Results → Leaderboard
   ↑                                                    │
   └────────────────── Play Again ──────────────────────┘
```

### Controls

| Platform | Control |
|----------|---------|
| **Touch** | Tap and drag to move catcher |
| **Mouse** | Click and drag to move catcher |
| **Keyboard** | Arrow keys or A/D to move left/right |

### Game Over Conditions

- Timer reaches 0 seconds
- Lives reach 0 (miss 3 items)

---

## Audio System

| Type | Source | Behavior |
|------|--------|----------|
| **Menu BGM** | `bgmSource` | Loops on Landing/Result scenes |
| **Game BGM** | `bgmSource` | Loops during gameplay |
| **Catch SFX** | `sfxSource` | One-shot on item catch |
| **Miss SFX** | `sfxSource` | One-shot on item miss |
| **Button SFX** | `sfxSource` | One-shot on UI button click |
| **Game Over SFX** | `sfxSource` | One-shot when game ends |

---

## Leaderboard System

- **Storage**: JSON file at `Application.persistentDataPath/leaderboard.json`
- **Auto-save**: Score saved automatically after each game
- **Display**: Top 20 entries shown in LeaderboardScene
- **Admin**: View all entries, export to CSV, clear all data
- **Sorting**: Descending by score

---

## Editor Tools

### SceneSetupEditor

**Menu**: KoKo Krunch > Setup All Scenes and Prefabs

Automatically generates:
- All required directories
- GameConfig ScriptableObject
- Placeholder sprites (colored rectangles)
- All prefabs (items, player, UI rows)
- All 7 scenes with complete UI hierarchy and wired components
- Build Settings configuration

### Theme Colors

| Element | Color | Hex |
|---------|-------|-----|
| Background Pink | `(0.886, 0.306, 0.455)` | #E24E74 |
| Gold Accent | `(1.0, 0.84, 0.0)` | #FFD700 |
| Admin Dark | `(0.12, 0.08, 0.06)` | #1F1410 |
| Strawberry | `(0.9, 0.1, 0.2)` | #E61A33 |
| Catcher Yellow | `(1.0, 0.85, 0.3)` | #FFD94D |

---

## Build & Deployment

1. **File > Build Settings**
2. Select target platform (Standalone / Android / iOS)
3. Verify all 7 scenes are listed in order
4. Configure player settings (resolution, orientation)
5. Build

### Platform Notes

| Platform | Notes |
|----------|-------|
| **Standalone** | Default 1920x1080 |
| **Android** | Min API 21+, portrait orientation |
| **iOS** | Portrait orientation, safe area rendering |

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| GameConfig not found | Ensure `Assets/Resources/GameConfig.asset` exists or run automated setup |
| Managers not initializing | Bootstrap runs before first scene; check console for errors |
| No audio playing | Verify audio clips are assigned in AudioManager; check volume |
| Leaderboard not saving | Check write permissions on `Application.persistentDataPath` |
| UI looks stretched/squashed | Ensure Canvas Scaler is "Scale with Screen Size" with ref resolution 1080x1920 |
| Admin panel not accessible | Tap hidden button area on Landing 10 times within 3 seconds |
| Screensaver not triggering | Only activates outside GameScene and AdminScene; wait 40 seconds |
| Exit popup not appearing | Tap top-right 15% corner area 5 times within 3 seconds |
