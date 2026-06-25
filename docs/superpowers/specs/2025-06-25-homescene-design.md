# HomeScene Design — Tile Match Three

## Overview

Design and implementation blueprint for the HomeScene (Main Menu) of Tile Match Three — a casual mobile match-3 game built with Unity 6 (URP 2D). This is the first scene the player sees on launch.

## UI System

- **uGUI** (Canvas-based), not UI Toolkit
- Reference resolution: 1080×1920 portrait
- Canvas Scaler: Scale With Screen Size

## Scene Setup

| Property | Value |
|----------|-------|
| Scene file | `Assets/_Project/Scenes/HomeScene.unity` |
| Camera | Orthographic, Size = 5, Clear Flags = Solid Color |
| Build Index | 0 (first scene) |
| Lighting | No realtime lights needed (UI-only) |

## Hierarchy

```
HomeSceneCanvas (Canvas - Screen Space - Overlay, 1080×1920)
├── bg_home (Image)
│   - Sprite: home_background 1.png
│   - Anchor preset: stretch full
│   - Pivot: (0.5, 0.5)
│   - Color: white (use sprite as-is)
│
├── panel_logo (RectTransform)
│   ├── img_title (Image)
│   │   - Sprite: title 1.png
│   │   - Anchor: center
│   │   - Preserve aspect ratio
│   │   - Y position: ~25% from top
│   │   - Scale: 0.8 (initial for animation)
│   │
│   └── (optional sparkle decorative — future)
│
├── spacer_mid (RectTransform, transparent)
│   - Flexible height, pushes btn_play down
│
└── btn_play (Button)
    - Image: button 1.png
    - Anchor: center
    - Y position: ~22% from bottom
    - Transition: None (LitMotion handles press)
    - onClick: HomeSceneController.OnPlayClicked()
```

## Scripts Structure

```
Assets/_Project/Scripts/Gameplay/HomeScene/
├── HomeSceneController.cs
└── HomeSceneInstaller.cs
```

- `Gameplay/` = scene-specific scripts
- `Core/` = shared systems (navigation etc. — TBD, not part of this spec)

## Script Details

### HomeSceneInstaller.cs

- Extends `MonoInstaller` (Reflex)
- Binds `HomeSceneController` as single
- Attached to a GameObject in the scene

```csharp
public class HomeSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<HomeSceneController>().AsSingle();
    }
}
```

### HomeSceneController.cs

- Resolves via Reflex injection
- Holds references to Canvas elements
- Manages:
  - Logo entrance animation (scale + alpha via LitMotion)
  - Button pulse idle animation (LitMotion loop)
  - Button click animation (LitMotion)
  - PLAY click → placeholder log (navigation TBD)

```csharp
[RequireComponent(typeof(HomeSceneInstaller))]
public class HomeSceneController : MonoBehaviour
{
    [SerializeField] private RectTransform _logo;
    [SerializeField] private Button _playButton;
    
    private void Start()
    {
        AnimateLogoEntrance();
        AnimateButtonPulse();
    }
    
    public void OnPlayClicked()
    {
        // TODO: Replace with navigation when core system is ready
        Debug.Log("[HomeScene] Play clicked — navigation TBD");
    }
}
```

## DI Setup

- **Global**: `RootScope.prefab` installed in `Assets/_Project/Settings/Dependencies/`
- **Scene-level**: `HomeSceneInstaller` on a GameObject inside HomeScene
- Reflex auto-detects scene installers when scene loads

## Animations (LitMotion)

| Animation | Target | Type | Ease | Duration | Notes |
|-----------|--------|------|------|----------|-------|
| Logo entrance | `_logo` localScale 0.8 → 1.0 | Float | OutBack | 0.5s | + alpha 0 → 1 |
| Logo entrance | `_logo` alpha 0 → 1 | Float | Linear | 0.3s | After scale starts |
| Button pulse | `_playButton` scale 1.0 ↔ 1.05 | Float | InOutSine | 2s loop | Idle state |
| Button press | scale 1.0 → 0.92 | Float | InBack | 0.1s | OnPointerDown |
| Button release | scale 0.92 → 1.0 | Float | OutBack | 0.15s | OnPointerUp |

## Navigation

- **Not implemented in this phase**
- `OnPlayClicked()` is a placeholder (`Debug.Log`)
- When navigation core system is built, inject via Reflex and call `SceneNavigationService.LoadSceneAsync()`

## Assets

| Asset | Path | Usage |
|-------|------|-------|
| `home_background 1.png` | `Art/Sprites/Game Sheet/` | Fullscreen background |
| `title 1.png` | `Art/Sprites/Game Sheet/` | Logo "Tile Match Three" |
| `button 1.png` | `Art/Sprites/Game Sheet/` | PLAY button |

## Future Considerations

- **Navigation system**: Core system TBD — inject into HomeSceneController when ready
- **Audio**: Click SFX, background music — not in scope yet
- **Logo sparkles**: Decorative particles — future enhancement
- **Loading screen**: Between scenes — separate scene design
