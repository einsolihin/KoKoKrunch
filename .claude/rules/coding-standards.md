# Coding Standards

## Namespaces
All scripts must use the `KoKoKrunch` namespace hierarchy:
- `KoKoKrunch.Data` — ScriptableObjects, data models
- `KoKoKrunch.Gameplay` — Game mechanics
- `KoKoKrunch.Managers` — Singleton managers
- `KoKoKrunch.UI` — Scene UI controllers
- `KoKoKrunch.Utils` — Utility classes
- `KoKoKrunch.Editor` — Editor-only scripts (in Assets/Editor/)

## Naming Conventions
- **Classes:** PascalCase (e.g., `GameManager`, `FallingItem`)
- **Methods:** PascalCase (e.g., `StartGame()`, `AddScore()`)
- **Private fields:** camelCase with no prefix (e.g., `inactivityTimer`)
- **SerializeField:** camelCase (e.g., `[SerializeField] private float boundaryLeft`)
- **Constants:** PascalCase (e.g., `InactivityTimeout`, `RequiredTaps`)
- **Events:** On + PastTense (e.g., `OnScoreChanged`, `OnGameOver`)

## Singleton Pattern
Managers use a simple singleton with Instance property:
```csharp
public static ManagerName Instance { get; private set; }
private void Awake()
{
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
}
```
All managers are created by `PersistentManagersBootstrap` — never manually add them to scenes.

## File Organization
- One class per file
- File name matches class name exactly
- Editor scripts go in `Assets/Editor/`
- Place new scripts in the appropriate namespace subfolder under `Assets/Scripts/`

## Scene Management
- Use `SceneLoader` static class for all scene navigation
- Scene constants are defined in `SceneLoader` (e.g., `SceneLoader.GameScene`)
- Never hardcode scene names in other scripts

## SerializeField References
- Use `[SerializeField] private` for all Inspector-assigned references
- Never use `public` fields for Inspector assignment
- Wire references via `SerializedObject` in SceneSetupEditor

## Assets
- Real game images are in `Assets/Images/` — use these instead of placeholder sprites
- Custom fonts are in `Assets/Fonts/` with SDF assets
- GameConfig ScriptableObject lives in `Assets/Resources/GameConfig.asset`
