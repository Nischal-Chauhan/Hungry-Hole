# Document 03 — App Flow — Hungry Hole

## 1. Application States

```text
Launch
  ↓
Main Menu
  ↓
Gameplay
  ├── Pause Overlay
  │     ├── Resume
  │     ├── Restart
  │     └── Main Menu
  ↓
Game Over / Results
  ├── Replay
  └── Main Menu
```

## 2. Main Menu

### Entry
The player launches the game.

### Display
- Hungry Hole title/logo.
- Main visual background.
- Primary Start Play button.
- Optional secondary buttons for Settings/Shop once those systems exist.

### Actions
**Start Play**
→ initializes a new run
→ enters gameplay.

**Settings**
→ opens settings overlay.

**Shop/Customize**
→ opens customization screen when implemented.

## 3. Gameplay

### Entry
A new run initializes:
- Score.
- Combo.
- Hole radius.
- Timer.
- Target population.
- Camera.
- HUD.

### Core actions
Player moves the hole.

When the hole reaches an eligible target:
1. Detect target.
2. Confirm target is consumable.
3. Play eat effect.
4. Remove/consume target.
5. Add score.
6. Increase combo.
7. Increase hole size.
8. Update HUD.
9. Spawn/recycle replacement target if required.

If target is too large:
- Target is not consumed.
- Hole/target interaction provides feedback.
- Player must grow before consuming it.

### End condition
When the round timer reaches zero:
→ stop gameplay
→ calculate final state
→ show Game Over/Results.

## 4. Pause Flow

Gameplay
→ Pause

Overlay:
- Resume
- Restart
- Main Menu

Resume:
→ return to gameplay.

Restart:
→ confirmation if needed
→ initialize a new run.

Main Menu:
→ confirmation if needed
→ return to menu.

## 5. Game Over / Results

Display:
- Final score.
- Best score (once persistence exists).
- Summary/feedback.
- Replay button.
- Main Menu button.

Replay:
→ initialize a fresh run.

Main Menu:
→ return to main menu.

## 6. Shop / Customization Flow (V2)

Main Menu
→ Shop/Customize
→ Category selection
→ Item selection
→ Preview
→ Equip
→ Return

Possible categories:
- Hole skins.
- Rim styles.
- Visual effects.
- Avatar/cosmetic presentation.

No real-money purchases are required for V1.

## 7. Settings Flow

Main Menu
→ Settings

Possible settings:
- Master volume.
- Music volume.
- SFX volume.
- Graphics quality.
- Resolution/fullscreen.
- Camera sensitivity if introduced.

Save locally.

## 8. Loading / Error States

### Loading
For normal scene startup:
- Use a simple loading state if initialization becomes noticeable.

### Missing asset/data
Fail gracefully and log a clear developer error.

### Gameplay spawn failure
The game should continue safely if an individual target cannot spawn; do not crash the run.

## 9. Key User Journeys

### Journey A — First Run
Launch → Main Menu → Start Play → Move → Eat → Grow → Score → Timer ends → Results → Replay.

### Journey B — Replay
Results → Replay → fresh run → Gameplay.

### Journey C — Customization
Main Menu → Shop → select item → preview/equip → return → Start Play.

## 10. Navigation Principle
No dead-end screens.

Every secondary screen must provide an obvious return path.

The flow should feel like one cohesive game rather than a collection of disconnected screens.
