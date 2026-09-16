# Document 02 — TRD — Hungry Hole

## 1. Technical Stack
- **Engine:** Unity 6.3 LTS (6000.3.23f1)
- **Rendering:** Universal Render Pipeline (URP)
- **Language:** C#
- **Target:** Windows PC
- **Build technology:** Windows Build Support (IL2CPP)
- **Input:** Unity Input System where practical
- **Scene format:** Unity scenes
- **Architecture:** Component-based Unity architecture with small, testable systems
- **Data:** ScriptableObjects for configurable gameplay data where useful
- **Version control:** Recommended, but no provider is currently locked in

## 2. Important Constraint
The project must be designed for an Intel HD 5500-class integrated GPU and i5-5300U CPU.

Prefer:
- Low-poly geometry.
- Small/medium texture sizes.
- Baked or inexpensive lighting where possible.
- Limited real-time shadows.
- Object pooling.
- Controlled particle counts.
- Minimal post-processing.
- No unnecessary packages or services.
- No large asset packs unless specifically justified.

## 3. Architecture

Suggested runtime systems:

```text
GameManager
├── GameStateManager
├── ScoreManager
├── ComboManager
├── TimerManager
├── TargetSpawner
├── HoleController
├── CameraController
├── AudioManager
└── UIManager
```

Suggested gameplay components:

```text
Hole
├── HoleController
├── HoleVisual
├── HoleCollider / trigger logic
└── Growth system

Target
├── TargetData
├── TargetController
├── Collider
└── Eat/destruction behavior
```

## 4. Suggested Folder Structure

```text
Assets/
├── Audio/
├── Effects/
├── Materials/
├── Models/
├── Prefabs/
├── Scenes/
│   ├── Main.unity
│   └── Gameplay.unity (if later separated)
├── Scripts/
│   ├── Core/
│   ├── Gameplay/
│   ├── Targets/
│   ├── Camera/
│   ├── UI/
│   ├── Audio/
│   └── Editor/
├── Textures/
├── UI/
└── Settings/
```

## 5. Scene Architecture

Initial main scene:

```text
Main
├── Main Camera
├── Directional Light
├── Global Volume
├── GameManager
├── World
│   ├── Ground
│   ├── Targets
│   └── Environment
├── Hole
└── Canvas
```

Keep the scene hierarchy clean and avoid placing large numbers of individual targets manually when they can be spawned/pool-managed.

## 6. Gameplay Data

Target data should preferably be configurable:

```text
Target ID
Display Name
Category
World Size
Required Hole Radius
Score Value
Growth Value
Mass/Push Strength
Prefab
Audio
VFX
```

Initial categories come from the existing HTML implementation:
apple, melon, person, bench, tree, car, house, building.

## 7. Input
V1:
- WASD
- Arrow keys

Later:
- Controller
- Optional mouse/alternate controls

Do not add mobile joystick support to the Windows build unless it becomes useful for a future platform.

## 8. Rendering
Use URP with a conservative configuration:
- Avoid HDRP.
- Keep render scale/resolution reasonable.
- Use simple materials.
- Keep shadow resolution modest.
- Disable expensive effects unless tested.
- Prefer stylized lighting over expensive realism.

## 9. Persistence
V1 does not require a backend.

Local persistence may later store:
- Best score.
- Settings.
- Unlocked cosmetic items.

Use Unity-appropriate local storage; do not introduce a database or authentication system without a product requirement.

## 10. External Services
No required third-party backend/API for V1.

Do not add:
- Supabase
- Firebase
- Authentication
- Cloud database
- Analytics
- Online services

unless a later requirement explicitly calls for them.

## 11. Coding Standards
- Use clear class names.
- One primary responsibility per MonoBehaviour.
- Avoid giant "God" scripts.
- Serialize important tuning values.
- Prefer events/interfaces for loose coupling where appropriate.
- Avoid per-frame allocations.
- Pool frequently spawned objects/effects.
- Add comments only where they explain non-obvious logic.
- Keep public APIs small.
- Do not silently change gameplay values from the source reference.

## 12. Definition of Technical Done
A system is technically complete only when:
- It compiles without errors.
- It works in Play Mode.
- It does not introduce avoidable frame spikes/allocations.
- Its important parameters are configurable.
- It is integrated with the documented architecture.
- It does not break previously completed systems.
