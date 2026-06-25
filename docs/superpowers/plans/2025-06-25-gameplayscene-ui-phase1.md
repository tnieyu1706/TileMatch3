# GameplayScene UI (Phase 1) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development or executing-plans. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Build the full static UI layout for GameplayScene in Unity using unity-mcp — no gameplay logic yet.

**Architecture:** Infrastructure-first: Canvas/Camera/EventSystem → Background → Header → Game Board Container → Slot Bar → Booster Bar → DI wiring. Every section uses anchor percentages for responsive scaling (1080×1920, Match 0.5).

**Tech Stack:** Unity 6000.0.69f1, uGUI (Canvas), URP 2D, Reflex DI, LitMotion, UniTask, TMP

---

### Task 1: Create GameplayScene + MainCamera

**Files:**
- Create: `Assets/_Project/Scenes/GameplayScene.unity`

- [ ] **Step 1: Create the scene**

```
manage_scene(action="create", name="GameplayScene", path="Assets/_Project/Scenes")
```

- [ ] **Step 2: Create MainCamera**

```
manage_gameobject(
    action="create",
    name="MainCamera",
    components_to_add=["Camera", "UniversalAdditionalCameraData"],
    position=[0, 0, -10],
    tag="MainCamera"
)
```

- [ ] **Step 3: Configure camera as orthographic**

```
manage_components(
    action="set_property",
    target="MainCamera",
    component_type="Camera",
    properties={
        "orthographic": true,
        "orthographicSize": 10,
        "clearFlags": 2,  // SolidColor
        "backgroundColor": [0.5, 0.6, 0.7, 1]
    }
)
```

---

### Task 2: Create Canvas + EventSystem

- [ ] **Step 1: Create Canvas**

```
manage_gameobject(
    action="create",
    name="GameplayCanvas",
    components_to_add=["Canvas", "CanvasScaler", "GraphicRaycaster"]
)
```

- [ ] **Step 2: Configure Canvas**

Set reference resolution 1080×1920, Match 0.5:

```
manage_components(
    action="set_property",
    target="GameplayCanvas",
    component_type="Canvas",
    properties={"renderMode": 0}
)
manage_components(
    action="set_property",
    target="GameplayCanvas",
    component_type="CanvasScaler",
    properties={
        "uiScaleMode": 1,  // ScaleWithScreenSize
        "referenceResolution": [1080, 1920],
        "screenMatchMode": 0,  // Match Width or Height
        "matchWidthOrHeight": 0.5
    }
)
```

- [ ] **Step 3: Create EventSystem**

```
manage_gameobject(
    action="create",
    name="EventSystem",
    components_to_add=["EventSystem", "InputSystemUIInputModule"]
)
```

- [ ] **Step 4: Save scene**

```
manage_scene(action="save", path="Assets/_Project/Scenes/GameplayScene.unity")
manage_scene(action="load", path="Assets/_Project/Scenes/GameplayScene.unity")
```

---

### Task 3: Create Background

**Files:**
- Modify: `Assets/_Project/Scenes/GameplayScene.unity`

- [ ] **Step 1: Create background image**

```
manage_gameobject(
    action="create",
    name="bg_dim",
    parent="GameplayCanvas",
    components_to_add=["Image"]
)
```

Set full stretch anchors and assign sprite. Use `execute_code` to set anchors precisely since unity-mcp may not support RectTransform anchors directly.

Actually, use `manage_components` to set via RectTransform:

```
manage_components(
    action="set_property",
    target="bg_dim",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0],
        "anchorMax": [1, 1],
        "offsetMin": [0, 0],
        "offsetMax": [0, 0]
    }
)
manage_components(
    action="set_property",
    target="bg_dim",
    component_type="Image",
    properties={
        "sprite": {"guid": "faa930b4ae16e0547ad9c7aa82329db6"},
        "type": 0,  // Simple
        "preserveAspect": false
    }
)
```

Set sort order to 0 (bottom layer).

- [ ] **Step 2: Create dim overlay**

```
manage_gameobject(
    action="create",
    name="dim_overlay",
    parent="GameplayCanvas",
    components_to_add=["Image"]
)
```

Set white color with alpha 40%, full stretch, raycast off:

```
manage_components(
    action="set_property",
    target="dim_overlay",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0],
        "anchorMax": [1, 1],
        "offsetMin": [0, 0],
        "offsetMax": [0, 0]
    }
)
manage_components(
    action="set_property",
    target="dim_overlay",
    component_type="Image",
    properties={"color": [1, 1, 1, 0.4], "raycastTarget": false}
)
```

- [ ] **Step 3: Save scene**

```
manage_scene(action="save", path="Assets/_Project/Scenes/GameplayScene.unity")
```

---

### Task 4: Create Header Bar

- [ ] **Step 1: Create header_bar container**

```
manage_gameobject(
    action="create",
    name="header_bar",
    parent="GameplayCanvas"
)
```

Set anchors: top-stretch (0, 0.92, 1, 1), zero offsets:

```
manage_components(
    action="set_property",
    target="header_bar",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0.92],
        "anchorMax": [1, 1],
        "offsetMin": [0, 0],
        "offsetMax": [0, 0]
    }
)
```

- [ ] **Step 2: Create left action button**

```
manage_gameobject(
    action="create",
    name="btn_left_action",
    parent="header_bar",
    components_to_add=["Image", "Button"]
)
```

Position top-left (anchor top-left, 40px from edges):

```
manage_components(
    action="set_property",
    target="btn_left_action",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 1],
        "anchorMax": [0, 1],
        "pivot": [0.5, 0.5],
        "anchoredPosition": [60, -60],
        "sizeDelta": [80, 80]
    }
)
```

Style with panel sprite (warm-yellow tint):

```
manage_components(
    action="set_property",
    target="btn_left_action",
    component_type="Image",
    properties={
        "sprite": {"guid": "6a8db038b5c57ed4898d14951c9a3fd7"},
        "color": [1, 0.85, 0.4, 1],  // warm-yellow
        "type": 1  // Sliced
    }
)
manage_components(
    action="set_property",
    target="btn_left_action",
    component_type="Button",
    properties={"transition": 0}  // None — use LitMotion later
)
```

- [ ] **Step 3: Create level label**

```
manage_gameobject(
    action="create",
    name="label_level",
    parent="header_bar",
    components_to_add=["Image", "TextMeshProUGUI"]
)
```

Center top, pill shape:

```
manage_components(
    action="set_property",
    target="label_level",
    component_type="RectTransform",
    properties={
        "anchorMin": [0.5, 1],
        "anchorMax": [0.5, 1],
        "pivot": [0.5, 0.5],
        "anchoredPosition": [0, -40]
    }
)
manage_components(
    action="set_property",
    target="label_level",
    component_type="TextMeshProUGUI",
    properties={
        "text": "Level 3",
        "fontSize": 36,
        "fontStyle": 1,  // Bold
        "color": [0.1, 0.1, 0.1, 1],  // dark text
        "alignment": 514,  // Center
        "enableAutoSizing": false
    }
)
```

Add ContentSizeFitter for auto-width:

```
manage_components(
    action="add",
    target="label_level",
    component_type="ContentSizeFitter"
)
```

Set pill background mint-green:

```
manage_components(
    action="set_property",
    target="label_level",
    component_type="Image",
    properties={
        "color": [0.365, 0.84, 0.627, 1],  // mint-green #5DEFA0
        "type": 1,  // Sliced
        "raycastTarget": false
    }
)
manage_components(
    action="set_property",
    target="label_level",
    component_type="ContentSizeFitter",
    properties={
        "horizontalFit": 1,  // PreferredSize
        "verticalFit": 2  // Unconstrained
    }
)
```

- [ ] **Step 4: Create right action button**

```
manage_gameobject(
    action="create",
    name="btn_right_action",
    parent="header_bar",
    components_to_add=["Image", "Button"]
)
```

Position top-right:

```
manage_components(
    action="set_property",
    target="btn_right_action",
    component_type="RectTransform",
    properties={
        "anchorMin": [1, 1],
        "anchorMax": [1, 1],
        "pivot": [0.5, 0.5],
        "anchoredPosition": [-60, -60],
        "sizeDelta": [80, 80]
    }
)
manage_components(
    action="set_property",
    target="btn_right_action",
    component_type="Image",
    properties={
        "sprite": {"guid": "6a8db038b5c57ed4898d14951c9a3fd7"},
        "color": [0.4, 0.7, 1, 1],  // sky-blue
        "type": 1
    }
)
manage_components(
    action="set_property",
    target="btn_right_action",
    component_type="Button",
    properties={"transition": 0}
)
```

- [ ] **Step 5: Save scene**

```
manage_scene(action="save", path="Assets/_Project/Scenes/GameplayScene.unity")
```

---

### Task 5: Create Game Board Container

- [ ] **Step 1: Create game_board_container**

```
manage_gameobject(
    action="create",
    name="game_board_container",
    parent="GameplayCanvas"
)
```

Set anchor between header and slot bar (0, 0.35, 1, 0.92):

```
manage_components(
    action="set_property",
    target="game_board_container",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0.35],
        "anchorMax": [1, 0.92],
        "offsetMin": [30, 0],  // left/right padding 30px
        "offsetMax": [-30, 0]
    }
)
```

No Image — transparent container for the tile grid later.

---

### Task 6: Create Slot Bar

- [ ] **Step 1: Create slot_bar_container**

```
manage_gameobject(
    action="create",
    name="slot_bar_container",
    parent="GameplayCanvas"
)
```

Anchor bottom (0, 0.17, 1, 0.28):

```
manage_components(
    action="set_property",
    target="slot_bar_container",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0.17],
        "anchorMax": [1, 0.28],
        "offsetMin": [0, 0],
        "offsetMax": [0, 0]
    }
)
```

- [ ] **Step 2: Create wooden rail background**

```
manage_gameobject(
    action="create",
    name="slot_bar_rail",
    parent="slot_bar_container",
    components_to_add=["Image"]
)
```

Full stretch inside container, rack sprite:

```
manage_components(
    action="set_property",
    target="slot_bar_rail",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0],
        "anchorMax": [1, 1],
        "offsetMin": [10, 5],
        "offsetMax": [-10, -5]
    }
)
manage_components(
    action="set_property",
    target="slot_bar_rail",
    component_type="Image",
    properties={
        "sprite": {"guid": "9cbdbbd8d8a312542bfea45194b59b54"},
        "type": 1  // Sliced
    }
)
```

- [ ] **Step 3: Create collected_slots container with Horizontal Layout Group**

```
manage_gameobject(
    action="create",
    name="collected_slots",
    parent="slot_bar_container",
    components_to_add=["HorizontalLayoutGroup"]
)
```

Center inside rail:

```
manage_components(
    action="set_property",
    target="collected_slots",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0],
        "anchorMax": [1, 1],
        "offsetMin": [50, 10],
        "offsetMax": [-50, -10]
    }
)
manage_components(
    action="set_property",
    target="collected_slots",
    component_type="HorizontalLayoutGroup",
    properties={
        "childAlignment": 4,  // MiddleCenter
        "spacing": 8,
        "childControlWidth": true,
        "childControlHeight": true,
        "childForceExpandWidth": false,
        "childForceExpandHeight": false
    }
)
```

- [ ] **Step 4: Create 7 slot items**

Create one slot as prefab reference, then duplicate. Each slot:

```
manage_gameobject(
    action="create",
    name="slot_item_1",
    parent="collected_slots",
    components_to_add=["Image", "Button"]
)
manage_components(
    action="set_property",
    target="slot_item_1",
    component_type="RectTransform",
    properties={
        "sizeDelta": [80, 80]
    }
)
manage_components(
    action="set_property",
    target="slot_item_1",
    component_type="Image",
    properties={
        "sprite": {"guid": "099ff59b9887cce4f86a3a65b6e1700c"},
        "type": 1  // Sliced
    }
)
manage_components(
    action="set_property",
    target="slot_item_1",
    component_type="Button",
    properties={"transition": 0}
)
```

Duplicate for slots 2-7 (copy-paste approach or batch create).

- [ ] **Step 5: Create vine decorations**

Left vine:

```
manage_gameobject(
    action="create",
    name="vine_decoration_left",
    parent="slot_bar_container",
    components_to_add=["Image"]
)
manage_components(
    action="set_property",
    target="vine_decoration_left",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0],
        "anchorMax": [0, 1],
        "pivot": [0, 0.5],
        "anchoredPosition": [10, 0],
        "sizeDelta": [40, 80]
    }
)
manage_components(
    action="set_property",
    target="vine_decoration_left",
    component_type="Image",
    properties={
        "color": [0.3, 0.6, 0.2, 1],  // leaf green
        "type": 0  // Simple
    }
)
```

Right vine:

```
manage_gameobject(
    action="create",
    name="vine_decoration_right",
    parent="slot_bar_container",
    components_to_add=["Image"]
)
manage_components(
    action="set_property",
    target="vine_decoration_right",
    component_type="RectTransform",
    properties={
        "anchorMin": [1, 0],
        "anchorMax": [1, 1],
        "pivot": [1, 0.5],
        "anchoredPosition": [-10, 0],
        "sizeDelta": [40, 80]
    }
)
manage_components(
    action="set_property",
    target="vine_decoration_right",
    component_type="Image",
    properties={
        "color": [0.3, 0.6, 0.2, 1],
        "type": 0
    }
)
```

- [ ] **Step 6: Save scene**

---

### Task 7: Create Booster Bar

- [ ] **Step 1: Create booster_bar container**

```
manage_gameobject(
    action="create",
    name="booster_bar",
    parent="GameplayCanvas"
)
```

Anchor bottom (0, 0.03, 1, 0.17):

```
manage_components(
    action="set_property",
    target="booster_bar",
    component_type="RectTransform",
    properties={
        "anchorMin": [0, 0.03],
        "anchorMax": [1, 0.17],
        "offsetMin": [30, 0],
        "offsetMax": [-30, 0]
    }
)
```

Add HorizontalLayoutGroup for 3 boosters:

```
manage_components(
    action="add",
    target="booster_bar",
    component_type="HorizontalLayoutGroup"
)
manage_components(
    action="set_property",
    target="booster_bar",
    component_type="HorizontalLayoutGroup",
    properties={
        "childAlignment": 4,  // MiddleCenter
        "spacing": 20,
        "childControlWidth": true,
        "childControlHeight": true,
        "childForceExpandWidth": false,
        "childForceExpandHeight": false
    }
)
```

- [ ] **Step 2: Create 3 booster slot buttons**

```
manage_gameobject(
    action="create",
    name="booster_slot_1",
    parent="booster_bar",
    components_to_add=["Image", "Button"]
)
manage_components(
    action="set_property",
    target="booster_slot_1",
    component_type="RectTransform",
    properties={"sizeDelta": [100, 100]}
)
manage_components(
    action="set_property",
    target="booster_slot_1",
    component_type="Image",
    properties={
        "sprite": {"guid": "099ff59b9887cce4f86a3a65b6e1700c"},
        "color": [0.8, 0.8, 0.8, 0.5],  // muted gray empty state
        "type": 1
    }
)
manage_components(
    action="set_property",
    target="booster_slot_1",
    component_type="Button",
    properties={"transition": 0}
)
```

Duplicate for booster_slot_2 and booster_slot_3.

- [ ] **Step 3: Save scene**

---

### Task 8: Create C# Scripts

- [ ] **Step 1: Create GameplaySceneController.cs**

```
manage_script(
    action="create",
    name="GameplaySceneController",
    path="Assets/_Project/Scripts/Gameplay/GameplayScene",
    namespace="TileMatch3.Gameplay.GameplayScene",
    script_type="MonoBehaviour"
)
```

Then write contents via `execute_code` or `manage_script` with contents:

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace TileMatch3.Gameplay.GameplayScene
{
    public class GameplaySceneController : MonoBehaviour
    {
        [SerializeField] private Button _btnLeftAction;
        [SerializeField] private Button _btnRightAction;
        [SerializeField] private Button[] _boosterSlots;

        private void Awake()
        {
            if (_btnLeftAction != null)
                _btnLeftAction.onClick.AddListener(OnLeftActionClicked);
            if (_btnRightAction != null)
                _btnRightAction.onClick.AddListener(OnRightActionClicked);
            for (int i = 0; i < _boosterSlots.Length; i++)
            {
                var index = i;
                if (_boosterSlots[i] != null)
                    _boosterSlots[i].onClick.AddListener(() => OnBoosterClicked(index));
            }
        }

        public void OnLeftActionClicked()
        {
            Debug.Log("[GameplayScene] Left action clicked");
        }

        public void OnRightActionClicked()
        {
            Debug.Log("[GameplayScene] Right action clicked");
        }

        public void OnBoosterClicked(int index)
        {
            Debug.Log($"[GameplayScene] Booster {index} clicked");
        }
    }
}
```

- [ ] **Step 2: Create GameplaySceneInstaller.cs**

```
manage_script(
    action="create",
    name="GameplaySceneInstaller",
    path="Assets/_Project/Scripts/Gameplay/GameplayScene",
    namespace="TileMatch3.Gameplay.GameplayScene",
    script_type="MonoBehaviour"
)
```

Contents:

```csharp
using Reflex.Core;
using UnityEngine;

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

- [ ] **Step 3: Check console for compilation errors**

```
read_console(action="get", types=["error"], count=5)
```

---

### Task 9: Create DI Scope + Wire Up

- [ ] **Step 1: Create SceneScope GameObject**

```
manage_gameobject(
    action="create",
    name="SceneScope",
    components_to_add=["ContainerScope"]
)
```

- [ ] **Step 2: Add GameplaySceneInstaller component**

```
manage_components(
    action="add",
    target="SceneScope",
    component_type="GameplaySceneInstaller"
)
```

- [ ] **Step 3: Assign controller reference in installer**

Use `execute_code` to wire the serialized reference:

```csharp
// Find the GameplaySceneController on the canvas
var scope = GameObject.Find("SceneScope");
var installer = scope.GetComponent<GameplaySceneInstaller>();
var controller = GameObject.Find("GameplayCanvas").GetComponent<GameplaySceneController>();
// Set via SerializedObject
var so = new UnityEditor.SerializedObject(installer);
so.FindProperty("_controller").objectReferenceValue = controller;
so.ApplyModifiedProperties();
```

- [ ] **Step 4: Assign button references to controller**

```csharp
var controller = GameObject.Find("GameplayCanvas").GetComponent<GameplaySceneController>();
var so = new UnityEditor.SerializedObject(controller);
so.FindProperty("_btnLeftAction").objectReferenceValue = GameObject.Find("btn_left_action")?.GetComponent<Button>();
so.FindProperty("_btnRightAction").objectReferenceValue = GameObject.Find("btn_right_action")?.GetComponent<Button>();
var boosters = new UnityEngine.Object[3];
boosters[0] = GameObject.Find("booster_slot_1")?.GetComponent<Button>();
boosters[1] = GameObject.Find("booster_slot_2")?.GetComponent<Button>();
boosters[2] = GameObject.Find("booster_slot_3")?.GetComponent<Button>();
so.FindProperty("_boosterSlots").arraySize = 3;
for (int i = 0; i < 3; i++)
    so.FindProperty($"_boosterSlots.Array.data[{i}]").objectReferenceValue = boosters[i];
so.ApplyModifiedProperties();
```

- [ ] **Step 5: Save scene as final**

- [ ] **Step 6: Create GameplayCanvas prefab for reuse**

```
manage_prefabs(
    action="create_from_gameobject",
    target="GameplayCanvas",
    prefab_path="Assets/_Project/Prefabs/GameplayCanvas.prefab"
)
```

- [ ] **Step 7: Final console check**

```
read_console(action="get", types=["error"], count=5)
```

---

### Task 10: Verify Scene Structure

- [ ] **Step 1: Validate scene**

```
manage_scene(action="validate", path="Assets/_Project/Scenes/GameplayScene.unity")
```

- [ ] **Step 2: Final confirm — check hierarchy**

```
manage_scene(action="get_hierarchy", path="Assets/_Project/Scenes/GameplayScene.unity", max_depth=3)
```
