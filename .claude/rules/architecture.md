# Architecture Rules

## Game Flow
```
LandingScene → NameInputScene → InstructionScene → GameScene → ResultScene → LeaderboardScene
                                                                    ↕
                                                              (Play Again loops back to NameInput)
LandingScene → AdminScene (hidden access: 10 taps on logo area)
```

## Manager System
All managers are bootstrapped automatically via `PersistentManagersBootstrap` using `[RuntimeInitializeOnLoadMethod]`. They live on a single DontDestroyOnLoad GameObject.

**Never manually instantiate managers. Never add them to scenes.**

### Manager Responsibilities
| Manager | Purpose |
|---------|---------|
| `GameManager` | Game state, score, lives, timer, events |
| `DataManager` | Leaderboard JSON persistence, CSV export |
| `AudioManager` | BGM and SFX playback (dual AudioSource) |
| `SettingsManager` | Runtime game settings persistence |
| `ScreensaverManager` | 40s inactivity overlay, dismisses to Landing |
| `AdminExitManager` | Corner-tap password exit sequence |

## Event-Driven Communication
UI scripts subscribe to GameManager events — never poll state in Update:
```csharp
GameManager.Instance.OnScoreChanged += UpdateScoreText;
GameManager.Instance.OnLivesChanged += UpdateHearts;
GameManager.Instance.OnTimeChanged += UpdateTimer;
GameManager.Instance.OnGameOver += HandleGameOver;
```

## Data Persistence
- **Leaderboard:** `{Application.persistentDataPath}/leaderboard.json`
- **Settings:** `{Application.persistentDataPath}/game_settings.json`
- **CSV Export:** `{Application.persistentDataPath}/leaderboard_export.csv`

Never use PlayerPrefs for persistent data.

## SceneSetupEditor
The `Assets/Editor/SceneSetupEditor.cs` is the automated setup tool. When run from Unity menu ("KoKo Krunch/Setup All Scenes and Prefabs"), it recreates all 7 scenes and prefabs.

When modifying scene layout:
1. Update the corresponding method in SceneSetupEditor (e.g., `CreateLandingScene()`)
2. Use `LoadImageSprite()` / `SetImageSprite()` for real image assets
3. Use `InputSystemUIInputModule` for EventSystem (never StandaloneInputModule)
4. Wire SerializedObject references for all UI components
