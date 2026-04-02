# Development Behavior Rules

## General
- Keep changes minimal and focused on what was requested
- Follow existing project style exactly (indentation, spacing, variable naming)
- Do not rename unrelated variables or methods
- Do not refactor or optimize unless explicitly asked
- Do not introduce packages, libraries, or frameworks not already used

## When Uncertain — STOP and Ask
- Which script or component file to modify
- Whether a similar component already exists elsewhere
- Expected behavior for a new feature
- Navigation structure changes
- GameConfig parameter changes

**Never assume. Never guess.**

## Testing
- After modifying scripts, verify no compilation errors via IDE diagnostics
- When modifying SceneSetupEditor, re-run the setup tool in Unity to verify scene generation
- Test touch, mouse, and keyboard input paths when modifying input handling

## Image Assets
- Real image assets are in `Assets/Images/` — always prefer these over placeholder sprites
- When adding new images, place them in `Assets/Images/` and update SceneSetupEditor if needed
- Image files should use descriptive names matching their purpose

## Build Notes
- Primary target is Standalone (touch panels), with Android/iOS secondary
- Portrait orientation only (1080x1920)
- Always test with both mouse and touch input
