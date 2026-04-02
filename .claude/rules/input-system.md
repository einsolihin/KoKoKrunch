# Input System Rules

## NEVER use old Input API

This project uses Unity's **New Input System** (`com.unity.inputsystem` 1.18.0). The old `UnityEngine.Input` class is **forbidden**.

### Required Imports
```csharp
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
```

### Forbidden Patterns (NEVER use these)
- `Input.touchCount`, `Input.GetTouch()`
- `Input.GetMouseButtonDown()`, `Input.mousePosition`
- `Input.anyKeyDown`, `Input.GetAxis()`
- `Input.GetKey()`, `Input.GetKeyDown()`, `Input.GetKeyUp()`
- `StandaloneInputModule` (use `InputSystemUIInputModule` instead)

### Correct Patterns
| Old API | New Input System |
|---------|-----------------|
| `Input.touchCount > 0` | `Touch.activeTouches.Count > 0` |
| `Input.GetTouch(0).position` | `Touch.activeTouches[0].screenPosition` |
| `Input.GetMouseButtonDown(0)` | `Mouse.current.leftButton.wasPressedThisFrame` |
| `Input.mousePosition` | `Mouse.current.position.ReadValue()` |
| `Input.anyKeyDown` | `Keyboard.current.anyKey.wasPressedThisFrame` |
| `Keyboard.current.leftArrowKey.isPressed` | Already correct |
| `StandaloneInputModule` | `InputSystemUIInputModule` |

### EnhancedTouchSupport
- Call `EnhancedTouchSupport.Enable()` in `Awake()` on any MonoBehaviour that uses touch
- Call `EnhancedTouchSupport.Disable()` in `OnDestroy()`
- Multiple enables are ref-counted, so it's safe to enable in multiple scripts

### EventSystem Setup
When creating scenes programmatically (SceneSetupEditor), always use:
```csharp
es.AddComponent<InputSystemUIInputModule>();
```
Never use `StandaloneInputModule`.
