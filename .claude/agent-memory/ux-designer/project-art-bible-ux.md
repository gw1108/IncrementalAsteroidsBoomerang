---
name: Art Bible Section 7 UX Alignment Positions
description: UX positions and conflict flags established during the Section 7 (UI/HUD) alignment pass on the IAB art bible
type: project
---

Session: 2026-04-23 — Art bible Section 7 UX alignment pass.

Key UX positions established:

1. CONFLICT FLAGGED: 9-10pt font floor is below safe threshold for Unity WebGL's lossier text rendering. UX recommends 12pt functional floor with 10pt hard minimum only for secondary labels. AD position is 9-10pt.

2. CONFLICT FLAGGED: AD rejects all persistent looping animation in-run. UX requires a reduced-motion-OFF persistent visual for fuel-critical state (below 25%) for low-vision accessibility. One-time pulse alone fails WCAG 2.1 SC 1.4.1 (use of color). Proposed resolution: slow 2s breathing pulse toggled ON only when Reduced Motion = OFF and fuel < 25%. Not cosmetic — functional accessibility.

3. CONFLICT FLAGGED: Blueprint Gold at 40% opacity focus ring on HUD Panel Dark (#1A1612) likely fails 3:1 contrast ratio minimum for non-text UI (WCAG 1.4.11). UX recommends 70-80% opacity or white-tinted variant for focus ring. AD position is 40% opacity.

4. Tab order for 25-30 hex nodes in irregular layout: UX recommends logical spatial grouping (left-to-right rows within each radial tier, tier-by-tier outward from root). Not a conflict — AD did not specify. Requires coordination with ui-programmer for UGUI SelectionNavigation override.

5. Tooltip model: Mouse hover = tooltip after 400ms delay. Gamepad = face button (South/A) toggles tooltip for focused node; second press or directional input dismisses. Do NOT use hold-to-confirm — creates input conflict with purchase confirm flow.

6. Boss HP bar at bottom-center: UX concern that eyes are drawn downward during boss phase when positional reading is most critical. Flagged for user decision — not a hard conflict, but a placement risk worth user awareness.

7. Accessibility-mandated additions missing from AD visual direction:
   - Reduced Motion toggle (mandatory for WCAG 2.1 SC 2.3.3 AAA; recommended for AA indie baseline)
   - UI Scale setting (recommended: 75/100/125/150% steps)
   - High Contrast toggle (recommended but deferrable to post-MVP if clearly documented)
   - Focus trap on WebGL pause menu (technical requirement, not aesthetic)

**Why:** These are standing UX positions for any downstream work on HUD, menus, skill tree, or accessibility systems.
**How to apply:** Reference when authoring UX specs, reviewing UI programmer work, or when AD requests changes to focus/font/animation rules.
