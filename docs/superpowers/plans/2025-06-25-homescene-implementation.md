# HomeScene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the HomeScene (main menu) for Tile Match Three — Canvas, sprite assets, LitMotion animations, Reflex DI.

**Architecture:** Single Canvas uGUI scene with component-based controllers. HomeSceneInstaller (Reflex) binds HomeSceneController. LitMotion handles all animations (logo entrance, button pulse, button click feedback). Navigation is a placeholder — core system TBD.

**Tech Stack:** Unity 6 (URP 2D), uGUI, Reflex DI, LitMotion, UniTask

---

## File Structure

```
Assets/_Project/
  Scenes/
    HomeScene.unity                      # NEW — the scene
  Scripts/
    Gameplay/
      HomeScene/
        HomeSceneInstaller.cs            # NEW — Reflex scene installer
        HomeSceneController.cs           # NEW — main scene controller
  Art/Sprites/Game Sheet/                # EXISTING — sprite assets
    home_background 1.png
    title 1.png
    button 1.png
```

---

### Task 1: Create branch + directory structure

**Files:**
- Create: `Assets/_Project/Scripts/Gameplay/HomeScene/` directory
- Branch: `feature/homescene`

- [ ] **Step 1: Create branch**

Run in project root:
```bash
git checkout -b feature/homescene
```

- [ ] **Step 2: Create directory**

```bash
mkdir -p Assets/_Project/Scripts/Gameplay/HomeScene
```

- [ ] **Step 3: Commit empty structure**

```bash
git add Assets/_Project/Scripts/Gameplay/HomeScene/
git commit -m "chore: create HomeScene script directory"
```

---

### Task 2: Create HomeScene Scripts (C#)

**Files:**
- Create: `Assets/_Project/Scripts/Gameplay/HomeScene/HomeSceneInstaller.cs`
- Create: `Assets/_Project/Scripts/Gameplay/HomeScene/HomeSceneController.cs`

- [ ] **Step 1: Write HomeSceneInstaller.cs**

```csharp
using Reflex.Core;
using UnityEngine;

namespace TileMatch3.Gameplay.HomeScene
{
    public class HomeSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<HomeSceneController>().AsSingle();
        }
    }
}
```

- [ ] **Step 2: Write HomeSceneController.cs**

```csharp
using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TileMatch3.Gameplay.HomeScene
{
    public class HomeSceneController : MonoBehaviour
    {
        [SerializeField] private RectTransform _logo;
        [SerializeField] private Button _playButton;

        private IDisposable _pulseMotion;

        private void Start()
        {
            AnimateLogoEntrance();
            AnimateButtonPulse();
        }

        private void AnimateLogoEntrance()
        {
            _logo.localScale = Vector3.one * 0.8f;
            _logo.GetComponent<CanvasGroup>().alpha = 0f;

            LMotion.Create(0.8f, 1f, 0.5f)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleX(_logo);

            LMotion.Create(0.8f, 1f, 0.5f)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleY(_logo);

            LMotion.Create(0f, 1f, 0.3f)
                .WithDelay(0.1f)
                .BindToAlpha(_logo.GetComponent<CanvasGroup>());
        }

        private void AnimateButtonPulse()
        {
            var seq = LSequence.Create()
                .Append(LMotion.Create(1f, 1.05f, 1f)
                    .WithEase(Ease.InOutSine))
                .Append(LMotion.Create(1.05f, 1f, 1f)
                    .WithEase(Ease.InOutSine));

            _pulseMotion = seq.Run()
                .Preserve()
                .AddTo(gameObject);
        }

        public void OnPlayClicked()
        {
            Debug.Log("[HomeScene] Play clicked — navigation TBD");
        }

        public void OnPointerDown()
        {
            LMotion.Create(1f, 0.92f, 0.1f)
                .WithEase(Ease.InBack)
                .BindToLocalScale(_playButton.transform);
        }

        public void OnPointerUp()
        {
            LMotion.Create(0.92f, 1f, 0.15f)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(_playButton.transform);
        }

        private void OnDestroy()
        {
            _pulseMotion?.Dispose();
        }
    }
}
```

- [ ] **Step 3: Create `.meta` files (Unity auto-generates on import) — skip, Unity handles this**

- [ ] **Step 4: Commit scripts**

```bash
git add Assets/_Project/Scripts/Gameplay/HomeScene/
git commit -m "feat: add HomeSceneInstaller and HomeSceneController"
```

---

### Task 3: Create and Set Up HomeScene in Unity Editor

**Files:**
- Create: `Assets/_Project/Scenes/HomeScene.unity`
- Unity Editor operations via Unity-MCP

- [ ] **Step 1: Create HomeScene**

Use Unity-MCP:
```
manage_scene(action="create", name="HomeScene", path="Assets/_Project/Scenes")
```

- [ ] **Step 2: Create Canvas**

```
manage_gameobject(action="create", name="HomeSceneCanvas")
```

- Set Canvas to Screen Space - Overlay
- Add Canvas Scaler: Scale With Screen Size, Reference Resolution 1080×1920

- [ ] **Step 3: Create background image (bg_home)**

```
manage_gameobject(action="create", name="bg_home", parent="HomeSceneCanvas")
```
- Add Image component
- Assign sprite: `home_background 1.png` from `Assets/_Project/Art/Sprites/Game Sheet/`
- Anchor: stretch full (left=0, right=1, top=1, bottom=0)

- [ ] **Step 4: Create logo panel + image**

```
manage_gameobject(action="create", name="panel_logo", parent="HomeSceneCanvas")
manage_gameobject(action="create", name="img_title", parent="panel_logo")
```
- Assign sprite: `title 1.png`
- Anchor: center
- Y-position: ~25% from top
- Add CanvasGroup component (for alpha animation)

- [ ] **Step 5: Create spacer**

```
manage_gameobject(action="create", name="spacer_mid", parent="HomeSceneCanvas")
```

- [ ] **Step 6: Create PLAY button**

```
manage_gameobject(action="create", name="btn_play", parent="HomeSceneCanvas")
```
- Add Button component
- Assign sprite: `button 1.png`
- Anchor: center
- Y-position: ~22% from bottom
- Set Button Transition to None (LitMotion handles feedback)
- Add EventTrigger component for PointerDown/PointerUp

- [ ] **Step 7: Attach scripts**

```
manage_components(action="add", target="HomeSceneCanvas", component_type="HomeSceneInstaller")
manage_components(action="add", target="HomeSceneCanvas", component_type="HomeSceneController")
```

- [ ] **Step 8: Wire up references in HomeSceneController**

- Drag `panel_logo` → `_logo` field
- Drag `btn_play` → `_playButton` field
- Assign `btn_play.onClick` → `HomeSceneController.OnPlayClicked()`
- Assign EventTrigger PointerDown → `HomeSceneController.OnPointerDown()`
- Assign EventTrigger PointerUp → `HomeSceneController.OnPointerUp()`

- [ ] **Step 9: Save scene**

```
manage_scene(action="save", name="HomeScene")
```

- [ ] **Step 10: Verify console for errors**

```
read_console(action="get", types=["error"])
```
Expected: No errors

- [ ] **Step 11: Commit scene**

```bash
git add Assets/_Project/Scenes/HomeScene.unity Assets/_Project/Scenes/HomeScene.unity.meta
git commit -m "feat: create HomeScene with Canvas, sprites, and script bindings"
```

---

### Task 4: Final Verification

- [ ] **Step 1: Check for compilation errors**

```
read_console(action="get", types=["error"])
```
Expected: No errors

- [ ] **Step 2: Verify scene loads in Editor**

```
manage_scene(action="load", name="HomeScene", path="Assets/_Project/Scenes/HomeScene.unity")
```

- [ ] **Step 3: Screenshot preview**

```
manage_camera(action="screenshot", include_image=true)
```
Verify: Canvas renders correctly, logo and button visible

- [ ] **Step 4: Run GitNexus analysis**

```bash
cd G:/GameDev/Engines/Unity/Game/TileMatch3
npx gitnexus analyze
```

- [ ] **Step 5: Detect changes before commit**

```
gitnexus_detect_changes(scope="all")
```

- [ ] **Step 6: Final commit**

```bash
git add -A
git commit -m "feat: complete HomeScene implementation"
```

---
