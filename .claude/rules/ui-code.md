---
paths:
  - "src/ui/**"
---

# UI Code Rules

- UI must NEVER own or directly modify game state — display only, use commands/events to request changes
- Support both keyboard/mouse AND gamepad input for all interactive elements
- All animations must be skippable and respect user motion/accessibility preferences
- UI sounds trigger through the audio event system, not directly
- UI must never block the game thread
- Scalable text and colorblind modes are mandatory, not optional
- Test all screens at minimum and maximum supported resolutions
