# TileMatch3 — AGENTS.md

## Project overview

Tile Match 3 game built with **Unity 6000.0.69f1** (Unity 6), **URP 2D Renderer**.

## Architecture

- **DI**: [Reflex](https://github.com/tnieyu1706/Reflex.git) (Zenject-style DI) — installers in `_Project/Settings/Dependencies/`
- **SOAP**: Obvious.Soap for ScriptableObject-driven data architecture
- **Async**: UniTask throughout (no raw coroutines)
- **Scene management**: `com.tnieyu1706.scenemanagement` package
- **Input**: Unity Input System (`InputSystem_Actions.inputactions`)
- **Custom package**: `Assets/TnieYuPackage/` (Runtime + Editor)
- **Editor tools**: Odin Inspector, VHierarchy, VFolders, VInspector, VTabs, Better Hierarchy

## Project layout

```
Assets/_Project/       # Main game code
  Animation/           # Animation controllers / clips
  Art/                 # Sprites, textures
  Audio/               # Sound effects, music
  Data/                # SOAP ScriptableObject assets
  Materials/           # URP materials
  Prefabs/             # Game object prefabs
  Scenes/              # Unity scenes (SampleScene.unity)
  Scripts/             # C# source code
    Tests/Editor/      # Edit mode tests
    Tests/Runtime/     # Play mode tests
  Settings/            # Project settings
    Dependencies/      # Reflex DI installers + RootScope
GameDesign/            # Design documents (.docx)
```

## Key dependencies (from `Packages/manifest.json`)

| Package | Purpose |
|---|---|
| `com.gustavopsantos.reflex` | DI framework |
| `com.obvious.soap` (local) | SOAP data architecture |
| `com.cysharp.unitask` | Async/await |
| `com.coplaydev.unity-mcp` | Unity-MCP AI integration |
| `com.eflatun.scenereference` | Type-safe scene references |
| `com.tnieyu1706.scenemanagement` | Scene loading/management |
| `com.tnieyu1706.projectsetup` | Shared project setup utils (local path) |

## Testing

Test framework: Unity Test Framework (`com.unity.test-framework` `1.6.0`).
Test directories exist at `_Project/Scripts/Tests/Editor/` and `Tests/Runtime/` — no tests written yet.

## Gotchas

- **CRLF warnings** on `git add` are normal on Windows — `.meta` files and JSON may warn about LF→CRLF conversion
- **Push**: branch `main` has no upstream until first `git push --set-upstream origin main`
- **No CI/CD** configured yet
- **No C# code written yet** — project is in early setup phase with only packages and assets imported
- Reflex settings asset at `_Project/Settings/Dependencies/Resources/ReflexSettings.asset`
- `com.tnieyu1706.projectsetup` is a file-local package (`G:/GameDev/Engines/Unity/##Utils`) — not available outside this machine
