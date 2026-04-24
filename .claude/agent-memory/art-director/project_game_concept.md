---
name: Game Concept — Incremental Asteroids Boomerang
description: Core game concept, pillars, platform constraints, and reference games for visual direction
type: project
---

Incremental bullet hell, top-down 2D space. Ship auto-throws a boomerang at nearest enemy; punches through on a readable arc and auto-returns. Mine asteroids, fight pirates until fuel runs out. Spend currency on a PoE-style persistent passive skill tree between runs.

**Core fantasy**: Desperate miner becoming the unstoppable miner. Multiplicative dopamine — felt moment a damage number doubles.

**Target platform**: Unity 2D, WebGL-deployable.

**Why:** WebGL first-load constraint — tight texture memory, no AAA-scale particles, aggressive atlas packing, sprite-based VFX preferred over shader-heavy postfx. Cold-load under 30 seconds.

**Reference games**:
- Astro Prospector — direct genre ref, top-down space 2D, sprite-based
- Path of Exile — dark fantasy, moody, impactful hit feedback, grim palette
- A Game About Feeding A Black Hole — clean minimalist 2D, strong silhouette readability, satisfying scale feedback

**5 Pillars (visual implications)**:
- P1 Multiplicative Dopamine: ship/boomerang must visibly evolve as tree fills
- P2 Positional Mastery: enemies/hazards spatially readable at a glance
- P3 Read the Arc: boomerang trail/silhouette/contrast are sacred — nothing may obscure its arc
- P4 Weighty Everything: VFX budget goes to impact moments (hitstop, debris, bursts) not ambient prettiness
- P5 The Tree IS the Game: skill tree must be a beautiful visual artifact the player wants to look at

**Anti-pillars**: No narrative/NPCs/portraits in MVP. No manual aim (no reticle). No chaotic random weapon behavior — visuals reinforce determinism.

**Scale**: ~25-30 skill nodes, 3-4 boomerang mod archetypes, several asteroid types, several enemy/boss types, 1 ship base with evolving visual.

**How to apply:** All visual proposals must be evaluated against WebGL constraint first. Skill tree is a MAJOR visual artifact — treat it as a foreground design problem, not an afterthought. Boomerang arc readability is non-negotiable.
