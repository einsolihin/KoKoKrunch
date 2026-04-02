# File Creation Policy

## Before Creating New Files
1. **Always check for existing files first** — search for screens, components, or helpers that already serve the purpose
2. **Reuse and extend** existing code whenever possible
3. **Never create new files without explicit user confirmation**

## When No Suitable File Exists
- STOP and inform the user
- Suggest a name based on existing naming patterns
- Ask for confirmation before creating

## Where New Files Go
| Type | Location |
|------|----------|
| Gameplay scripts | `Assets/Scripts/Gameplay/` |
| Manager scripts | `Assets/Scripts/Managers/` |
| UI controllers | `Assets/Scripts/UI/` |
| Data models | `Assets/Scripts/Data/` |
| Utility scripts | `Assets/Scripts/Utils/` |
| Editor scripts | `Assets/Editor/` |
| Prefabs | `Assets/Prefabs/{Items,Player,UI}/` |
| Images | `Assets/Images/` |
| Scenes | `Assets/Scenes/Game/` |

## Naming Approval
Before introducing any new identifier, observe existing patterns, propose a name, and ask for confirmation. This applies to:
- File names
- Class names
- Scene names
- Prefab names

## Import Order (required)
```csharp
// 1. System namespaces
using System;
using System.Collections.Generic;

// 2. Unity namespaces
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 3. Third-party (TMPro, etc.)
using TMPro;

// 4. Project namespaces
using KoKoKrunch.Managers;
using KoKoKrunch.Data;
```
