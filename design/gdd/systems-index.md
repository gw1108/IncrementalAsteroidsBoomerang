# Systems Index: Incremental Asteroids Boomerang

> **Status**: Active
> **Last Updated**: 2026-04-24

---

## Overview

16 systems across five layers for a 2D incremental bullet-hell on Unity 6.3 LTS + URP 2D, WebGL-first. The core loop is a 1–3 minute combat run feeding a persistent grid skill tree. "The Tree IS the Game" — the skill tree (P1a/P1b) and stat-resolution layer (C6) are the architectural center of gravity. The boomerang (G3) is the mechanical identity and the Week-1 prototype gate. All gameplay systems use Unity's built-in `Update()` function directly — no custom fixed-timestep abstraction.

---

## Systems

### 1. C6 — Stat Resolver & Upgrade Aggregation

**Coding:** Aggregates stat contributions from all `IUpgradeSource` producers (skill tree, mods) into a frozen `GameStatsContext` struct consumed by gameplay systems. Recomputes on a dirty-flag basis so consumers always read a consistent snapshot. No gameplay system queries the tree or mods directly — they read `GameStatsContext` only.

**Art:** None required.

**Audio:** None required.

---

### 6. G1 — Player Ship Controller

**Coding:** Handles keyboard (WASD/arrow keys) movement, dash timing, and damage intake for the player ship. Reads movement and defense stats from `GameStatsContext`. Broadcasts health changes to the HUD via events; death triggers the run-end flow.

**Art:** A small ship sprite with a distinct silhouette readable at low zoom. An engine-thrust particle or frame animation conveys movement direction. A damage-flash shader tint communicates a hit without requiring a HUD read.

**Audio:** A looping engine hum, a hit sound on damage intake, and a death explosion are the minimum set.

---

### 7. E1 — Fuel Economy

**Coding:** Tracks a continuous fuel value that decays each tick and is extended by kills and asteroid mining, with diminishing returns on successive extensions within a run. Expiry triggers the run-end flow. Fuel rate multipliers are read from `GameStatsContext`.

**Art:** Fuel is represented by a bar or gauge on the HUD; no standalone world-space art is required.

**Audio:** A low-fuel warning tone and a fuel-depleted sound cue communicate critical state without requiring a HUD read.

---

### 8. E2 — Currency & XP Economy

**Coding:** Maintains persistent ledgers for credits and XP earned across runs, persisted via S1. Exposes deposit, withdrawal, and balance-query methods consumed by the skill tree and HUD. No run-scoped economy beyond fuel is tracked here.

**Art:** A small icon for each currency type (credits, XP) used in the HUD and skill tree UI.

**Audio:** A satisfying pickup chime plays on currency collection.

---

### 9. G3 — Boomerang Weapon ⚠ Week-1 Gate

**Coding:** Implements the boomerang as a kinematic projectile that travels an arc, auto-aims on throw toward the nearest enemy within a forward cone, and auto-returns after reaching max range or a hit trigger. All stats (speed, arc width, damage, pierce count) are read from `GameStatsContext`.

**Art:** A single boomerang sprite with a rotation animation, visually distinct from enemies and asteroids. A subtle arc-preview ghost or line shows the projected throw path. Hit sparks on impact are defined here as placeholder; final VFX are owned by V1.

**Audio:** A throw whoosh, a hit-impact crack, and a return-catch thwack are the three key sounds. The return catch must feel rewarding — it is the mechanical identity of the game.

---

### 10. P1a — Skill Tree Architecture ⚠ Design Gate

**Coding:** Implements the grid-graph data structure, prerequisite rule evaluation, and the `IUpgradeSource` contract that feeds C6. Nodes are `ScriptableObject` assets. This system owns purchase transaction logic but not the visual grid (U2) or node designs (P1b).

**Art:** No unique art assets; node icons and backgrounds are defined in P1b and rendered by U2.

**Audio:** A distinct node-purchase sound, separate from the currency-pickup chime.

---

### 11. G4 — Mod System

**Coding:** Defines a data-driven mod system where each archetype (Pierce, Chain) contributes stat deltas and ability tags through `IUpgradeSource` into C6. Mods attach to the boomerang per-run based on unlocked skill tree nodes. Per-run boundary rule: mods reshape tactics only — no permanent power gain invisible to the tree.

**Art:** Each mod archetype has a small icon displayed in the HUD and skill tree to indicate which mods are active.

**Audio:** Mod-specific hit sounds (e.g., a chain-bounce ricochet) differentiate mod effects during play.

---

### 12. P1b — Skill Tree Node Catalog ⚠ Design Gate

**Coding:** Defines 25–30 individual node `ScriptableObjects`, each specifying stat deltas, unlock prerequisites, and display metadata. Tier-1 ships ~15 nodes across 3 clusters; Tier-2 expands to 25–30. Every node must produce a visible gameplay effect at the moment of purchase — no invisible stat nodes.

**Art:** Each node requires an icon sprite and a category color. Locked, unlocked, and purchased visual states are defined here and rendered by U2.

**Audio:** No additional audio beyond the purchase sound defined in P1a.

---

### 13. G6 — Enemy System

**Coding:** Implements 3 enemy archetypes (charge, orbit, snipe) with movement AI, health, and damage behaviors. Stats scale via `GameStatsContext` so difficulty increases without code changes. Enemy death broadcasts an event consumed by E1 and E2.

**Art:** Three distinct enemy sprites, visually readable as separate threat types at a glance. Zone-2 variants use a color-shift shader rather than new sprites. A shared death-particle effect plays for all variants.

**Audio:** Each archetype has a distinct death sound. A spawn or alert sound for the sniper type.

---

### 14. G5 — Asteroid Mining

**Coding:** Manages asteroid objects with a crack-and-crumble lifecycle: each hit reduces integrity, spawning smaller debris at defined thresholds, until full destruction yields currency. Ore tier is assigned at spawn and determines yield. Yield multipliers are read from `GameStatsContext`.

**Art:** At least 3 size variants (large, medium, small fragment) with a crack overlay or sprite-swap to show damage state. A crumble particle burst on final destruction.

**Audio:** A distinct mineral-crack sound on hit, different from enemy hit sounds, and a mineral-drop chime on full destruction.

---

### 15. G8 — Boss Encounter

**Coding:** Implements a boss state machine with at least 3 phases driven by health thresholds, an attack-pattern library (projectile spray, charge, summon), and escalation triggers. Two distinct bosses are required at MVP. Boss health is broadcast to the HUD via event.

**Art:** Each boss requires a large, screen-readable sprite with a clear silhouette distinct from standard enemies. Phase transitions use a visible state change (color shift or new attachment sprites). Attack-telegraph sprites (warning lines, charge indicators) aid readability.

**Audio:** A boss-entry music sting that transitions into a combat track, a phase-transition impact sound, and a boss-death fanfare.

---

### 16. G7 — Wave & Spawn Director

**Coding:** Drives spawn timing, enemy mix, and density escalation over the run using configurable difficulty curves in `ScriptableObject` spawn tables. Reads current fuel level and kill count to modulate pacing. Triggers the boss encounter via G8 at defined thresholds.

**Art:** No unique art. Spawn-point flash indicators reuse particles from V1.

**Audio:** No unique audio; relies on enemy and boss systems for encounter sounds.

---

### 17. U1 — In-Run HUD

**Coding:** Displays fuel gauge, credit counter, active mod indicators, run timer, and boss health bar as UGUI elements. All data arrives through events from G1, E1, E2, and G8 — no per-frame polling. Updates must not allocate per frame.

**Art:** Fuel bar, currency icons, boss health bar, and mod-slot indicators each require distinct sprites or UI textures with a consistent visual language (bar shape, color coding).

**Audio:** No HUD-specific audio beyond the low-fuel warning defined in E1.

---

### 18. U2 — Skill Tree UI

**Coding:** Renders the persistent skill tree grid using UGUI with a custom `Graphic` subclass for connector lines between nodes. Handles node selection, purchase confirmation, and tooltip display. Reads tree state from P1a and currency balance from E2.

**Art:** A grid background texture, connector line art, and node frame sprites define the screen's visual identity. Node icons come from P1b. Selected-node highlight and locked-node desaturation states are required.

**Audio:** Node hover and purchase sounds as defined in P1a.

---

### 19. V1 — Juice Layer

**Coding:** Implements camera shake, hitstop freeze frames, and world-space VFX (hit sparks, currency pickups, boomerang trail, death explosions) through a centralized service consumed by G1, G3, G5, G6, and G8. Uses a pre-allocated string pool for floating damage numbers. A shared Weight Events Table coordinates which effects trigger per event and at what intensity, cross-referenced by A1.

**Art:** Hit spark sprites, floating damage number font, currency-pickup flash, boomerang trail particle, and enemy/asteroid death explosions. All effects must read clearly against the space background.

**Audio:** V1 owns screen-shake timing and hitstop framing; A1 owns the corresponding impact sounds. The Weight Events Table arbitrates per-event ownership.

---

### 20. A1 — Audio System

**Coding:** Provides an SFX pool for fire-and-forget sounds and a music manager for looped tracks with crossfade. Handles the WebGL AudioContext unlock requirement (silent buffer on first user interaction).

**Art:** None required.

**Audio:** Minimum at MVP: one in-run combat music track, one skill-tree ambient track, and the per-system SFX defined in each system above (weapon throws, enemy deaths, asteroid cracks, boss stings, UI feedback).

---

## Design Order

| # | System | Layer |
|---|--------|-------|
| 1 | C6 Stat Resolver ⚠ | Foundation |
| 2 | G1 Player Ship Controller | Core |
| 3 | E1 Fuel Economy | Core |
| 4 | E2 Currency & XP | Core |
| 5 | G3 Boomerang Weapon ⚠ Week-1 | Feature |
| 6 | P1a Skill Tree Architecture ⚠ | Feature |
| 7 | G4 Mod System | Feature |
| 8 | P1b Skill Tree Node Catalog ⚠ | Feature |
| 9 | G6 Enemy System | Feature |
| 10 | G5 Asteroid Mining | Feature |
| 11 | G8 Boss Encounter | Feature |
| 12 | G7 Wave & Spawn Director | Feature |
| 13 | U1 In-Run HUD | Presentation |
| 14 | U2 Skill Tree UI | Presentation |
| 15 | V1 Juice Layer | Presentation |
| 16 | A1 Audio System | Presentation |
