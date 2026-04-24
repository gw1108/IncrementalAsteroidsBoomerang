---
name: IAB Project Context
description: Core facts about Incremental Asteroids Boomerang — engine, platform, pillars, and input model — needed to inform all UX decisions
type: project
---

Game: Incremental Asteroids Boomerang (working title)
Engine: Unity 6 LTS, URP 2D, UGUI (not UI Toolkit — TD mandate)
Platform: WebGL desktop browser (primary); Steam PC post-MVP. No mobile/touch.
Input: Keyboard+mouse primary, gamepad secondary. New Input System.

Five locked pillars:
- P1 Multiplicative Dopamine
- P2 Positional Mastery, Not Aim Mastery
- P3 Read the Arc (boomerang visibility sacred)
- P4 Weighty Everything
- P5 The Tree IS the Game

Visual direction: Deep Forge. Angular/chamfered language throughout. No rounded corners. No drop shadows. No outer glows. Blueprint Gold, Spent Cyan, Ore Amber, Crimson (boss HP only).

Tree: hex-grid, 25-30 nodes MVP, center-out prereq-gated, UGUI with custom mesh connectors.
HUD: screen-space, sparse. Top-left stack (fuel bar, currency, XP). Boss HP bottom-center. Zone indicator top-right. Damage numbers world-space.
Typography: Rajdhani Medium/SemiBold + Share Tech Mono. Floor 9-10pt at 1080p.

**Why:** Informs every downstream UX spec, interaction pattern, and accessibility decision.
**How to apply:** Check every UX recommendation against these constraints before proposing. Do not propose touch, mobile, or UI Toolkit solutions.
