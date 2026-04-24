# Systems Index: Incremental Asteroids Boomerang

> **Status**: Draft — awaiting CD-SYSTEMS
> **Created**: 2026-04-23
> **Last Updated**: 2026-04-23
> **Source Concept**: [design/gdd/game-concept.md](./game-concept.md)

> **Technical Director Review (TD-SYSTEM-BOUNDARY)**: CONCERNS (adopted) 2026-04-23 — 5 structural items adopted; recorded under Architectural Constraints.
> **Producer Review (PR-SCOPE)**: OPTIMISTIC (adopted) 2026-04-23 — P1 split into P1a + P1b; 4 soft-cut candidates + G3/C6 design-gating recorded.
> **Creative Director Review (CD-SYSTEMS)**: CONCERNS (adopted) 2026-04-23 — 4 design-rule gaps recorded under Creative Director Notes; all addressable in downstream GDD authoring.

---

## Overview

This decomposition breaks the game concept into 30 systems across five dependency
layers. The game is an incremental bullet-hell built on Unity 6.3 LTS + URP 2D for
WebGL-first deployment. The core loop runs at two timescales — a 1–3 minute combat
run and a persistent hex-grid skill tree that the run feeds. Because "The Tree IS
the Game" (Pillar 4), the progression hub (P1a/P1b) and stat-resolution layer (C6)
are the architectural center of gravity; almost every gameplay system flows stats
through C6 rather than querying the tree directly. The weapon (G3 Boomerang) is
the mechanical identity and the Week-1 prototype gate — its feel fails or the
concept is invalidated.

Solo dev, Tier-2 target shipping product, realistic timeline 5–5.5 months.
28 systems in MVP; 1 in Vertical Slice (onboarding). No systems deferred to Alpha
or Full Vision — post-MVP work is content expansion and prestige (explicitly out
of scope per game concept).

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| C1 | Input & Control Abstraction | Core | MVP | Not Started | — | (none) |
| C2 | Object Pooling Framework | Core | MVP | Not Started | — | (none) |
| C3 | Fixed-Timestep Game Tick | Core | MVP | Not Started | — | (none) |
| C4 | Addressables Content Pipeline | Core | MVP | Not Started | — | (none) |
| S1 | Save System | Persistence | MVP | Not Started | — | (none) |
| C6 | Stat Resolver & Upgrade Aggregation *(inferred — TD)* | Core | MVP | **In Review** *(revised 2026-04-24; re-review or Accept pending)* | [design/gdd/c6-stat-resolver.md](./c6-stat-resolver.md) | C5 (lifecycle controller — Hard runtime dep); consumes `IUpgradeSource` producers (P1a, G4) |
| C5 | Scene & Mode Flow *(inferred)* | Core | MVP | Not Started | — | C1, C4, S1 |
| S2 | Settings & Preferences *(inferred)* | Persistence | MVP | Not Started | — | S1, C1 |
| G2 | Camera System *(inferred)* | Gameplay | MVP | Not Started | — | C3 |
| G1 | Player Ship Controller | Gameplay | MVP | Not Started | — | C1, C3, G2, C6 |
| E1 | Fuel Economy | Economy | MVP | Not Started | — | C3, C5 |
| E2 | Currency & XP Economy | Economy | MVP | Not Started | — | S1 |
| E3 | Run Stats Tracker *(inferred — TD)* | Economy | MVP | Not Started | — | C3 |
| G3 | Boomerang Weapon | Gameplay | MVP | **Designed** *(pending `/design-review`)* | [design/gdd/g3-boomerang-weapon.md](./g3-boomerang-weapon.md) | C2, C3, G1, G2, C6 |
| P1a | Skill Tree Architecture *(split from P1 per PR)* | Progression | MVP | Not Started | — | E2, S1; produces→C6 |
| G4 | Mod System | Gameplay | MVP | Not Started | — | G3, P1a; produces→C6 |
| P1b | Skill Tree Node Catalog *(split from P1 per PR)* | Progression | MVP | Not Started | — | P1a, G3, G4 |
| G6 | Enemy System | Gameplay | MVP | Not Started | — | C2, C3, G1, C6 |
| G5 | Asteroid Mining | Gameplay | MVP | Not Started | — | C2, G3, E2, C6 |
| G8 | Boss Encounter | Gameplay | MVP | Not Started | — | G6, E1 |
| P2 | Zone Progression | Progression | MVP | Not Started | — | G8, S1 |
| G7 | Wave & Spawn Director | Gameplay | MVP | Not Started | — | G6, G5, C5, E1, P2, C6 |
| M1 | Accessibility & Theme Service | UI | MVP | Not Started | — | S2, C1 |
| U1 | In-Run HUD *(inferred)* | UI | MVP | Not Started | — | E1, E2, G1, G8, E3, M1 |
| U2 | Skill Tree UI | UI | MVP | Not Started | — | P1a, P1b, E2, C1, S1, M1 |
| U3 | Main Menu & Meta UI *(inferred)* | UI | MVP | Not Started | — | C1, C5, S1, S2, P2, M1 |
| U4 | Run Results Screen *(inferred)* | UI | MVP | Not Started | — | C5, E2, E3, P1a, M1 |
| V1 | Juice Layer (VFX + Hitstop + Damage Numbers) | VFX/Feel | MVP | Not Started | — | C2, C3, G2 |
| A1 | Audio System | Audio | MVP | Not Started | — | C2, C4, C1 |
| M2 | Tutorial / Onboarding *(inferred)* | Meta | Vertical Slice | Not Started | — | C5, G1, G3, U1, E1 |

---

## Categories

| Category | Description | Systems in this index |
|----------|-------------|-----------------------|
| **Core** | Engine-level infrastructure and state machinery | C1, C2, C3, C4, C5, C6 |
| **Gameplay** | Systems that execute during a run | G1, G2, G3, G4, G5, G6, G7, G8 |
| **Economy** | Resource creation, flow, and tracking | E1, E2, E3 |
| **Progression** | Persistent player growth across runs | P1a, P1b, P2 |
| **Persistence** | Save state and settings | S1, S2 |
| **UI** | Player-facing information and interaction | M1, U1, U2, U3, U4 |
| **Audio** | Sound and music | A1 |
| **VFX/Feel** | Impact surfaces, juice, hitstop | V1 |
| **Meta** | Systems around the core loop | M2 |

Narrative, Networking, and Mobile UI categories are explicitly NOT present —
scoped out in `design/gdd/game-concept.md` anti-pillars.

---

## Priority Tiers

| Tier | Count | Milestone | Systems |
|------|-------|-----------|---------|
| **MVP** | 29 | Tier-1 shipping floor through Tier-2 "should ship" | All systems except M2 |
| **Vertical Slice** | 1 | Polished single-area demo | M2 Tutorial / Onboarding |
| **Alpha** | 0 | (content expansion only) | — |
| **Full Vision** | 0 | (Prestige/NG+/daily challenges explicitly out of MVP scope) | — |

**Why aggressive MVP**: Tier-2 from the concept is the target shipping product,
and almost every system has some MVP-level requirement. Tier bracketing here is
about *when the GDD is authored*, not about when features are implemented. Phased
implementation (e.g., M1's a11y features rolling in after the theme service) is
tracked inside each GDD's Acceptance Criteria, not at the index level.

**Tutorial deferred**: M2 is Vertical Slice because onboarding copy, pacing, and
the 20-second-first-run framing cannot be designed until the core-loop feel is
prototyped in WebGL. Designing M2 before G3 is validated is wasted work.

---

## Dependency Map

### Foundation Layer (no dependencies)

1. **C1 Input & Control Abstraction** — wraps New Input System; keyboard + gamepad; handles WebGL gamepad quirks
2. **C2 Object Pooling Framework** — `Pool<T>` with pre-warm; TD mandate #1
3. **C3 Fixed-Timestep Game Tick** — deterministic simulation decoupled from render; TD mandate #7
4. **C4 Addressables Content Pipeline** — TD mandate #4; from day 1
5. **S1 Save System** — `ISaveStore` + `BrowserIndexedDBStore`; JSON blob, schema-versioned, clipboard export/import; TD mandate #2
6. **C6 Stat Resolver** — registers `IUpgradeSource` producers (P1a, G4) and emits frozen `GameStatsContext`; decouples progression hub from consumers

### Core Layer (depends on Foundation)

1. **C5 Scene & Mode Flow** — depends on: C1, C4, S1 — main-menu → run → tree-shop → results state machine
2. **S2 Settings & Preferences** — depends on: S1, C1
3. **G2 Camera System** — depends on: C3 — follow-cam with shake hook
4. **G1 Player Ship Controller** — depends on: C1, C3, G2, C6 — kinematic WASD/gamepad + damage intake
5. **E1 Fuel Economy** — depends on: C3, C5 — continuous tick + diminishing-returns kill/mine extension
6. **E2 Currency & XP Economy** — depends on: S1
7. **E3 Run Stats Tracker** — depends on: C3 — counters for pierces, mined, damage dealt

### Feature Layer (depends on Core)

1. **G3 Boomerang Weapon** ⚠ — depends on: C2, C3, G1, G2, C6 — kinematic arc; auto-aim; auto-return; readable trajectory
2. **P1a Skill Tree Architecture** ⚠ — depends on: E2, S1; produces→C6 — hex-grid graph, prereq rules, effect-application contract
3. **G4 Mod System** — depends on: G3, P1a; produces→C6 — Pierce / Chain / Explode-on-return archetypes as data-driven stat deltas + ability tags
4. **P1b Skill Tree Node Catalog** ⚠ — depends on: P1a, G3, G4 — 25–30 individual node designs (Tier-1: ~15 nodes, 3 clusters; Tier-2: 25–30)
5. **G6 Enemy System** — depends on: C2, C3, G1, C6 — 6 variants (3 types × 2 zones re-tinted)
6. **G5 Asteroid Mining** — depends on: C2, G3, E2, C6 — ore tiers, crack-and-crumble lifecycle
7. **G8 Boss Encounter** — depends on: G6, E1 — boss state machine; in-field escalation; 2 distinct bosses
8. **P2 Zone Progression** — depends on: G8, S1 — boss-kill → next-zone unlock
9. **G7 Wave & Spawn Director** — depends on: G6, G5, C5, E1, P2, C6 — density escalation + zone-specific spawn tables

### Presentation Layer (depends on features)

1. **M1 Accessibility & Theme Service** — depends on: S2, C1 — central palette tokens + input rebinding + reduced-motion state, consumed by U1–U4
2. **U1 In-Run HUD** — depends on: E1, E2, G1, G8, E3, M1 — fuel gauge, currency, damage tier, run timer, boss health
3. **U2 Skill Tree UI** ⚠ — depends on: P1a, P1b, E2, C1, S1, M1 — UGUI hex grid with custom mesh-based connector Graphic (TD mandate #6)
4. **U3 Main Menu & Meta UI** — depends on: C1, C5, S1, S2, P2, M1 — start/continue/pause/settings/zone-select/save-management
5. **U4 Run Results Screen** — depends on: C5, E2, E3, P1a, M1 — between-run summary; 1→2 dopamine-moment staging
6. **V1 Juice Layer** — depends on: C2, C3, G2 — impact VFX, hitstop, camera shake, damage numbers (TD mandate #8 pre-allocated string pool)
7. **A1 Audio System** — depends on: C2, C4, C1 — SFX pool + music manager + WebGL AudioContext unlock

### Polish Layer (cross-cutting)

1. **M2 Tutorial / Onboarding** — depends on: C5, G1, G3, U1, E1 — first-run coaching; 1→2 damage moment framing

---

## Circular Dependencies

**None found.**

Near-miss worth documenting: **G4 Mod System depends on both G3 Boomerang and
P1a Skill Tree.** P1a does not depend on G4 — it only exposes "unlocked mods" as
queryable data via `IUpgradeSource`. This is a clean one-way graph.

TD-SYSTEM-BOUNDARY endorsed this by explicitly recommending the `IUpgradeSource`
→ C6 → `GameStatsContext` contract, which keeps P1a, G4, and future stat sources
as producers-only and all gameplay systems as consumers-only of the resolved
context.

---

## Recommended Design Order

Combined dependency sort + priority tier + producer's design-gate policy.
**G3 and C6 are "design-gate" systems** — downstream GDDs that consume their
contracts should wait for their approval.

| Order | System | Priority | Layer | Est. Effort | Notes |
|-------|--------|----------|-------|-------------|-------|
| 1 | C2 Object Pooling Framework | MVP | Foundation | S | Universal dep; design first |
| 2 | C3 Fixed-Timestep Tick | MVP | Foundation | S | TD mandate; pattern already explicit |
| 3 | C1 Input & Control | MVP | Foundation | S | Required by C5 and all UIs |
| 4 | C4 Addressables Pipeline | MVP | Foundation | M | Content taxonomy + load conventions |
| 5 | S1 Save System | MVP | Foundation | M | Schema + versioning + clipboard |
| 6 | **C6 Stat Resolver** ⚠ design-gate | MVP | Foundation | M | API contract; pollutes downstream if wrong |
| 7 | C5 Scene & Mode Flow | MVP | Core | S | Orchestrator |
| 8 | S2 Settings | MVP | Core | S | Minimum volume + rebind |
| 9 | G2 Camera System | MVP | Core | S | Follow-cam + shake hook |
| 10 | G1 Player Ship Controller | MVP | Core | M | Movement + damage intake |
| 11 | E1 Fuel Economy | MVP | Core | M | Formulas + diminishing returns |
| 12 | E2 Currency & XP | MVP | Core | S | Ledger |
| 13 | E3 Run Stats Tracker | MVP | Core | S | Counters |
| 14 | **G3 Boomerang Weapon** ⚠ design-gate, Week-1 | MVP | Feature | L | HIGHEST RISK — Week-1 prototype gate |
| 15 | P1a Skill Tree Architecture ⚠ | MVP | Feature | L | Pillar 4 hub; must follow C6 |
| 16 | G4 Mod System | MVP | Feature | M | Pierce / Chain / Explode-on-return |
| 17 | P1b Skill Tree Node Catalog ⚠ | MVP | Feature | L | 25–30 nodes; follows G3 + G4 so stat deltas are concrete |
| 18 | G6 Enemy System | MVP | Feature | M | 3 types × 2 zones = 6 variants |
| 19 | G5 Asteroid Mining | MVP | Feature | M | Ore tiers + yield |
| 20 | G8 Boss Encounter | MVP | Feature | M | 2 distinct bosses |
| 21 | P2 Zone Progression | MVP | Feature | S | Unlock state + 2-zone content |
| 22 | G7 Wave & Spawn Director | MVP | Feature | M | Density escalation curves |
| 23 | M1 Accessibility & Theme Service | MVP | Presentation | M | Must exist before U1 hooks up |
| 24 | U1 In-Run HUD | MVP | Presentation | M | Tier-1 weight surfaces visible |
| 25 | **U2 Skill Tree UI** ⚠ | MVP | Presentation | L | UGUI custom mesh connector Graphic |
| 26 | U3 Main Menu & Meta UI | MVP | Presentation | M | Full meta UI surface |
| 27 | U4 Run Results Screen | MVP | Presentation | S | Dopamine staging |
| 28 | V1 Juice Layer | MVP | Presentation | M | Tier-1 weight surfaces only at MVP |
| 29 | A1 Audio System | MVP | Presentation | M | WebGL AudioContext + SFX pool + 2 music tracks |
| 30 | M2 Tutorial / Onboarding | VS | Polish | S | After core-loop feel validated |

**Effort totals**: 9× S + 15× M + 6× L ≈ ~55 focused design sessions for full
30-system authoring. Producer estimates 11–13 weeks of design-phase work at solo
pace (see Soft-cut Triggers below).

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| **G3 Boomerang Weapon** *(pillar-validation gate)* | Technical + Design + Scope | Week-1 prototype gate from TD-FEASIBILITY; entire game rests on feel; Pillar 3 "Read the Arc" lives here. **G3 is a gate, not just a system** — if Week-1 feel target fails, downstream consumers G4 (mod archetypes), V1 (impact juice event targets), and U1 (reticle/HUD feedback if any) are invalidated and require rescope before their GDDs are authored. | Prototype in **WebGL browser build** Week 1 per concept's Production Guidance; do not defer to later sprint. **If G3 Week-1 prototype fails**, escalate to concept-level revision — do not author G4, V1, U1 GDDs against an invalidated weapon feel. |
| **U2 Skill Tree UI** | Technical | UGUI custom mesh connector at PoE fidelity within 512 MB WebGL budget; PR-SCOPE flagged as 3–4 week hidden cost | Spike-prototype the connector Graphic in parallel with U2 GDD; TD mandate #6 is non-negotiable |
| **P1a + P1b Skill Tree** | Design | Pillar 1 requires each early node to deliver a felt power jump; 25–30 distinct "feel-jump" nodes is the known hard problem | Author P1a before G4; author P1b only after G3 + G4 so stat deltas are concrete; playtest-tune per concept's cut-order |
| **C6 Stat Resolver** | Architectural | Wrong contract here pollutes all gameplay GDDs downstream | Design-gate per PR — other GDDs that consume `GameStatsContext` should not be written until C6 is approved |
| **E1 Fuel Economy** | Design | Tight pacing curve (20s first run → 3min max); diminishing-returns math is subtle | Formulas tested against 3–4 reference runs before lock; expect playtest iteration |
| **A1 Audio System** | Technical | WebGL AudioContext unlock + Safari quirks from TD-FEASIBILITY | Verify on Safari during Month-3 "WebGL Stability Week" per concept's Production Guidance |
| **G4 Mod System** | Design | 3 mod archetypes must feel distinct (Pierce / Chain / Explode-on-return); Pillar 3 constrains how many can stack simultaneously | Design after G3 prototype confirms arc-reading feel; max-stack count is a derived constraint from Pillar 3 watch-item |

---

## Architectural Constraints

Decisions adopted from TD-SYSTEM-BOUNDARY (CONCERNS, 2026-04-23). These bind all
downstream GDDs and become inputs to `/create-architecture`.

1. **`IUpgradeSource` → C6 → `GameStatsContext` contract.** P1a and G4 are the
   only stat *producers* at MVP. C6 aggregates them into a frozen immutable
   `GameStatsContext` per run (or per dirty-flag recompute). G1, G3, G5, G6, G7
   are *consumers only* — they read `GameStatsContext`, never P1a or G4 directly.
   Future run-scoped modifiers (boss debuffs, zone effects) plug in as additional
   `IUpgradeSource` producers without touching AP2 (anti-pillar "no per-run power
   upgrades invisible to the tree") because the source and resolution are
   separated.

2. **No project-wide event bus.** Cross-system signaling uses concrete C#
   interfaces (e.g., `ISaveStore.SaveRequested`, `IWaveDirector.WaveCleared`).
   With 27+ gameplay systems and a solo dev, global pub/sub invites exactly the
   implicit-shared-state problem the decomposition avoids. Revisit at Vertical
   Slice if cross-system signaling pain emerges.

3. **M1 owns the central theme/palette service.** U1, U2, U3, U4 consume palette
   tokens, colorblind variants, reduced-motion state, and input-rebind mappings
   from M1. No UI system implements its own a11y logic.

4. **P2 Zone Progression is read by G7 Wave & Spawn Director, not G8 Boss.** The
   boss encounter logic itself does not need zone progression state; the spawn
   director does. G8 is a leaf of G7 in the dependency graph.

5. **Data validation lives in C4 Addressables pipeline.** ScriptableObject
   `OnValidate` checks enforce content integrity at import time. This is not a
   separate system — document it in the C4 GDD.

---

## Soft-cut Triggers

Adopted from PR-SCOPE (OPTIMISTIC, 2026-04-23). The 30-system decomposition is
well-structured but design-phase consumes ~25–30% of the Tier-2 timeline budget
before implementation begins. If design phase crosses **week 10**, pre-commit to
these cuts in this order:

1. **M1 Accessibility** — ship MVP with hardcoded palette tokens; extract the
   theme service in Vertical Slice.
2. **E3 Run Stats Tracker** — minimum counters only; rich stats post-MVP.
3. **U4 Run Results Screen** — minimal "you died / you won" panel; dopamine-moment
   framing as a VS enhancement.
4. **G8 Boss Encounter — zone-2 boss only** ⚠ **requires CD-SYSTEMS escalation,
   not unilateral cut.** Per CD-SYSTEMS review, Zone-2 boss is the emotional
   climax of the "desperate miner → unstoppable miner" core fantasy arc — it is
   the first "I am now unstoppable" proof point. Cutting it forces Zone-1 boss
   to carry the full unstoppable feeling at a power level too early in the curve.
   If design phase runs long, present the cut proposal to creative-director for
   fantasy-arc rescope before taking it.

**Timeline baseline recommendation (PR)**: Tier-2 realistic at 5.5 months = 12
weeks design + 12 weeks implementation + 4 weeks polish/buffer. This updates the
concept's 4–5 month estimate based on the detailed system count.

**Systems to protect (PR)**: G3, P1a, P1b, U2, G4, G6, E1, S1, C6. Do not cut
these under any time pressure.

---

## Design-Gate Systems

Per PR-SCOPE: these systems must be approved before downstream GDDs that consume
their contracts are written. Failing to gate these upstream will cause
re-authoring cost when the contract shape changes.

| System | Downstream consumers |
|--------|---------------------|
| **C6 Stat Resolver** | G1, G3, G5, G6, G7, P1a, G4 (all stat-touching gameplay) |
| **G3 Boomerang Weapon** | P1a, P1b, G4, G5, G6 (all damage-touching gameplay) |

Author C6 and G3 GDDs, run `/design-review` and CD-GDD-ALIGN on each, confirm
approval, then begin any GDD in their consumer list.

---

## Creative Director Notes (CD-SYSTEMS)

Non-blocking design-rule gaps from CD-SYSTEMS (CONCERNS, 2026-04-23). None restructure
the decomposition; all must surface as named constraints inside the specified GDDs
when those GDDs are authored. Record verbatim in the listed GDDs.

1. **P4 weight-surface ownership must be arbitrated between V1 and A1.** Pillar 4
   "Weighty Everything" is currently co-owned by V1 Juice Layer and A1 Audio
   System with no per-event budget arbitration. Without a shared spec, V1 and A1
   will drift on events like "boomerang catch" (which layer leads? who owns the
   shake-ms-vs-sub-bass-ms budget?).
   → **Required artifact**: a shared **"Weight Events Table"** cross-referenced by
     both V1 and A1 GDDs, OR explicit designation of V1 as primary with A1 as a
     subordinate contract. This is CD-PILLARS watch item #3 made concrete.

2. **G4 Mod System GDD must contain a named "Per-run boundary" section** stating
   what mods CAN do (per-run tactical reshape) vs CANNOT do (permanent power gain
   invisible to the tree). This is CD-PILLARS watch item #4 (AP2 clarity) given
   a system home — without it, the anti-pillar violation surfaces in playtest,
   not design.

3. **P1b Node Catalog GDD must contain a named "Node Feel Threshold" section**
   converting Pillar 1 ("every early upgrade must significantly change how the
   game plays") into a testable gate. Suggested rule: *"No node ships if its
   effect is invisible to the player at the moment of purchase."* This is
   CD-PILLARS watch item #1 (mid-tree hollowness) given a design-rule home.

4. **G3 Boomerang Weapon is a pillar-validation gate, not just a system** — see
   annotation in High-Risk Systems table. Fallback scope for G4/V1/U1 is named
   there.

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 30 |
| Design docs started | 2 |
| Design docs reviewed | 1 |
| Design docs approved | 0 |
| MVP systems designed | 2 / 29 |
| Vertical Slice systems designed | 0 / 1 |

**G3 Boomerang Weapon** — Designed 2026-04-23; CD-GDD-ALIGN CONCERNS adopted; pending independent `/design-review` in a fresh session.

**C6 Stat Resolver** — Designed 2026-04-24; CD-GDD-ALIGN APPROVED; pending independent `/design-review` in a fresh session.

---

## Next Steps

- [ ] CD-SYSTEMS creative-director review of this index before GDD authoring begins
- [ ] Design MVP-tier Foundation systems first (`/design-system c2-object-pooling` or equivalent slug per file-naming convention)
- [ ] Run `/design-review design/gdd/[slug].md` on each completed GDD
- [ ] Run `/review-all-gdds` after all MVP GDDs are authored
- [ ] Run `/gate-check concept-to-architecture` when MVP GDDs complete
- [ ] Prototype G3 Boomerang Weapon in WebGL browser build Week 1 per concept's Production Guidance — **this is a hard gate**
