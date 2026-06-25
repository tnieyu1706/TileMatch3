# TileMatch3 — AGENTS.md

## Project overview

Tile Match 3 game built with **Unity 6000.0.69f1** (Unity 6), **URP 2D Renderer**.

## AI tools

- **GitNexus MCP** — code knowledge graph (indexed ✅). Always run `impact()` before edits, `detect_changes()` before commits.
- **Unity-MCP** (com.coplaydev.unity-mcp) — bridge to Unity Editor. Enable in Unity before using; communicate with Unity scene/objects via MCP tools.

## Architecture

- **DI**: [Reflex](https://github.com/tnieyu1706/Reflex.git) (Zenject-style DI) — installers in `_Project/Settings/Dependencies/`
- **SOAP**: Obvious.Soap for ScriptableObject-driven data architecture
- **Async**: UniTask throughout (no raw coroutines)
- **Animation**: LitMotion (lightweight, job-based tweening)
- **Scene management**: `com.tnieyu1706.scenemanagement` package — multi-scene additive navigation
- **Input**: Unity Input System (`InputSystem_Actions.inputactions`)
- **Custom packages**:
  - `Assets/TnieYuPackage/` — shared Runtime + Editor utilities
  - `com.tnieyu1706.projectsetup` — file-local package for project-wide setup utils
- **Editor tools**: Odin Inspector (custom Inspector UI), VHierarchy, VFolders, VInspector, VTabs, Better Hierarchy

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
    Core/              # Shared/important scripts (services, core systems)
    Gameplay/          # Scene-specific scripts (HomeScene, etc.)
    Tests/Editor/      # Edit mode tests
    Tests/Runtime/     # Play mode tests
  Settings/            # Project settings
    Dependencies/      # Reflex DI installers + RootScope
GameDesign/            # Design documents (.docx)
```

## Key dependencies (from `Packages/manifest.json`)

| Package | Purpose |
|---|---|---|
| `com.gustavopsantos.reflex` | DI framework |
| `com.obvious.soap` (local) | SOAP data architecture |
| `com.cysharp.unitask` | Async/await |
| `com.coplaydev.unity-mcp` | Unity-MCP (AI ↔ Unity Editor bridge) |
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
- **C# code location**: Shared/core systems go in `Scripts/Core/`, scene-specific logic in `Scripts/Gameplay/{SceneName}/`
- Reflex settings asset at `_Project/Settings/Dependencies/Resources/ReflexSettings.asset`
- `com.tnieyu1706.projectsetup` is a file-local package (`G:/GameDev/Engines/Unity/##Utils`) — not available outside this machine

## Scene Layouts (UI Analysis)

Layout analysis documents at `Assets/_Project/Art/Layouts/` for 4 planned scenes:

### HomeScene / MainMenu
- **Style**: Fullscreen illustration, village/meadow landscape, casual mobile portrait
- **Layout**: Logo panel (10–55% height) → spacer (55–70%) → PLAY button pill mint-green (70–85%) → decorative flowers footer
- **Logo**: 3-line "Tile Match Three" with wooden ribbon banner, each word colored (yellow / sky-blue / lime-green), prominent white outline + drop shadow
- **CTA**: PLAY button — mint gradient (#5DEFA0→#3DD68C), pill shape, soft shadow
- **No header/footer UI** — everything floats on background art

### GameplayScene / InGame
- **Style**: Same village background but dimmed (white overlay ~40%) to make game board pop
- **Layout**: Header bar (0–8%) → Tile Grid 7×7 layered (8–65%) → gap (65–72%) → Collection Slot Bar (72–83%) → Booster Bar (83–97%) → bottom pad
- **Header**: Level label (mint pill) + 2 action buttons (warm-yellow left, sky-blue right)
- **Tile Grid**: Multi-layer stacked tiles (irregular shape, not full rectangle); top layer brighter with stronger shadow, lower layers dimmed
- **Slot Bar**: Wooden plank rail with 7 empty slots + vine decorations at ends
- **Booster Bar**: 3 empty booster slots at bottom

### LoadingScene / SplashScreen
- **Style**: Same logo + detailed village background (boat, tower, mailbox), full brightness
- **Layout**: Logo (5–48%) → scenery gap (48–80%) → progress bar pill (80–90%) → tagline text (90–95%) → bottom pad
- **Progress bar**: Mint gradient fill + "72%" bold label centered, ivory track, pill shape
- **Tagline**: Light cyan italic text below bar, cycles tips/lore during loading
- **Auto-driven**: progress fill and text change by loading progress; auto-fade on 100%

### LevelMapScene
- PNG reference exists at `Assets/_Project/Art/Layouts/LevelMapScene.png` — no analysis document yet

### Shared Design Tokens
| Token | Value |
|-------|-------|
| Accent colors | Mint-green (#5DEFA0→#3DD68C), warm-yellow, sky-blue, lime-green |
| Logo style | 3-line colored text + wooden ribbon, white outline + shadow |
| Background | Illustrated village scene (reused across scenes) |
| Buttons | Pill shape, rounded_full, soft shadow |
| Typography | Bold + large/extralarge for headings, bold for labels |

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **TileMatch3** (9534 symbols, 19656 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> Index stale? Run `node .gitnexus/run.cjs analyze` from the project root — it auto-selects an available runner. No `.gitnexus/run.cjs` yet? `npx gitnexus analyze` (npm 11 crash → `npm i -g gitnexus`; #1939).

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows. For regression review, compare against the default branch: `detect_changes({scope: "compare", base_ref: "main"})`.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `query({search_query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `context({name: "symbolName"})`.
- For security review, `explain({target: "fileOrSymbol"})` lists taint findings (source→sink flows; needs `analyze --pdg`).

## Never Do

- NEVER edit a function, class, or method without first running `impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `rename` which understands the call graph.
- NEVER commit changes without running `detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/TileMatch3/context` | Codebase overview, check index freshness |
| `gitnexus://repo/TileMatch3/clusters` | All functional areas |
| `gitnexus://repo/TileMatch3/processes` | All execution flows |
| `gitnexus://repo/TileMatch3/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
