# GameplayScene — Static UI (Phase 1)

## Overview

Build the full static UI layout for GameplayScene in Unity using unity-mcp.
No gameplay logic — only Canvas structure, prefabs, DI wiring, and responsive layout.

## Scene Hierarchy

```
GameplayScene (scene file)
├── MainCamera (orthographic, URP)
├── GameplayCanvas (Screen Space - Overlay, 1080×1920, Match 0.5)
│   ├── bg_dim (Image — gameplay_background 1.png)
│   │   └── dim_overlay (Image — white #FFFFFF, alpha 40%, raycast off)
│   ├── header_bar (RectTransform — anchor top-stretch)
│   │   ├── btn_left_action (Button + warm-yellow bg)
│   │   ├── label_level (mint pill + TMP "Level 3")
│   │   └── btn_right_action (Button + sky-blue bg)
│   ├── game_board_container (RectTransform — empty, anchor mid)
│   ├── slot_bar_container (RectTransform — anchor bottom)
│   │   ├── vine_decoration_left (Image)
│   │   ├── slot_bar_rail (Image — rack 1.png)
│   │   ├── collected_slots (Horizontal Layout Group)
│   │   │   └── slot_item × 7 (prefab)
│   │   └── vine_decoration_right (Image)
│   └── booster_bar (RectTransform — anchor bottom)
│       ├── booster_slot_1 (Button)
│       ├── booster_slot_2 (Button)
│       └── booster_slot_3 (Button)
├── SceneScope (GameObject)
│   └── ContainerScope + GameplaySceneInstaller
└── EventSystem + InputSystemUIInputModule
```

## Responsive Layout Strategy (1080×1920, Match 0.5)

All major sections use **anchor percentage** (anchorMin/Max), NOT fixed offsets.

| Vùng | anchorMin.y | anchorMax.y |
|------|-------------|-------------|
| Header | 0.92 | 1.0 |
| Game Board | 0.35 | 0.92 |
| Slot Bar | 0.17 | 0.28 |
| Booster Bar | 0.03 | 0.17 |

- **Background**: full stretch (0,0,1,1)
- **Header buttons**: anchor top-left / top-right, fixed size (80×80), canvas scaler handles resolution
- **Level label**: anchor top-center, ContentSizeFitter for pill auto-width
- **Slot items + Boosters**: AspectRatioFitter 1:1, Horizontal Layout Group auto-spacing
- **ConstrainProportionsScale**: on all Image elements with sprites

## Scripts

### GameplaySceneController.cs
```csharp
namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplaySceneController : MonoBehaviour
    {
        [SerializeField] private Button _btnLeftAction;
        [SerializeField] private Button _btnRightAction;
        [SerializeField] private Button[] _boosterSlots;

        private void Awake()
        {
            _btnLeftAction.onClick.AddListener(OnLeftActionClicked);
            _btnRightAction.onClick.AddListener(OnRightActionClicked);
            for (int i = 0; i < _boosterSlots.Length; i++)
            {
                var index = i;
                _boosterSlots[i].onClick.AddListener(() => OnBoosterClicked(index));
            }
        }

        public void OnLeftActionClicked() { /* stub — Pause/Settings */ }
        public void OnRightActionClicked() { /* stub — Hint/Info */ }
        public void OnBoosterClicked(int index) { /* stub */ }
    }
}
```

### GameplaySceneInstaller.cs
```csharp
namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplaySceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private GameplaySceneController _controller;
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterValue(_controller);
        }
    }
}
```

## Scene Objects Setup

| GameObject | Components | Notes |
|------------|------------|-------|
| `SceneScope` | ContainerScope (Reflex), GameplaySceneInstaller | DI scope, separate from Canvas per AGENTS.md |
| `EventSystem` | EventSystem, InputSystemUIInputModule | References InputSystem_Actions.inputactions |

## Implementation Steps (via unity-mcp)

1. **Create scene** — Create GameplayScene.unity, add MainCamera (orthographic URP)
2. **Create Canvas** — GameplayCanvas, Screen Space - Overlay, 1080×1920, Match 0.5
3. **Create EventSystem** — EventSystem + InputSystemUIInputModule
4. **Create Background** — bg_dim + dim_overlay
5. **Create Header** — header_bar with btn_left, label_level, btn_right
6. **Create Game Board Container** — game_board_container (empty)
7. **Create Slot Bar** — slot_bar_container with rail, 7 slots, vines
8. **Create Booster Bar** — booster_bar with 3 buttons
9. **Create DI scope** — SceneScope GameObject with ContainerScope + installer
10. **Create C# scripts** — GameplaySceneController, GameplaySceneInstaller
11. **Wire up** — assign references, connect button events
12. **Save scene + prefab** — save as GameplayScene.unity
