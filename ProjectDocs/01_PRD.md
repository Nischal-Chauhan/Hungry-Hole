# Document 01 — PRD — Hungry Hole

## 1. Product Identity
- **Project name:** Hungry Hole
- **Genre:** Casual 3D arcade / action
- **Target platform:** Windows PC
- **Engine:** Unity 6.3 LTS
- **Rendering:** URP
- **Primary reference:** Existing Hungry Hole HTML/Three.js game supplied by the project owner
- **Visual/reference source:** Uploaded development video, *I Made a Game in 48 Hours and Published It on YouTube*

## 2. Product Vision
Create a polished Unity version of Hungry Hole that preserves the existing game's core "grow by eating increasingly large objects" gameplay while substantially improving presentation, UI/UX, visual feedback, effects, progression, and overall game feel.

The existing HTML game is the gameplay baseline, not a limitation. The Unity version should reproduce its important mechanics faithfully first and then improve them.

## 3. Target User
A PC player who wants a simple, immediately understandable arcade experience: control a hole, consume objects, grow larger, score points, build combos, and clear as much of the environment as possible before time runs out.

The game should be approachable within seconds and satisfying through visual/audio feedback, growth, progression, and increasingly valuable targets.

## 4. Core Gameplay Loop
1. Launch the game.
2. Reach the main menu.
3. Start a run.
4. Control the hole around the environment.
5. Eat targets that are small enough for the current hole size.
6. Gain score and combo.
7. Grow the hole.
8. Unlock access to larger targets as the hole grows.
9. Continue for the configured round duration.
10. Show final score/results.
11. Allow replay and return to menu.

## 5. Must-Have Features

### Gameplay
- Controllable hole.
- Smooth movement suitable for keyboard controls.
- Hole growth based on consumed objects.
- Target-size eligibility: a target can only be consumed when the hole is large enough.
- Push/collision behavior when a target is too large.
- Target spawning/population.
- Multiple target categories.
- Score values by target type.
- Combo system.
- Round timer.
- Game-over/results state.
- Restart/replay flow.
- Camera follow and impact feedback.

### Target categories from the existing game
- Apple
- Melon
- Person
- Bench
- Tree
- Car
- House
- Building

The original game uses different sizes/points for these categories and starts with a populated world. These values should be treated as the initial gameplay reference and exposed as configurable Unity data rather than hard-coded wherever practical.

### UI/UX
- Main menu.
- Start Play button.
- In-game score.
- Combo indicator.
- Hole size indicator.
- Timer.
- Game-over/results screen.
- Restart/replay.
- Quit/restart confirmation where appropriate.
- Strong visual feedback for eating, scoring, combo increases, growth, and major targets.

## 6. Nice-to-Have / V2
- Character/avatar presentation around menus or results.
- Shop/customization system inspired by the uploaded video.
- Unlockable hole skins.
- Unlockable visual effects.
- Persistent high score.
- Additional environments.
- More target categories.
- Additional game modes.
- Audio settings and graphics settings.
- Controller support.
- Optional mouse/alternate controls.
- More advanced destruction/eating effects.

These must not delay the first playable milestone.

## 7. Visual Direction
The uploaded video is the visual/presentation reference. Important observed characteristics include:
- Bright, colorful stylized 3D presentation.
- A clearly visible stylized hole with a strong rim/identity.
- Large quantities of small food/object targets producing a visually busy scene.
- A stylized human/avatar presentation layer.
- Strong radial/background graphic treatments in presentation sections.
- A shop/customization-style screen.
- Strong visual feedback around the hole and consumed objects.
- A clear countdown/game HUD presentation.

Do not copy third-party assets blindly. Recreate the design language with original/project-appropriate assets.

## 8. Performance Goal
The project must remain playable on the owner's low-end hardware:
- Intel Core i5-5300U
- Intel HD Graphics 5500
- 16 GB RAM
- Windows 11

Avoid unnecessary high-poly assets, expensive post-processing, huge textures, excessive real-time shadows, and uncontrolled object counts.

## 9. Out of Scope for V1
- Online multiplayer.
- Server-dependent gameplay.
- Cloud backend.
- Account/authentication system.
- Live-service infrastructure.
- Large open-world streaming.
- Photorealistic graphics.
- Heavy HDRP rendering.

## 10. Success Criteria
V1 is successful when:
- The game launches into a polished menu.
- A player can start a run and control the hole.
- The hole can consume eligible targets.
- The hole grows correctly.
- Score/combo/timer work correctly.
- Larger target progression works.
- Game over and replay work.
- The presentation is substantially more polished than the original HTML implementation.
- The Windows build runs acceptably on the target PC.
