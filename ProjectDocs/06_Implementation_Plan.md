# Document 06 — Implementation Plan — Hungry Hole

## Build Principle
Build in small vertical slices. Every phase must compile and be testable before the next phase begins.

The existing HTML game is the gameplay reference; the uploaded video is the visual/presentation reference.

## Phase 1 — Project Setup
### Goal
Establish a clean Unity foundation.

Tasks:
1. Unity 6.3 LTS project.
2. URP.
3. Windows build support.
4. Organized Assets folders.
5. Main scene.
6. Basic project settings.
7. Lightweight rendering defaults.
8. Confirm Play Mode works.

### Done when
- Project opens without errors.
- Main scene loads.
- Play Mode works.
- No avoidable console errors.

## Phase 2 — Core Hole Prototype
### Goal
Create the central game mechanic.

Tasks:
1. Ground.
2. Hole visual.
3. Hole rim.
4. Hole controller.
5. Keyboard movement.
6. Camera follow.
7. Configurable hole radius.
8. Basic growth.

### Done when
- Player can move the hole smoothly.
- Camera follows.
- Hole can visually grow.

## Phase 3 — Eating System
### Goal
Make the game actually playable.

Tasks:
1. Target interface/base component.
2. Target size.
3. Required hole radius.
4. Consumption detection.
5. Too-large target collision/push behavior.
6. Eat animation.
7. Target removal.
8. Hole growth on consumption.

### Done when
- Small target can be eaten.
- Too-large target cannot be eaten.
- Successful eating grows the hole.

## Phase 4 — Target Library
### Goal
Recreate the reference game's object progression.

Implement:
1. Apple.
2. Melon.
3. Person.
4. Bench.
5. Tree.
6. Car.
7. House.
8. Building.

Tasks:
- Prefab per category.
- Configurable size.
- Score.
- Growth value.
- Appropriate visual/audio feedback.
- Spawn rules.

### Done when
- All target classes can spawn.
- Size progression is logical.
- Targets interact correctly with the hole.

## Phase 5 — Score, Combo and Timer
### Goal
Complete the arcade loop.

Tasks:
1. Score manager.
2. Combo manager.
3. Combo timeout/reset.
4. 90-second initial round duration based on reference game.
5. Countdown.
6. Floating score feedback.
7. Game-over trigger.

### Done when
- Score changes correctly.
- Combo increases/resets correctly.
- Timer reaches zero and ends the run.

## Phase 6 — World / Environment
### Goal
Create the stylized playable environment.

Tasks:
1. Ground material.
2. Roads/paths.
3. Simple city blocks.
4. Houses.
5. Buildings.
6. Trees.
7. Environmental decoration.
8. Target distribution.
9. Spawn boundaries.

Use lightweight geometry and materials.

### Done when
- Environment communicates a town/city.
- Targets are readable.
- Performance remains acceptable on target hardware.

## Phase 7 — Camera and Game Feel
### Goal
Make actions feel satisfying.

Tasks:
1. Smooth camera follow.
2. Dynamic framing based on hole size.
3. Controlled camera shake.
4. Eat impact feedback.
5. Growth feedback.
6. Large-target feedback.

### Done when
- Movement feels responsive.
- Eating feels rewarding.
- Effects do not hurt performance.

## Phase 8 — UI/UX
### Goal
Replace the prototype UI with the polished game interface.

Tasks:
1. Main menu.
2. Start button.
3. Gameplay HUD.
4. Score.
5. Timer.
6. Combo.
7. Hole-size/progress indicator.
8. Pause menu.
9. Game-over/results.
10. Settings.
11. Optional shop/customization foundation.

### Done when
- All documented navigation paths work.
- UI is readable at target resolutions.
- No screen has dead-end navigation.

## Phase 9 — Visual Polish
### Goal
Move from functional prototype to polished presentation.

Tasks:
1. Final hole visual.
2. Better target models/materials.
3. Particles.
4. VFX.
5. Audio.
6. UI animation.
7. Background/presentation graphics.
8. Menu presentation.
9. Shop/customization presentation if included.

Use the uploaded video as the visual reference while keeping assets original and performance-conscious.

### Done when
- Game has a coherent visual identity.
- Feedback is satisfying.
- No major prototype/debug visuals remain.

## Phase 10 — Persistence and Optional Progression
### Goal
Add lightweight local progression.

Tasks:
1. Best score.
2. Settings persistence.
3. Cosmetic unlock state if shop is implemented.
4. Save validation/versioning.

### Done when
- Restarting the game preserves intended local data.
- Corrupt/missing data falls back safely.

## Phase 11 — Optimization
### Goal
Make the game practical for the target PC.

Tasks:
1. Profile CPU/GPU.
2. Reduce unnecessary draw calls.
3. Pool targets.
4. Pool VFX.
5. Optimize shadows.
6. Optimize textures.
7. Limit particles.
8. Check garbage allocations.
9. Test multiple resolutions.
10. Test long Play Mode sessions.

### Done when
- No major performance spikes.
- Memory use remains controlled.
- Game is comfortable on the target machine.

## Phase 12 — Windows Build and QA
### Goal
Produce a usable Windows build.

Tasks:
1. Configure Windows build.
2. Build development version.
3. Test clean launch.
4. Test menu.
5. Test gameplay.
6. Test pause/restart.
7. Test game over.
8. Test settings.
9. Test save data.
10. Test resolution/fullscreen.
11. Build final candidate.

### Done when
- Fresh Windows build launches.
- All critical journeys work.
- No blocking errors.
- Game can be played from launch to completion and replayed.

## AI Agent Operating Rules
1. Treat these six documents as project source-of-truth documents.
2. Do not invent a backend.
3. Do not change the core gameplay rules without documenting the change.
4. Do not introduce heavy packages without justification.
5. Do not create huge amounts of code in one step.
6. Build one phase/system at a time.
7. After each major system, compile/test before proceeding.
8. Prefer reusable prefabs/components over duplicated logic.
9. Keep performance appropriate for Intel HD 5500.
10. When a requirement is ambiguous, flag it instead of silently inventing behavior.
