# Game Concept: Incremental Asteroids Boomerang (working title)

*Created: 2026-04-23*
*Status: Draft — Ready for /setup-engine and downstream GDD authoring*

---

## Elevator Pitch

> It's an **incremental bullet hell** where your ship auto-throws a weighty **boomerang** projectile that punches through asteroids and enemies on a readable arc and auto-returns — run until fuel runs out, then spend earned currency on a persistent PoE-style **skill-tree grid** to make the next run multiplicatively stronger, until a zone boss falls and the next zone unlocks.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | Incremental bullet hell, Action |
| **Platform** | Desktop browser (WebGL primary); Steam PC port post-MVP |
| **Target Audience** | "Optimizing Mastery Player" — Achiever-Explorer crossover (see Player Profile) |
| **Player Count** | Single-player |
| **Session Length** | 30–60 minutes (composed of 1–3 minute runs punctuated by skill-tree purchases) |
| **Monetization** | Free browser-play; Premium Steam port (pricing TBD post-MVP) |
| **Estimated Scope** | Medium (Tier 2 MVP: 4–5 months solo, professional-background dev) |
| **Comparable Titles** | Astro Prospector, Vampire Survivors, Brotato, Path of Exile (progression), Dome Keeper |

---

## Core Fantasy

**The desperate miner becoming the unstoppable miner.**

The player starts vulnerable — 20 seconds of fuel, 1.3 asteroids destroyed, a single point of score. The first upgrade doubles damage from 1 to 2, and suddenly the same moves that barely worked now devastate the field. That felt shift — **the moment a number doubles and the game re-teaches the player what it can do** — is the emotional center. It is the PoE loot-treadmill collapsed into 1–3 minute cycles.

The player is not piloting a ship. The player is **building a weapon**, node by node, until a boomerang they forged outperforms the hostile cosmos it was made to survive.

---

## Unique Hook

**Like Astro Prospector, AND ALSO like Hades' Stygian weapons fused with Path of Exile's passive tree.**

Three things make this differentiated from its nearest neighbors:

1. **A single weighty boomerang instead of proximity lasers.** Auto-aim + auto-return means skill expression lives entirely in positioning, not aiming. Mod archetypes (Pierce, Chain, Explode-on-return) stack into behavior variants on a single readable weapon.
2. **A persistent skill-tree grid instead of a flat upgrade shop.** Center-out, prereq-gated, hex-tiled nodes — the tree is a map of *who the player is becoming*, not a menu of stat purchases.
3. **Multiplicative-dopamine pacing instead of flat percentage scaling.** Early-game upgrades deliver threshold jumps (1→2 damage) that the player feels instantly. Later-game scales flatter as the curve matures.

The hook is explainable in one sentence: *"Vampire-Survivors-tempo runs on a PoE-shape tree, with a single boomerang you re-invent every time you unlock a new node."*

---

## Visual Identity Anchor

**Direction: Deep Forge** (with electric-cyan boomerang variant)

### One-line visual rule

> **You are building a weapon, not piloting a ship.**

Every visual decision is evaluated against this. If an art choice makes the player feel like a passenger in a vehicle rather than an artisan tuning a tool, it's wrong for this game.

### Mood and atmosphere

Dark fantasy translated to space, without apology. The void is a forge interior — warm undertone, not cold science-fiction blue. Asteroids are ore chunks, not celestial bodies. Enemies are obstacles to smelting. The skill tree is a craftsman's blueprint. There is beauty here, but it's the beauty of a mineral cross-section or a workbench schematic — pattern and density, not spectacle.

### Shape language

Irregular, heavy, forged-looking.
- **Asteroids**: angular multi-faceted silhouettes with visible weight
- **Boomerang**: dense angular wedge, blunt-tipped, weighted toward the striking edge — no grace, all impact. Visually evolves as tree fills (more layered geometry per archetype unlock)
- **Ship**: compact, low, industrial — tool-like, not vehicle-like
- **Enemies**: asymmetrical, spiky, improvised-looking
- **Skill tree**: blueprint-schematic grid. Hexagonal nodes; grid-aligned layout; connectors drawn like wiring diagrams or mineral-vein lines

### Color philosophy

**Every saturated color has a single reserved meaning.** If it can't be categorized into one of these, it doesn't belong in the palette.

| Color | Meaning | Usage |
| ---- | ---- | ---- |
| Near-black with **warm undertone** | Background (forge-interior) | Space, starfield layer |
| **Electric cyan** | The boomerang — owned exclusively | Weapon body, full trail, return arc. Shifts from dull cyan (low tier) toward sharp cyan with white-hot accents as tree fills. **Zero conflict with ore, enemies, or background.** |
| **Steel grey + rust accent** | Asteroids | Mineable-chunk aesthetic |
| **Dark crimson** | Enemies, pirates, enemy projectiles | Faction signal |
| **Molten amber** | Ore drops, currency, XP pickups | "Things that pay you" |
| **White-yellow flash** | Impact only | Appears on hit, <0.2s. Highest saturation in the game, reserved for contact moments. |
| **Blueprint aged-parchment + gold schematic** | Skill tree background + nodes | Activated nodes light up in molten amber. Tree reads as a document, not a UI. |

### Supporting principles (each with a design test)

1. **Hue is reserved for meaning.**
   *Test*: If adding a new entity, does it fit an existing hue role? If not, don't add the entity until a hue role can be defined.

2. **Impact has monopoly on the highest saturation.**
   *Test*: If VFX is proposed for something other than a contact event, push back. Ambient world stays muted.

3. **Tree aesthetic = geometric blueprint at WebGL-friendly fidelity.**
   *Test*: If a tree UI decision would require a high-resolution texture, find a drawn-geometry alternative first.

This section is the foundation of the forthcoming art bible and the gate all asset production must pass.

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Challenge** (obstacle course, mastery) | **1 (primary)** | Positional mastery in bullet hell; boss gauntlets; tree planning |
| **Expression** (self-expression, creativity) | **2** | Mod archetype stacking; tree-path choice; build theorycraft |
| **Sensation** (sensory pleasure) | **3** | Weighty boomerang impact feedback; hitstop; audio/visual juice on contact |
| **Discovery** (exploration, secrets) | **4** | Finding build synergies; unlocking mod archetypes; revealing tree clusters |
| **Submission** (relaxation, comfort zone) | **5** | Short runs induce light flow state; "one more run" cadence |
| **Fantasy** (make-believe, role-playing) | 6 | Mild — space prospector / weapon-artisan framing |
| **Narrative** (drama, story arc) | N/A | Explicitly excluded via AP5 |
| **Fellowship** (social connection) | N/A | Single-player, no multiplayer |

### Key Dynamics (Emergent player behaviors)

- Players will **theorycraft builds** by planning routes through the tree before purchasing
- Players will **route through asteroid fields** to maximize chain/pierce hits
- Players will **balance risk vs farm** — engage boss early with weaker tree, or farm fuel/currency first
- Players will **develop muscle memory for positional micro-adjustments** under boomerang return windows
- Players will **mix-and-match mod archetypes** within prereq constraints to find identity for a given run

### Core Mechanics (Systems we build)

1. **Auto-aim, auto-return kinematic boomerang** with modular on-contact behaviors (pierce, chain, explode-on-return) — scripted motion, not physics simulation
2. **Fuel-limited run economy** with kill/mine extension
3. **Persistent hex-grid skill tree** (center-out, prereq-gated, 25–30 nodes MVP)
4. **In-field boss spawn pattern** (escalating waves + boss-on-field, no separate arena)
5. **Asteroid mining** with distinct ore tiers and mining yield

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** | Meta-layer build choice (which tree node, which mod archetype); in-run positional decisions | **Core (meta)** / Supporting (in-run) |
| **Competence** | Threshold-based power jumps (1→2 damage re-teaches the game); visible tree fill; positional skill growth | **Core** |
| **Relatedness** | No NPCs, no multiplayer, no narrative connection by design | **Minimal** |

### Player Type Appeal (Bartle Taxonomy)

- [x] **Achievers** (primary) — Progression-driven. Measuring power thresholds. Filling the tree. Clearing bosses.
- [x] **Explorers** (secondary) — Discovering mod synergies, exploring tree paths, theorycrafting builds.
- [ ] **Killers / Competitors** — Not served. Out of scope.
- [ ] **Socializers** — Not served. Out of scope.

### Flow State Design

- **Onboarding curve**: First run is 20 seconds, produces ~1 asteroid destroyed and earns the first tree node. The 1→2 damage threshold jump on purchase is the anchor dopamine moment. The second run is measurably different — the feedback loop is established within 60 seconds of starting.
- **Difficulty scaling**: Enemy density scales per zone; tree power scales per node purchased. Boss fights serve as skill checkpoints.
- **Feedback clarity**: Tier thresholds (integer damage jumps), tactile hit feedback (hitstop + VFX + audio), visible tree fill, explicit currency/XP counters.
- **Recovery from failure**: Runs end in 1–3 minutes; death is instant return-to-shop. No progress loss — all tree purchases are permanent. Failure is educational (what does the next node unlock?), not punishing.

---

## Core Loop

### Moment-to-Moment (30 seconds)

Player pilots the ship with WASD. The boomerang auto-throws at the nearest enemy on a short cooldown, travels on a visible weighty arc, punches through targets (pierce mods), triggers mod behaviors on contact (chain, explode-on-return), and auto-returns on a fixed arc. During the flight-and-return window, the player **reads enemy density and repositions** — the returning boomerang should chain through the next cluster on its way back. Mining asteroids yields currency; killing enemies yields XP/currency. Fuel ticks down continuously.

**What makes it satisfying in isolation**:
- **Audio**: heavy "thunk" on asteroid crack; whistle-arc during flight; low chime on catch
- **Visual juice**: micro-hitstop on impact; debris particle burst; cyan trail that widens with speed
- **Timing**: the flight-return pause creates read-and-reposition windows — this IS the tactical layer
- **Depth**: build variety comes from mod-on-contact behaviors, not from aim

### Short-Term (5–15 minutes) — a single run

A run begins with fuel starting to tick down. Player engages asteroids and enemies; mining and kills grant small fuel extensions (with diminishing returns to prevent infinite-run exploits). Enemy density and aggression escalate. At a threshold (time-based or fuel-based), a boss entity spawns in the field. Killing the boss marks the zone cleared; fuel running out ends the run.

**Decisions at this level**:
- Which asteroids to prioritize (yield vs distance)
- When to engage enemies vs flee
- Positioning to maximize chain-pierce hits

### Session-Level (30–120 minutes)

Run → persistent skill-tree shop → run → shop → … → boss defeated → next zone unlocked. Each shop visit is the multiplicative-dopamine moment: a node purchase meaningfully changes the next run (per P1).

**Natural stopping point**: after defeating a boss. **Reason to come back**: the next tree cluster is visible but locked.

### Long-Term Progression

- Fill more tree nodes → more build identity
- Unlock new mod archetypes (MVP: 3; Stretch: 4)
- Clear successive zone bosses
- *Tier 3 stretch*: Prestige / NG+ mechanic (reset tree for a meta-multiplier)

### Retention Hooks

- **Investment**: Accumulated tree purchases are permanent. Fill progress visible at a glance.
- **Social**: None in MVP (AP5).
- **Mastery**: Positional micro-optimization. Boss routing.

---

## Game Pillars

### Pillar 1: Multiplicative Dopamine

> Every early-game upgrade must significantly change how the game plays by increasing player power. Later upgrades can change player power less.

*Design test*: If debating two early-game nodes — "+5% damage" vs "+1 damage tier (1→2)" — the tier wins. The first ~10 tree purchases must each produce a player-felt power jump. Deep-tree nodes are allowed to trend smaller.

*Watch item (CD feedback)*: This pillar trades "visible vs behavioral" discipline for curve-honesty. If mid-tree nodes start feeling hollow in playtesting, revisit — may need a secondary clause about decision-change, not just power-increase.

### Pillar 2: Mastery in Where the Player Moves, Not Aiming

> Skill expression lives in positioning, not aiming. Auto-aim frees the player to move.

*Design test*: If debating "add manual aim" vs "add dash" → **dash**.

### Pillar 3: Weighty Everything

> Every hit has perceivable weight: audio, hitstop, particles, camera response. The game is sold on tactile feel before it's sold on numbers.

*Design test*: If debating "more enemies" vs "more impact VFX budget" → **VFX**.

*Watch item (CD/TD feedback)*: This is the most expensive pillar to execute well. Define **Tier-1 weight surfaces** for MVP (boomerang impact + enemy deaths + asteroid cracks are the mandatory ones; upgrade-purchase juice is optional for Tier 1, should-have for Tier 2).

### Pillar 4: The Tree IS the Game

> Progression is spatial navigation of a persistent skill tree, not accumulation of stats. Looking at the tree should tell the player who they are becoming.

*Design test*: If debating "random upgrade offers" vs "prereq tree branches" → **tree**. No temporary per-run buffs that aren't visible on the tree.

*Watch item (CD feedback)*: AP2 forbids per-run *power* upgrades. It does NOT forbid per-run *tactical* choices (zone routing, boss engagement order, which asteroids to prioritize). When the tree GDD is authored, clarify this boundary explicitly.

### Anti-Pillars (What This Game Is NOT)

- **NOT manual aim** — violates P2 (positional mastery). No reticle, no aim input, no player-driven targeting.
- **NOT per-run temporary power upgrades** — violates P4 (tree is the game). No in-run boons, no temporary buffs invisible to the tree. Per-run *tactical* choice is still open.
- **NOT flat percentage upgrades** (especially early-game) — violates P1. Thresholds feel; percentages don't.
- **NOT random or chaotic boomerang behaviors** — violates P1. No randomized arcs, no RNG targeting. Deterministic auto-aim only.
- **NOT narrative, NPCs, or dialogue in MVP** — out of timeline scope; narrative is not in the top 5 MDA aesthetics.
- **(AP6 considered and not adopted)** — "No runs longer than 5 min" was proposed as a session-length cap. Deferred to the fuel-economy GDD as a tuning target rather than a pillar-level wall. Re-evaluate if Act 2 content pushes run length.

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently | Why It Matters |
| ---- | ---- | ---- | ---- |
| **Astro Prospector** | Direct genre reference: auto-fire weapon + shop-per-run + fuel-gated mining | Boomerang instead of proximity laser; persistent skill-tree grid instead of flat shop; Deep Forge palette | Validates the market exists; our differentiators are explicit |
| **Path of Exile** | Persistent passive skill tree; threshold-based power jumps; build theorycraft culture | Compressed to 1–3 min runs; bullet-hell combat layer; much smaller tree (~25–30 nodes MVP) | Proves players will engage deeply with a spatial tree if it's well-designed |
| **Vampire Survivors** | Auto-fire removes the aim-tax; player focuses on positioning; short addictive runs | Single weapon with modular behaviors (not 8 weapons with chaotic overlap); readability sacred (P2) | Proves the auto-fire + incremental progression pattern at mass scale |
| **Hades** | Mod-stacking for build identity; satisfying single-weapon depth; hitstop and audio feel | Persistent tree (not per-run boons); no narrative layer; 2D top-down (not 2.5D action) | Validates that deep mod archetypes feel fresh even with one weapon |
| **A Game About Feeding A Black Hole** | Clean readable 2D silhouettes; tactile scaling; visual restraint | Focused on single-run ship rather than monotonic "black hole grows you" loop | Taste anchor — readability + minimalism without feeling empty |
| **Dome Keeper** | Mine-and-defend loop with meta progression; solo-dev structural comparable | Boomerang-weapon identity; skill-tree-centric meta instead of linear upgrade shop | Closest proof-point that this scope is solo-achievable |

**Non-game inspirations**:
- Geological cross-section diagrams (visual anchor for tree + asteroids)
- Blacksmithing / forge aesthetics (Deep Forge palette + "building a weapon" fantasy)
- Ore assay maps (skill tree as schematic document rather than menu)
- Old-school arcade shmup pacing (short intense engagements, not sessions)

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Gaming experience** | Mid-core to hardcore; comfortable with incremental progression systems and/or bullet hell |
| **Time availability** | 30–60 minute sessions during evenings; 5-minute coffee-break pickup play works due to short run length |
| **Platform preference** | Desktop browser primary (free-play, itch.io / portal distribution); |
| **Current games they play** | Path of Exile, Vampire Survivors, Brotato, Astro Prospector, Hades, Slay the Spire |
| **What they're looking for** | Instant-feedback dopamine of incrementals fused with skill-expression of action roguelikes; theorycraft depth; short sessions that still feel meaningful |
| **What would turn them away** | slow runs; manual-aim requirements; narrative filler; |

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Recommended Engine** | **Unity 6 LTS** — confirmed; locked in by user preference |
| **Rendering** | **URP 2D Renderer** (for 2D lights + 2D post-processing + modern sprite shaders). Ruthless feature stripping required for WebGL bundle size. |
| **Input** | **New Input System** — keyboard only (WASD and arrow keys) |
| **Physics** | **Physics 2D (Box2D)** for collision detection only. Boomerang motion is **kinematic scripted**, not Rigidbody2D. Strip Physics 3D from build entirely. |
| **Asset Management** | Direct asset references in the scene; `Resources.Load` for runtime-loaded assets only. |
| **Persistence** | Unity `PlayerPrefs` for lightweight persistent state (skill tree purchases, currency). No save-system abstraction. |
| **Art Style** | 2D sprite-based; Deep Forge palette; flat per-surface color (no normals, no PBR); hand-drawn or AI-assisted |
| **Art Pipeline Complexity** | Medium — custom 2D sprites, sprite-sheet animation, sprite-based VFX |
| **Audio Needs** | Moderate — SFX critical for P4 weighty feel; 2–3 music tracks for MVP; AudioContext must be unlocked on first user input (Unity WebGL quirk) |
| **Content Volume (Tier 2 MVP)** | 2 zones, 6 enemy variants (3 shared × 2 zones re-tinted), 2 bosses, 3 boomerang mod archetypes, 25–30 tree nodes. 3–4 hr first playthrough; 8–12 hr theorycraft completionist |
| **Procedural Systems** | None at MVP — asteroid field positions may be procedural but content is hand-placed |

### Architectural mandates (from TD-FEASIBILITY)

The following are inputs to `/create-architecture` and `/architecture-decision`:

1. **Object pooling layer** — `Pool<T>` with pre-warm; mandatory for boomerang, enemies, damage numbers, VFX, audio sources, asteroids, pickups
2. **Kinematic scripted boomerang** — not Rigidbody2D
3. **URP 2D with ruthless feature stripping** — document enabled features
4. **UGUI for skill tree** with custom mesh-based connector Graphic (NOT one Image per line, NOT UI Toolkit for MVP)
5. **Pre-allocated string pool** for damage numbers / UI labels — no per-frame string allocation
6. **No `FindObjectsOfType` in hot paths**

### TD success criteria (to validate this verdict at implementation time)

- Prototype week 1: boomerang feels weighty in a **WebGL browser build**, not just editor
- First WebGL build (pre-content): <20 MB Brotli, <10s cold-load on target hardware
- Gameplay slice: no GC pauses >10ms during a 6-enemy pierce event

---

## Risks and Open Questions

### Design Risks

- **Boomerang feel fragility** — entire game rests on one mechanic. Week-1 prototype is load-bearing; if it fails the feel test, the concept is HIGH RISK and requires revision.
- **Fuel economy tuning subtlety** — 20s first run to ~3min max requires tight pacing curves with diminishing-returns on kill/mine extension.
- **Tree node variety per P1** — 25–30 distinct "feel-jump" nodes is hard. Risk of filler nodes violating P1 at playtest time.
- **Two distinct boss mechanics at solo-dev budget** — each boss needs unique attack patterns and spatial identity.

### Technical Risks (from TD-FEASIBILITY)

- **Unity WebGL cold-load size** — hard ceiling ~40–50 MB Brotli-compressed for <30s first-interaction load on mid-range hardware
- **Boehm GC pauses mid-pierce** (30–80ms) — mitigated only by aggressive pooling from day 1
- **Skill tree UI performance** under naive UGUI implementation — mitigated by single-mesh connector Graphic
- **Brotli/COOP-COEP host support** — verify at target launch date for whichever free-play portal is chosen

### Scope Risks (from PR-SCOPE)

- **Hex-grid skill tree UI is a 3–4 week hidden cost** (not "substantial UI work"). Most underestimated line item in the plan.
- **Tier 2 realistic at 5 months**, not 4 — TD architectural mandates front-load ~3 weeks of non-visible infrastructure
- **P4 "Weighty Everything" invites infinite polish** — hard-cap first-10-nodes polish time at 3 weeks across the project
- **Solo context-switching tax** — batch systems / art / polish in continuous blocks rather than interspersing
- **Motivation decay Month 3–4** — classic solo-dev failure mode; external playtests at Week 10 and Week 16 mitigate

### Open Questions (answered by prototyping, not debate)

- Does the boomerang feel weighty in a **WebGL browser build** after Week 1 prototyping? *(Hard gate — if no, revisit concept)*
- Does the auto-throw cadence + arc-reading + return-timing rhythm produce a flow state? *(Playtest Week 10)*
- Does the 20-second first run deliver the promised 1→2 damage dopamine moment? *(Early balance test)*
- Will 25–30 tree nodes feel rich or claustrophobic? *(Depends on P1 discipline on every node)*
- Does the Deep Forge palette read correctly under a variety of monitor gamma / ambient lighting? *(Visual polish milestone)*

---

## MVP Definition

**Core hypothesis**: *The 1→2 damage dopamine moment, experienced in a 20-second first run followed by a skill-tree purchase and a measurably-stronger 40-second second run, creates sufficient retention to drive a player from first-boot to defeating the zone 2 boss.*

The MVP answers one question: **Is the core loop fun enough to retain the Optimizing Mastery Player through 3–4 hours of first playthrough and 8–12 hours of theorycraft?**

### Required for MVP (Tier 2)

1. Kinematic auto-throw auto-return boomerang with 3 mod archetypes (Pierce, Chain, Explode-on-return)
2. Fuel-limited run economy with diminishing-return kill/mine extension
3. Persistent hex-grid skill tree with ~25–30 nodes, center-out, prereq-gated
4. 2 zones, each with 3 enemy types + 1 boss (in-field escalating-wave boss pattern)
5. WebGL desktop-browser deployment, <30s cold-load target
6. Deep Forge visual polish on Tier-1 weight surfaces (boomerang impact, enemy deaths, asteroid cracks)
7. Sound design pass (2 music tracks, weighty impact SFX)
8. Balance pass on fuel economy and tree pacing

### Explicitly NOT in MVP

- Prestige / NG+ mechanic
- 3rd zone
- Steam port
- Daily challenges / leaderboards
- Narrative, NPCs, dialogue (AP5)
- Mobile browser (explicit scope-out)
- Custom boss mechanics beyond 2 total

### Scope Tiers

| Tier | Content | Timeline (solo, pro dev) |
| ---- | ---- | ---- |
| **Tier 1 — Must Ship** | 1 zone, 3 enemies, 1 boss; 1 boomerang + 2 mods (Pierce, Chain); ~15 tree nodes, 3 clusters; fuel economy; WebGL build | **~3 months** |
| **Tier 2 — Should Ship (realistic target)** | Tier 1 + 2nd zone + 2nd boss + 3rd mod (Explode-on-return) + tree at 25–30 nodes + Tier-1 weight polish + sound pass + balance pass | **~4–5 months** (PR recommends planning for 5) |
| **Tier 3 — Stretch** | Tier 2 + 4th mod (Multi-throw) + tree to ~40 nodes + prestige/NG+ + elites/enemy variants + full juice polish pass | **~5–6 months** |
| **Tier 4 — Post-launch** | Steam port (3–4 weeks) + Steam achievements/leaderboards/cloud saves + 3rd zone + daily challenges | Post-MVP |

**Cut order under time pressure**: Tier 3 cut first → Tier 2 2nd-zone-bundle is the hardest defensible cut. Tier 1 is the commercial floor — thin at 60–90 min first-playthrough but mechanically complete. If Tier 1 is all that ships, it is a shippable-but-small product, not a broken one.

*(PR-SCOPE recommended pulling the 3rd mod archetype + 7 more nodes down into Tier 1 to strengthen the graceful-fallback. User declined the reorder in favor of ship-the-minimum-and-iterate philosophy. Flagged here for re-evaluation if Tier 2 runs long.)*

---

## Production Guidance

Operational recommendations from PR-SCOPE. Not pillars, but project-level rules that shape sprint planning.

- **Budget 5 months for Tier 2, not 4.** TD's architectural mandates front-load ~3 weeks of non-visible infrastructure. 4 months is optimistic.
- **Prototype Week 1 is a hard gate.** Boomerang-in-isolation feel test in actual WebGL browser build (not editor). If it does not feel weighty after 5 working days, the concept is HIGH RISK and the plan is revised before any further content is built.
- **Month 3 "WebGL Stability Week".** Dedicated time to test on Firefox.
- **External playtests at Week 10 and Week 16.** Resets calibration, restores motivation during the Month 3–4 "messy middle." Do not skip as premature.
- **Record 30-second "reference feel clips"** at every major feel-tuning milestone. Monthly A/B against latest build to catch silent feel regressions (solo-dev has no QA safety net).
- **Hard-cap first-10-nodes polish time at 3 weeks** across the entire project. P1's "felt power jump" quality bar invites infinite polish; the cap is the circuit-breaker.
- **Batch work in continuous blocks** — systems block → content block → art/audio/polish block. Solo context-switching tax is 20–45 minutes per hat-swap; interspersing burns hours/week.

---

## Next Steps

Ordered — do not skip ahead.

- [x] 1. `/setup-engine` — configure Unity 6 LTS + URP 2D in the repo; populate `docs/engine-reference/unity/` with version-aware API snapshots (LLM knowledge cutoff is May 2025; engine version is newer)
- [x] 2. `/art-bible` — Deep Forge visual anchor expanded into a full production-gating art bible (asset standards, character/enemy/boss design direction, VFX palette)
- [x] 3. `/design-review design/gdd/game-concept.md` — validate this document against the 8-section GDD standard before downstream GDD authoring
- [x] 4. `/map-systems` — decompose the concept into individual systems (ship control, boomerang weapon, mod system, fuel economy, asteroid mining, enemy waves, boss encounter, skill tree, shop/progression, run/session state) with dependency map and priority tiers
- [ ] 5. `/design-system [system]` — author per-system GDDs in dependency order. Start with the Boomerang Weapon system (highest-risk, validates Week 1 prototype gate).
- [ ] 6. `/review-all-gdds` — holistic cross-system consistency check before architecture
- [ ] 7. `/gate-check concept-to-architecture` — phase gate
- [ ] 8. `/create-architecture` — master architecture blueprint and Required ADR list
- [ ] 9. `/architecture-decision (×N)` — write ADRs for each Required decision (object pooling, URP feature stripping, UI framework choice, kinematic motion pattern, …)
- [ ] 10. `/create-control-manifest` — compile decisions into actionable rules sheet
- [ ] 11. `/architecture-review` — validate coverage
- [ ] 12. `/ux-design` — UX specs for main menu, pre-run, in-run HUD, skill tree, death/return screens
- [ ] 13. `/prototype boomerang-core` — Week 1 prototype gate. Validate feel in WebGL browser build.
- [ ] 14. `/playtest-report` — document prototype session verdict. If PASS, proceed to `/create-epics` and production sprints.
- [ ] 15. `/create-epics` + `/create-stories` — break systems into epics and implementable stories
- [ ] 16. `/sprint-plan new` — plan first production sprint
