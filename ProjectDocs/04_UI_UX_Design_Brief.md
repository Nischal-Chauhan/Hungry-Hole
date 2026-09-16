# Document 04 — UI/UX Design Brief — Hungry Hole

## 1. Design Goal
Create a playful, energetic, polished arcade-game interface that matches the colorful stylized 3D direction observed in the uploaded development video while remaining readable and lightweight.

The UI should feel like a finished game rather than a developer prototype.

## 2. Visual Direction
- Playful.
- Colorful.
- Energetic.
- Clean.
- High readability.
- Strong game-like hierarchy.
- Bold primary actions.
- Stylized rather than photorealistic.
- UI should complement, not cover, the 3D world.

The uploaded video shows colorful scenes, stylized character presentation, strong radial graphic backgrounds in presentation moments, and a shop/customization presentation. These are visual references, not assets to copy.

## 3. Color Direction
Use a bright arcade palette with:
- Dark/near-black tones for hole-related UI.
- Bright accent colors for score/combo.
- Warm colors for warnings/countdowns.
- Cool/light backgrounds where appropriate.
- Strong contrast between UI text and its background.

Exact colors should be centralized in a UI theme asset so they can be changed globally.

## 4. Typography
Prefer a bold, highly readable game font for:
- Title.
- Score.
- Combo.
- Timer.
- Buttons.

Body/supporting text should be simple and highly legible.

Avoid tiny text.

## 5. Main Menu
Recommended hierarchy:

```text
[HUNGRY HOLE]

     Play
  Customize
   Settings
```

The Play button should be the strongest visual element.

Use subtle animation:
- Button hover/press.
- Title motion.
- Background motion.
- Small environmental movement.

Avoid expensive animated UI effects.

## 6. Gameplay HUD

Recommended layout:

```text
┌─────────────────────────────────────┐
│ SCORE                 TIME          │
│ 000000                01:30         │
│                                     │
│              GAME WORLD             │
│                                     │
│                                     │
│         [ HOLE ]                    │
│                                     │
│                                     │
│              COMBO                  │
│                                     │
│        HOLE SIZE / PROGRESS         │
└─────────────────────────────────────┘
```

HUD priorities:
1. Timer.
2. Score.
3. Current combo.
4. Hole size/progression.

The HUD should remain readable while the world is busy.

## 7. Feedback
When consuming an object:
- Floating score.
- Small scale/pop animation.
- Particle burst.
- Audio cue.
- Hole growth feedback.

For larger targets:
- Stronger camera shake.
- Larger VFX.
- Stronger audio.
- More dramatic score feedback.

Keep effects performant.

## 8. Combo Presentation
Combo should feel rewarding:
- Increase text size slightly.
- Brief scale animation.
- Accent glow/flash.
- Optional streak meter.

Do not permanently occupy a large portion of the screen.

## 9. Timer
Use a prominent countdown.

Suggested states:
- Normal.
- Warning near end.
- Final seconds.

Warning effects should remain readable and should not become visually annoying.

## 10. Game Over
Recommended:

```text
      TOWN CLEARED!

      SCORE
      12,450

      BEST
      18,920

   [ PLAY AGAIN ]
   [ MAIN MENU ]
```

The results screen should feel rewarding, not like a debug panel.

## 11. Shop / Customize
Inspired by the visual idea shown in the uploaded video:
- Large preview.
- Item cards.
- Clear selected/equipped state.
- Price/unlock state if progression is introduced.
- Simple navigation.
- Avoid complex menus.

## 12. Interaction
Desktop:
- Mouse hover feedback.
- Clear click states.
- Keyboard navigation where practical.
- Escape for pause/back where appropriate.

## 13. Accessibility
- High contrast.
- Minimum readable font sizes.
- Avoid communicating critical information through color alone.
- UI scaling should work at common Windows resolutions.
- Do not require rapid UI interactions.

## 14. Performance
UI must not:
- Spawn unnecessary objects every frame.
- Use large animated textures unnecessarily.
- Add expensive full-screen effects.
- Continuously allocate memory.

## 15. Design Rule
Every UI element must answer one question:
**Does this help the player understand, decide, or enjoy the game?**

If not, remove it.
