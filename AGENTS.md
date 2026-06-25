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
- **No C# code written yet** — project is in early setup phase with only packages and assets imported
- Reflex settings asset at `_Project/Settings/Dependencies/Resources/ReflexSettings.asset`
- `com.tnieyu1706.projectsetup` is a file-local package (`G:/GameDev/Engines/Unity/##Utils`) — not available outside this machine

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
