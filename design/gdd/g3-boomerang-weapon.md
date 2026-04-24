# G3 Boomerang Weapon

> **Status**: In Design — CD-GDD-ALIGN CONCERNS adopted
> **Author**: user + /design-system orchestration
> **Last Updated**: 2026-04-23
> **Last Verified**: 2026-04-23
> **Implements Pillar**:
> - **Mechanical primary**: P2 Mastery in where the player moves — the weapon's motion math, target acquisition, and contact rules exist to support positional skill expression.
> - **Experiential primary**: P1 Multiplicative Dopamine + P4 The Tree IS the Game — what the *player* feels when a throw hits.
> - **Supporting**: P2 Mastery (catch-window positioning), P3 Weighty Everything (contact feel).
>
> Both framings are correct at their respective layers. Mechanical primary = what the code must enforce. Experiential primary = what the player must feel.
>
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (adopted) 2026-04-23 — pillar-layer reconciliation and Expression-aesthetic deferral recorded below.

## Summary

The boomerang is the game's single weapon — an auto-aimed, auto-returning
kinematic projectile that travels through asteroids and enemies. The player 
does not aim or fire; the boomerang targets the nearest valid entity on a short 
cooldown and returns. Skill
expression lives in positioning — moving the ship to
maximize contacts on flight and return.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `C2 Object Pooling, C3 Fixed-Timestep Tick, G1 Player Ship Controller, G2 Camera System, C6 Stat Resolver`

## Overview

The boomerang is thrown automatically from the player's ship on a fixed
cooldown toward the nearest valid enemy or asteroid. It travels on a weighty
scripted arc — not physics-simulated — striking any targets along its path,
then reverses and returns on a mirrored arc the player can predict and
exploit. On catch, the cooldown resets and the next throw queues.

The weapon is *the* mechanical identity of the game. Where Vampire Survivors
delegates combat to proximity auras and Astro Prospector delegates it to
forward lasers, this game delegates it to a single thrown tool that the
player re-forges across runs via the skill tree. Without the boomerang,
there is no game — it is the surface on which every skill-tree node, every
mod archetype, and every fuel-extending kill is rendered legible.

Three things define it:

1. **Kinematic scripted motion, not physics simulation.** The arc is
   authored, not emergent. Pillar 2 ("Mastery in where the player moves") requires the player to
   predict impact and return trajectory 100% of the time — a bouncing
   physics body cannot meet that bar.
2. **Auto-aim + auto-return.** No reticle, no fire button, no steering
   mid-flight. Pillar 2 (Mastery in positioning) lives here:
   the player commands *where they stand*, not *where they point*.
3. **Weighty feel on contact.** Audio, hitstop, camera response, and VFX
   converge at the moment of impact. Pillar 4 ("Weighty Everything") is sold
   on the boomerang's contact events before anything else.

The boomerang is a data-driven projectile whose per-run stats (damage, arc
radius, throw cooldown, pierce count, chain targets, return behavior) are
resolved from a `GameStatsContext` produced by the skill tree (P1a) and mod
system (G4) via the stat resolver (C6). The weapon does not know those
upstream systems exist — it queries a frozen stat context each throw and
executes. This keeps the weapon durable against progression-layer changes:
new tree nodes or mods add to the stat context without modifying weapon
motion code.

## Player Fantasy

**The desperate miner becoming the unstoppable miner — measured in the boomerang.**

The first runs feel desperate. Twenty seconds of fuel, a boomerang that
barely chips the ore, a cosmos that does not care. You are not mining the
field — you are surviving it, throwing a blunt wedge of cyan and hoping it
comes back before the asteroids close. Then the tree opens. A Pierce node.
A second tier of damage. A return-detonation. And somewhere in the third or
fourth run, you notice you have stopped flinching. The boomerang leaves
your ship and you are already repositioning for the *next* throw. The weapon
responds exactly as you built it.

This is the fantasy of **craft outpacing threat**. The boomerang becomes
the instrument you feel in your hands every second of the run: heat in the
release, weight in the return, a satisfying thud when it seats back into
your hull. You do not aim at asteroids. You arrange yourself relative to
them, and the weapon — a weapon that is visibly more layered, more
angular, more *itself* than it was three runs ago — does the rest. Every
node you placed in the tree is a gram of metal in the thing you are
throwing.

**The peak second this section is trying to bottle:** the first run where
you stop dodging and start stepping *into* the field, because the
boomerang you built has more authority over that space than the asteroids
do.

### Pillar alignment

This section primarily serves:

- **P1 Multiplicative Dopamine** — the cross-run arc from "desperate" to
  "stepping into the field" is the felt shape of P1 made visible in a
  weapon.
- **P5 The Tree IS the Game** — every mechanical change in the boomerang's
  behavior between runs is a node placed in the tree; the weapon is the
  tree's avatar in play.

And depends on (mechanical preconditions):

- **P2 Mastery in where the player moves** — the player commands positioning
  to maximize impact. Catch-window and chain routing demand spatial planning.
- **P4 Weighty Everything** — "heat in the release, weight in the return, a
  satisfying thud" is P4 rendered into sentences. Game Feel + Visual/Audio
  Requirements specify the feel budget.

**MDA Expression aesthetic — deferred out of G3.** The Expression aesthetic
(priority #2 in game-concept.md) — "more layered, more angular, more
itself" visible weapon evolution — is *not* owned by G3. G3 is the weapon
mechanics layer; it is intentionally scoped narrow. Expression is delivered
by:

- **V1 Juice Layer** — boomerang sprite evolution, trail density, hue
  shift, layered geometry overlays per archetype unlocked. V1's GDD will
  own the visual-evolution specification.
- **P1b Node Catalog** — per-node visual payoff at purchase (the 1→2
  damage dopamine moment has a visual component authored at the node level,
  not the weapon level).

Stories that implement Expression-serving visual evolution must cite V1 or
P1b, not this GDD.

### What this fantasy is NOT

- **NOT a godlike-power fantasy.** The boomerang outperforms the field — it
  does not trivialize it. Every run must still be fought; the unstoppable
  feeling is relative to where you were three runs ago, not absolute.
  Keeps Pillar 1 honest: later upgrades trend flatter, and the field scales
  to match.
- **NOT a Hades wrath fantasy.** No divine anger, no character voice, no
  narrative framing. The emotional register is artisanal — a craftsperson
  trusting their tool.
- **NOT a Vampire Survivors horde-survival fantasy.** The player is not
  surrounded-and-surviving; they are forging and deploying a specific tool
  against a specific field.
- **NOT a manual-aim power fantasy.** Autonomy lives in positioning (P2),
  not aiming.

## Detailed Design

### Core Rules

Rules are numbered. Each is unambiguous. Formulas live in Section Formulas;
edge-case handling lives in Section Edge Cases.

**CR-1 Throw Conditions.** The ship fires a throw when ALL are true in the same
fixed tick:
1. Boomerang state machine is in `Idle`.
2. Throw cooldown timer ≤ 0. Timer is seeded from `GameStatsContext.throw_cooldown`
   at the moment of the *previous catch*, not the previous throw.
3. At least one valid target exists in the play-area rectangle (CR-4).

If any is false the system waits silently. No "attempted throw" feedback.

**CR-2 Arc Shape.** The boomerang travels on a smooth planar C-shaped arc
computed once at throw commit from two inputs: ship anchor position (G1) and
selected target position (CR-4). Shape is hardcoded geometry; only `arc_radius`
and `arc_flight_time` from `GameStatsContext` modulate it.

- **Outbound leg**: boomerang departs the ship anchor, sweeping to the *right*
  of the ship→target vector by a lateral offset of `arc_radius`. Reaches apex
  at the target's position at midpoint of `arc_flight_time`.
- **Peak / Turn**: at apex, motion transitions continuously from outbound to
  inbound — no stop, no visible corner.
- **Inbound leg**: boomerang returns, homing on the ship's *current* anchor
  position. The inbound arc mirrors the outbound (lateral offset is now to the
  *left* of the original ship→target vector).

"Left" and "right" are frozen at throw time — boomerang does not steer
mid-flight to follow a moving target. The inbound leg's landing point DOES
track the ship's live position each tick.

**CR-3 Bounds Enforcement.** Before committing to a throw, sample the computed
arc and verify all samples lie within the camera-bound play-area rectangle
(G2). If any sample exceeds bounds, reduce `arc_radius` until all samples fit.
Target position is NOT altered. If even `arc_radius = 0` (straight-line)
exceeds bounds, the throw is suppressed this tick (treat as no-valid-target;
remain in `Idle`). Flag for Section Edge Cases.

**CR-4 Target Acquisition.** A valid target satisfies all three:
- Implements `IBoomerangTarget` (enemies G6, asteroids G5, bosses G8)
- Position is within camera-bound play-area rectangle
- `IsAlive == true`

**Selection algorithm.** At throw commit, iterate the `TargetRegistry`
candidate list once via index loop. Track minimum squared-distance to ship
anchor. If two candidates are within ε = 0.01 world-units² of each other,
tiebreak by smallest world-X (leftmost on screen). Return a `TargetHandle`.

**TargetRegistry contract:**
- Scene-persistent `MonoBehaviour` singleton, initialized before wave spawning,
  cleared on scene unload.
- Internal storage: `List<IBoomerangTarget>` pre-allocated to capacity 64
  (covers 50-entity peak with headroom).
- API: `Register(IBoomerangTarget)`, `Unregister(IBoomerangTarget)`,
  `GetCandidateCount()`, `GetCandidateAt(int index)`. No enumerable-returning
  methods — index access only to avoid allocation.
- `IBoomerangTarget` exposes at minimum: `Transform TargetTransform { get; }`,
  `bool IsAlive { get; }`.
- `TargetHandle` is a readonly struct: `{ Transform TargetTransform, bool IsValid }`.
  Does NOT hold an `IBoomerangTarget` reference (pool-safe).

Performance budget: < 0.05 ms per call at 50 candidates on WebGL IL2CPP.

**CR-5 Contact Rule (damage application).** Whenever the boomerang's collider
enters another entity's trigger during any flight state:

1. Resolve damage via pierce-falloff formula (Section Formulas) with
   `n = contact index within this arc` (n = 0 for first hit).
2. Call `IHittable.TakeDamage(int damage, Vector2 contact_point)`. G3 does NOT
   know whether the target is enemy or asteroid — the interface dispatches.
3. Raise `BoomerangHit(TargetHandle, int pierceIndex, int damageDealt)` C#
   event (G4 mods / V1 juice / A1 audio subscribe).
4. Increment contact index for this arc. No hard cap — pierce is unlimited
   (CR-6).
5. If this is the primary boomerang's **first outbound contact**, trigger
   chain (CR-7).

Multi-entity contact in a single fixed-timestep frame: process in order of
Physics2D collider-enumeration (first-enter = first-processed). Flag for
Section Edge Cases (frame-ordering determinism).

**CR-6 Pierce Rule.** The boomerang arc is **unaltered on contact**. Pierce is
not a capacity-gated feature — the boomerang passes through every target it
encounters along its arc. Damage falls off with each contact per the pierce
formula (Section Formulas). Practical effect: after ~10 pierces at typical
falloff rates, damage is negligible but non-zero — a natural soft cap by
arithmetic, not by rule.

**CR-7 Chain Rule.** Chain activates only if ALL true:
- The current hit is the primary boomerang's **first outbound contact**
  (`pierceIndex == 0` AND state is `Outbound`, not `Inbound`)
- `GameStatsContext.chain_count > 0`
- At least one valid target exists other than the entity just hit

On activation:
1. Acquire a chain-boomerang instance from C2's chain-pool (separate pool).
2. Spawn chain-boomerang at the contact position.
3. Select chain-target: nearest valid `IBoomerangTarget` (per CR-4) excluding
   the entity just hit.
4. Chain-boomerang inherits the parent's `GameStatsContext` snapshot (same
   damage, same pierce falloff, same arc parameters).
5. Chain-boomerang computes its own arc from (spawn position, chain-target).
   It travels to that target and returns — but returns to its **spawn
   position** and vanishes, NOT to the ship. No ship-collider catch check.
6. Raise `ChainTriggered(TargetHandle chainTarget, Vector2 spawnPosition)`
   C# event.

**Chain does NOT cascade.** The chain-boomerang's first hit does *not* trigger
another chain — only the primary boomerang can. Max 2 boomerang instances on
screen at any time.

**CR-8 Return Phase Begin.** Primary boomerang transitions to `PeakTurn` upon
reaching arc apex (target position at throw time). Chain-boomerang transitions
to its return phase upon reaching its chain-target position. Neither waits for
a live target at the apex — apex is a geometric point, not a re-acquisition
event.

**CR-9 Catch Rule.** Catch detection is only active during `Inbound`. Each
fixed-timestep frame, the primary boomerang checks whether its collider
intersects G1's ship-collider. On intersection:
1. Transition to `CaughtLate` transient state (one fixed-timestep frame).
2. If `GameStatsContext.return_detonate_radius > 0`, fire AoE damage to all
   valid targets within radius from catch position. This is the only moment
   the AoE fires. Formula applies full `base_damage` (no pierce falloff —
   detonate is a separate damage event).
3. Seed cooldown timer from `GameStatsContext.throw_cooldown`. Cooldown begins
   counting down immediately.
4. Return boomerang instance to C2 pool.
5. Raise `BoomerangCaught(Vector2 catchPosition, int detonateTargetsHit)` C#
   event.

Transition to `Idle`. If cooldown was pre-expired during flight, the next
throw fires at the next tick after `CaughtLate` resolves — no dead frame.

The chain-boomerang does NOT use this rule — it vanishes at its spawn position
with no cooldown effect (CR-7).

**CR-10 No-Valid-Target Rule.** If CR-4 returns `IsValid == false` (zero
candidates in registry):
- No throw fires.
- Cooldown is not reset, not modified.
- State remains `Idle`; system re-evaluates next fixed tick.
- No audio, no VFX, no UI feedback. Silence is the signal that the field is
  clear.

**CR-11 Pool Constraints.**
- **Primary boomerang pool**: pre-warm count = 1. Only-one-primary-in-flight
  enforced by CR-1 (only thrown from `Idle`).
- **Chain boomerang pool**: pre-warm count = 1. Chain cannot cascade → at
  most one chain-boomerang at a time.
- Total boomerang instances active simultaneously: ≤ 2.
- Pool exhaustion attempt (unreachable given invariants): throw is suppressed
  silently; flag for Section Edge Cases as a defensive case.

### States and Transitions

Two distinct state machines — primary boomerang lifecycle and chain-boomerang
lifecycle. They run independently but share the same underlying arc integrator.

#### Primary Boomerang State Machine

| State | Entry Condition | Behavior During State | Exit Condition |
|-------|----------------|----------------------|----------------|
| **Idle** | Game start; OR `CaughtLate` completes; OR throw suppressed (no target / bounds fail) | Cooldown timer counts down. Each fixed tick re-evaluates throw conditions (CR-1). | All throw conditions met → `ArmedForThrow` (same tick) |
| **ArmedForThrow** | Throw conditions satisfied in `Idle` | Run CR-4 target acquisition. Compute arc (CR-2). Run bounds clamp (CR-3). Snapshot `GameStatsContext` from C6. Acquire pool instance. Set arc parameters on instance. | Arc committed + instance acquired → `Outbound`. If bounds fails at `arc_radius=0` → back to `Idle` |
| **Outbound** | Entry from `ArmedForThrow` | Boomerang interpolates along outbound arc leg each fixed tick. Contact detection active. On contact: CR-5 fires; if first contact: CR-7 also fires. | Boomerang position reaches apex (target position) → `PeakTurn` |
| **PeakTurn** | Apex reached | Single fixed-timestep frame. Assigns inbound vector. Contact detection remains active this frame. | Frame ends → `Inbound` |
| **Inbound** | Entry from `PeakTurn` | Boomerang interpolates along inbound arc leg each fixed tick, homing on ship's live anchor. Contact detection active (CR-5 fires on overlaps with enemies/asteroids but NOT chain — chain is outbound-only per CR-7). Catch-check against ship-collider each tick (CR-9). | Boomerang collider overlaps ship-collider → `CaughtLate` |
| **CaughtLate** | Catch detected in `Inbound` | Single fixed-timestep frame. Fire detonate AoE if active. Seed cooldown. Release to pool. Raise `BoomerangCaught` event. | Frame ends → `Idle` |

Happy path: `Idle → ArmedForThrow → Outbound → PeakTurn → Inbound → CaughtLate → Idle`.

#### Chain Boomerang State Machine

| State | Entry Condition | Behavior During State | Exit Condition |
|-------|----------------|----------------------|----------------|
| **Spawned** | CR-7 activation on primary's first outbound contact | Acquire from chain-pool. Compute arc from (contact position, chain-target). Inherit parent's `GameStatsContext` snapshot. Set arc parameters. | Arc committed → `ChainOutbound` (same tick) |
| **ChainOutbound** | Entry from `Spawned` | Interpolates along arc toward chain-target. Contact detection active — applies damage via CR-5 with own contact index. Does NOT fire CR-7 (no cascade). | Reaches chain-target position → `ChainReturning` |
| **ChainReturning** | Chain-target reached | Interpolates back along mirrored arc toward **spawn position** (NOT ship). Contact detection active on the return leg (pierce damage still applies). | Reaches spawn position → `Vanished` |
| **Vanished** | Spawn position reached | Release to chain-pool. Raise `ChainVanished(Vector2 finalPosition)` event. | Instance released; machine resets |

Happy path: `Spawned → ChainOutbound → ChainReturning → Vanished`.

### Interactions with Other Systems

| Dependency | G3 reads | G3 writes | Ownership | Notes |
|---|---|---|---|---|
| **C2 Object Pooling** | `primaryPool.Acquire()`; `chainPool.Acquire()` | `primaryPool.Release(instance)`; `chainPool.Release(instance)` | C2 owns pool lifetime; G3 calls acquire/release | Two pools: primary (pre-warm 1), chain (pre-warm 1). Seeded at run-start, not lazily — avoids first-throw allocation spike. |
| **C3 Fixed-Timestep Tick** | `GameTick` event via `OnTick(float fixedDeltaTime)` subscription | — | C3 owns tick cadence | G3 subscribes `OnEnable`, unsubscribes `OnDisable`. All arc integration, contact evaluation, and catch-check run in this callback. Never in `Update`. |
| **C6 Stat Resolver** | `GameStatsContext` snapshot read **once per primary throw** at `ArmedForThrow`. Chain inherits parent's snapshot, does NOT re-query C6. | — | C6 owns resolution. G3 is consumer-only. | Stats: `base_damage`, `throw_cooldown`, `arc_radius`, `arc_flight_time`, `pierce_falloff`, `chain_count`, `return_detonate_radius`. Mid-flight stat changes do NOT affect in-flight instances — immutable per throw by design (predictability; Pillar 2). |
| **G1 Player Ship Controller** | `IShipAnchor.WorldPosition` (throw origin, sampled at throw-commit); `IShipAnchor.ShipCollider` reference (catch detection) | — | G1 exposes stable interface; G3 holds cached reference injected at scene-load (no `Find`) | Catch detection: each `Inbound` tick, G3 checks overlap of own collider with ship-collider. |
| **G2 Camera System** | `ICameraBounds.PlayArea` (for bounds enforcement, CR-3) | `ICameraShakeRequester.Request(intensity, duration, direction)` on contact events | G2 owns camera; G3 is requester via concrete interface (NOT event bus per TD-SYSTEM-BOUNDARY #2) | Shake intensity derives from stats × per-event scalar — exact mapping in Game Feel section. |
| **G4 Mod System** | None at runtime — G4's mod effects are baked into `GameStatsContext` via C6 | C# events: `BoomerangThrown`, `BoomerangHit(TargetHandle, int pierceIndex, int damageDealt)`, `ChainTriggered(TargetHandle, Vector2)`, `BoomerangReturnStarted`, `BoomerangCaught`, `ChainVanished` | G3 owns event declarations; G4 subscribes | Plain C# `event` delegates — NOT a project-wide event bus. G4's "Explode-on-return", "Pierce", "Chain" archetypes are encoded as stat deltas into C6. Future non-stat behavior mods (e.g., "spawn satellite on hit") subscribe to these events. **`pierceIndex = -1` is the sentinel value indicating a detonate-on-return hit** (F4) rather than a pierce hit (F3); subscribers use this to distinguish damage-event type per EC-13. |
| **G5 Asteroid Mining** | — | `IAsteroidHittable.TakeDamage(int damage, Vector2 contactPoint)` on collider overlap | G5 owns ore-tier resolution + crack lifecycle | G3 does not know ore tiers — calls the interface and moves on. Damage value is resolved per CR-5 with pierce falloff. |
| **G6 Enemy System** | — | `IEnemyHittable.TakeDamage(int damage, Vector2 contactPoint)` same pattern | G6 owns enemy health + reactions | Identical interface contract. G3 hits anything implementing `IHittable` supertype. |
| **G8 Boss Encounter** | — | `IBossHittable.TakeDamage` (identical or extends IEnemyHittable) | G8 owns boss state | G3 treats bosses the same as enemies. G8 implements the interface. |
| **P1a Skill Tree** | Not directly | Not directly | Indirect — flows through C6 only | P1a pushes stat deltas to C6 via `IUpgradeSource`. G3 never queries P1a. This indirection is the entire point of C6. |
| **V1 Juice Layer** | — | `IJuiceRequester.Request(JuiceEventType, Vector2 contactPoint, float intensityScalar)` on: throw, contact, return-start, catch, chain-trigger, detonate, chain-vanish | V1 owns VFX pool and playback; G3 is requester | Concrete interface, no event bus. Intensity scalar derives from damage-dealt / base_damage ratio (larger hits = louder juice). |
| **A1 Audio System** | — | `IAudioRequester.Request(SfxEventType)` on: throw, contact, return-start, catch, chain-trigger, detonate, chain-vanish | A1 owns SFX pool + WebGL AudioContext; G3 is requester | Same concrete-interface pattern. |
| **U1 HUD** | — | `BoomerangState` read-only struct exposed by `BoomerangController` | U1 polls each render frame for cooldown-wheel display | U1 also subscribes to `BoomerangCaught` for cooldown-reset animation trigger. |

#### Unity 6.3 LTS Implementation Requirement (not implementation)

**Requirement**: Kinematic scripted motion with reliable trigger-based contact
detection on a pooled 2D collider at fixed-timestep cadence, with interpolated
render between ticks for smooth playback on 60/120/144 Hz displays.

**Implementation path deferred to ADR.** Two candidates are both viable and
must be evaluated by the kinematic-motion ADR:

1. **Kinematic `Rigidbody2D` + `MovePosition`** — required for
   `OnTriggerEnter2D` callbacks to fire reliably on a scripted mover (a Unity
   2D physics requirement, not a post-cutoff change). Standard callback
   architecture.
2. **Pure `Transform.position` writes + manual `Physics2D.OverlapCircleNonAlloc`
   polling each tick** — zero Rigidbody2D. Interprets TD-FEASIBILITY mandate #3
   strictly. Adds G3 contact-budget but maximum control over detection timing.

The GDD does not select between these — the ADR will. Both options satisfy
this GDD's contract.

**Post-cutoff risk**: Unity 6 Physics 2D solver iterations changed from 2022
LTS (per `docs/engine-reference/unity/breaking-changes.md` MEDIUM risk). If
option 1 is selected, verify trigger-enter callback ordering at Week-1
prototype against dense-cluster pierce scenarios.

**Fixed-tick render interpolation**: idiomatic pattern — store
`_previousPosition` and `_currentPosition` in the controller each C3 tick; in
`Update`, lerp by `(Time.time - Time.fixedTime) / Time.fixedDeltaTime`. Both
implementation candidates use this pattern.

#### Edge Cases Flagged for Section Edge Cases

Recorded here so nothing is lost when Section Edge Cases is authored:

1. Target destroyed between CR-4 selection tick and CR-2 arc-commit tick.
2. Target destroyed between arc commit and arc apex (dead-target apex).
3. Ship moves off-screen during throw (ship-collider temporarily out of
   play area).
4. Bounds clamp to zero arc_radius still exceeds play area (target outside
   camera).
5. Chain-boomerang spawns when no other valid target exists (next-nearest
   is the entity just hit — excluded — leaves zero candidates).
6. Chain fires but pierce-falloff has reduced damage to visually-zero
   (floating-point underflow; tiny-number UX).
7. Primary boomerang's detonate-AoE re-damages an entity already hit in
   this arc (double damage within one throw?).
8. Frame-level ordering when primary hits 2+ entities in a single
   fixed-timestep frame (which is "first" for chain-trigger?).
9. `arc_flight_time` tuning value = 0 (division-by-zero in interpolation).
10. Pool exhaustion attempt on primary or chain pool (unreachable per
    CR-11 but needs defensive behavior).
11. Chain-boomerang still in flight when primary boomerang is caught
    (orphaned chain — does it vanish too, or continue independently?).
12. Chain-boomerang's spawn position overlaps ship-collider (immediate
    self-catch-fail? ship-collider is not a valid chain interaction).
13. Player ship dies during primary boomerang flight (where does the
    boomerang go when there's no anchor to return to?).

## Formulas

All formulas are evaluated in the C3 `OnTick(float fixedDeltaTime)` callback.
No heap allocations occur in formula evaluation paths — all intermediate values
are `float` or `Vector2` stack locals.

**Tuning-knob convention**: `arc_radius` is interpreted as **peak deviation**
(the maximum lateral offset from the straight line P0→P2), not as the Bezier
control-point offset. This requires the control point P1 to be placed at
`2 × arc_radius` off the midpoint (Bezier peak deviation = half the
control-point offset). Tuning designers always set `arc_radius` in
player-visible world-unit terms — what you set is what the player sees.

---

### F1 Arc Position — Outbound Leg

**Method**: Quadratic Bezier. Maps naturally to three semantic points —
departure (P0), perpendicular bulge (P1), apex (P2) — and `Vector2.Lerp`
evaluates it with no extra dependency. Alternative (parametric sine) requires 
more algebra to enforce "apex lands exactly at target."

**Perpendicular direction** (RIGHT of the ship→target vector):

```
forward    = normalize(P2 - P0)
perp_right = -Vector2.Perpendicular(forward)   // Unity returns (-y, x) = screen-LEFT; negate for RIGHT
```

Both `forward` and `perp_right` are computed once at throw commit and stored
frozen on the boomerang instance.

**Control point** (peak-deviation interpretation — OQ-F1 resolved):

```
P1 = (P0 + P2) / 2 + perp_right * (arc_radius * 2)
```

The factor of 2 ensures the resulting Bezier peak deviation equals
`arc_radius` exactly.

**Named expression**:

```
pos_out(t_norm) = (1 - t_norm)^2 * P0
                + 2 * (1 - t_norm) * t_norm * P1
                + t_norm^2 * P2
```

where `t_norm = t / arc_flight_time` (normalized 0 → 1, clamped).

**Variables**:

| Symbol | Type | Range | Description |
|---|---|---|---|
| `P0` | Vector2 | play area bounds | Ship anchor at throw commit. Frozen. |
| `P2` | Vector2 | play area bounds | Target position at throw commit. Frozen. |
| `P1` | Vector2 | derived | Bezier control point: midpoint(P0,P2) + perp_right × 2 × arc_radius. Frozen. |
| `arc_radius` | float | 0.0–20.0 world units | Peak deviation of the arc from the straight line. Clamped by CR-3 before throw. |
| `arc_flight_time` | float | 0.1–5.0 s | Outbound leg duration. Must be > 0; clamped at 0.1 before use. |
| `t` | float | 0.0–arc_flight_time s | Elapsed time since throw. Accumulated each fixed tick. |
| `t_norm` | float | 0.0–1.0 | `t / arc_flight_time`. Clamped. |
| `pos_out` | Vector2 | play area bounds | World position of boomerang at time t. |

**Output range**: bounded by play area (CR-3). `t_norm` hard-clamped to [0, 1].

**Worked example** (`arc_radius = 5`, `arc_flight_time = 0.8 s`, ship at (0,0), target at (10,0)):
- `forward = (1, 0)`; `perp_right = (0, -1)`
- `P1 = (5, 0) + (0, -1) × 10 = (5, -10)` (note: 2 × arc_radius = 10)
- At `t = 0.4 s` → `t_norm = 0.5`:
  - `pos_out = 0.25·(0,0) + 0.5·(5,-10) + 0.25·(10,0) = (5.0, -5.0)`
- **Peak deviation at midpoint = 5.0 world units** ✓ matches `arc_radius` as designed.

---

### F2 Arc Position — Inbound Leg

Inbound is a quadratic Bezier mirroring the outbound, with three differences:

1. Origin is the apex P2 (frozen at throw commit).
2. Destination is the ship's **live** anchor position sampled each tick.
3. Lateral offset uses `perp_left = -perp_right` — the LEFT side of the original
   ship→target axis (both frozen at throw commit).

Because the destination is live, the inbound Bezier is **recomputed each tick**
with the current ship position. This gives the homing behavior described in
CR-2 while keeping the arc shape stable and predictable (same perpendicular
axis, same peak deviation).

**Named expression**:

```
P2_in   = P2                                     // apex (frozen)
P3_live = IShipAnchor.WorldPosition              // sampled each tick
P1_in   = (P2_in + P3_live) / 2 + perp_left * (arc_radius * 2)

pos_in(s_norm) = (1 - s_norm)^2 * P2_in
               + 2 * (1 - s_norm) * s_norm * P1_in
               + s_norm^2 * P3_live
```

where `s_norm = s / arc_flight_time` (s = elapsed time since `PeakTurn`).

**Variables**:

| Symbol | Type | Range | Description |
|---|---|---|---|
| `P2_in` | Vector2 | play area bounds | Apex position = F1's P2. Frozen at throw commit. |
| `P3_live` | Vector2 | play area bounds | Ship anchor sampled each fixed tick from `IShipAnchor.WorldPosition`. |
| `P1_in` | Vector2 | derived | Recomputed each tick: midpoint(P2_in, P3_live) + perp_left × 2 × arc_radius. |
| `perp_left` | Vector2 | unit vector | `-perp_right` from F1. Frozen. |
| `arc_radius`, `arc_flight_time` | float | same as F1 | Unchanged between legs. |
| `s` | float | 0.0–arc_flight_time s | Elapsed since `PeakTurn`. |
| `s_norm` | float | 0.0–1.0 | `s / arc_flight_time`. Clamped. |
| `pos_in` | Vector2 | play area bounds | World position at inbound time s. |

**Output range**: guaranteed to reach `P3_live` at `s_norm = 1.0`. If the ship
moves during return, `P1_in` shifts each tick — change per tick bounded by
`ship_max_speed × fixedDeltaTime`, small enough to read as smooth.

---

### F3 Pierce Damage Falloff

**Named expression**:

```
damage(n) = max(1, floor(base_damage * (1 - pierce_falloff)^n))
```

`n` is 0-indexed contact count within a single arc (both legs combined). Resets
to 0 at each new throw.

**Variables**:

| Symbol | Type | Range | Description |
|---|---|---|---|
| `base_damage` | int | 1–∞ | Integer damage tier from `GameStatsContext`. Pillar 1 integer-tier discipline. |
| `pierce_falloff` | float | 0.0–1.0 | Per-hit damage retention loss. 0.0 = no falloff; 1.0 = only first hit does damage, rest floor to 1. |
| `n` | int | 0–∞ | Contact index this arc. Resets each throw. |
| `damage(n)` | int | 1–base_damage | Integer damage at contact n. Clamped to minimum 1 (every hit produces visible feedback). |

**Output range**: `[1, base_damage]`. Monotonically non-increasing in n. Never
zero — `max(1, ...)` enforces floor.

**MVP default**: `pierce_falloff = 0.35`. At `base_damage = 5` this gives the
sequence `5, 3, 2, 1, 1, 1...` — three distinct-feeling hits before flattening
to the minimum.

**Worked example** (`base_damage = 5`, `pierce_falloff = 0.35`):

| n | Calculation | damage(n) |
|---|---|---|
| 0 | max(1, floor(5 × 1.000)) | 5 |
| 1 | max(1, floor(5 × 0.650)) = floor(3.25) | 3 |
| 2 | max(1, floor(5 × 0.4225)) = floor(2.11) | 2 |
| 3 | max(1, floor(5 × 0.2746)) = floor(1.37) | 1 |
| 4 | max(1, floor(5 × 0.1785)) = floor(0.89) | 1 (floor) |
| 5+ | floor drops below 1 | 1 (floor) |

---

### F4 Detonate-on-Return AoE Damage

No falloff. Detonate is a separate damage event — the catch reward, not a
pierce extension.

**Named expression**:

```
detonate_damage = base_damage
```

Applied to each `IHittable` within `return_detonate_radius` of the catch
position at the moment CR-9 fires.

**Variables**:

| Symbol | Type | Range | Description |
|---|---|---|---|
| `base_damage` | int | 1–∞ | Same stat as F3. No modification. |
| `return_detonate_radius` | float | 0.0–∞ world units | AoE radius from catch position. 0.0 = no detonate. |
| `detonate_damage` | int | 1–∞ | Damage dealt to each entity in radius. Equals `base_damage` exactly. |

**Output range**: `[1, ∞)` bounded by `base_damage`. No pierce falloff.

**Rationale for flat base_damage**: detonate is the payoff for repositioning
to catch cleanly (P2, P4). Scaling it would require another stat and tree-node
surface area the MVP doesn't need. A future "Detonate Damage ×2" node becomes
a stat multiplier in `GameStatsContext`; the formula stays structurally
identical.

---

### F5 Cooldown Timer

**Named expression**:

```
cooldown_remaining(tick) = max(0.0, cooldown_remaining(tick - 1) - fixedDeltaTime)
```

Initialized to `throw_cooldown` at the tick `CaughtLate` resolves (CR-9
step 3). Decrements each fixed tick. Clamps at 0.0 and holds. Throw permitted
when `cooldown_remaining <= 0` (CR-1 condition 2).

**Variables**:

| Symbol | Type | Range | Description |
|---|---|---|---|
| `throw_cooldown` | float | 0.1–10.0 s | Seed value from `GameStatsContext`. Loaded at catch, not at throw. |
| `fixedDeltaTime` | float | > 0 s | Unity's `Time.fixedDeltaTime`, typically 0.02 s (50 Hz). From C3. |
| `cooldown_remaining` | float | 0.0–throw_cooldown s | Current value. Read by CR-1 condition 2 and by U1's cooldown wheel. |

**Output range**: `[0.0, throw_cooldown]`. Terminal state 0.0 is stable.

---

### F6 Camera Shake Intensity

**Scaling choice**: logarithmic. Linear scaling would produce absurd shake at
tier-10+ (damage could be 50+). The log-ratio formulation anchors
`intensity = base_shake_intensity` when `damage_dealt = base_damage` — the first
unmodified hit always feels like the baseline. Doubling damage doubles shake
once, then log-compresses further doublings.

**Named expression (pierce-hit variant)**:

```
shake_intensity = clamp(
    base_shake_intensity * (1 + log2(damage_dealt / base_damage)),
    0.0,
    max_shake_intensity
)
```

**Named expression (detonate-on-return variant — OQ-F6B resolved)**:

```
detonate_shake_intensity = clamp(
    (base_shake_intensity * 2.0) * (1 + log2(damage_dealt / base_damage)),
    0.0,
    max_shake_intensity
)
```

Detonate uses twice the base intensity as its baseline, signaling the catch
reward as a distinct weight surface from pierce hits (P4).

**Variables**:

| Symbol | Type | Range | Description |
|---|---|---|---|
| `damage_dealt` | int | 1–∞ | Integer damage delivered this contact. Output of F3 (pierce) or F4 (detonate). |
| `base_damage` | int | 1–∞ | Current `GameStatsContext.base_damage`. Normalization denominator. |
| `base_shake_intensity` | float | 0.0–∞ | Shake intensity for a hit equal to base_damage. MVP placeholder: **0.15** (TBD — OQ-F6A). |
| `max_shake_intensity` | float | 0.0–∞ | Hard ceiling on shake. MVP placeholder: **0.6** (TBD — OQ-F6A). |
| `shake_intensity` | float | 0.0–max_shake_intensity | Output passed to `ICameraShakeRequester.Request`. |
| `detonate_shake_intensity` | float | 0.0–max_shake_intensity | Detonate variant output. |

**Output range**: `[0.0, max_shake_intensity]`. Hard-clamped. At
`damage_dealt < base_damage` (pierce-reduced hit), intensity falls below
baseline — weaker hits produce lighter shake, reinforcing pierce falloff with
tactile feedback.

**Note on `damage_dealt < base_damage`**: `log2(x < 1)` is negative, so
intensity < base_shake_intensity. `clamp(0.0, ...)` prevents negative. A
pierce-reduced hit should produce less shake than a clean hit — the clamp
makes this correct.

**Open questions** (carried forward to Open Questions section and Tuning Knobs):
- **OQ-F6A**: `base_shake_intensity = 0.15` and `max_shake_intensity = 0.6` are
  TBD placeholders. Validate at Week-1 feel prototype. Exact values depend on
  G2's camera-shake implementation units, camera FoV, and target screen
  resolution.

**Worked example** (`base_damage = 5`, `base_shake_intensity = 0.15`,
`max_shake_intensity = 0.6`):

| Event | damage_dealt | formula variant | intensity |
|---|---|---|---|
| Clean first hit (n=0) | 5 | pierce | 0.15 |
| Pierce n=1 | 3 | pierce | clamp(0.15×(1+log2(0.6)), 0, 0.6) = 0.039 |
| Pierce n=3 (floored) | 1 | pierce | 0.0 (clamped from negative) |
| Tier-2 clean hit | 10 (base_damage=10) | pierce | 0.15 |
| Tier-4, no falloff mod, 2× multiplier | 40 (base_damage=20) | pierce | 0.30 (log2(2) = 1.0) |
| Detonate, clean base | 5 (base_damage=5) | detonate | 0.30 (2× base) |
| Detonate, 2× damage mod | 10 (base_damage=5) | detonate | 0.60 (clamped at max) |

---

### Formula Summary

| Formula | Expression (short) | Output | Range |
|---|---|---|---|
| F1 Outbound position | Bezier(P0, P1 = mid + perp_right × 2·arc_radius, P2) at t_norm | Vector2 | Play area |
| F2 Inbound position | Bezier(P2, P1_in live, P3_live) at s_norm | Vector2 | Play area |
| F3 Pierce falloff | max(1, floor(base_damage × (1-pierce_falloff)^n)) | int | [1, base_damage] |
| F4 Detonate damage | base_damage | int | [1, ∞) |
| F5 Cooldown timer | max(0, remaining - fixedDeltaTime) per tick | float | [0, throw_cooldown] |
| F6 Shake intensity (pierce) | clamp(base_shake × (1 + log2(dmg/base_dmg)), 0, max_shake) | float | [0, max_shake_intensity] |
| F6 Shake intensity (detonate) | clamp(2·base_shake × (1 + log2(dmg/base_dmg)), 0, max_shake) | float | [0, max_shake_intensity] |

## Edge Cases

19 edge cases resolved below, grouped by theme. Each follows the format:
**If** [exact condition] **then** [exact outcome]. *[Rationale if non-obvious.]*

Resolutions that should surface as testable Acceptance Criteria (Section H)
are flagged with **→ AC flag**.

### Target Lifecycle

**EC-1 Target dies between selection and arc commit.** **If** the selected
`TargetHandle` is acquired during CR-4 and by the time `ArmedForThrow` computes
the arc (same tick, later in execution order) the target's `IsAlive == false`,
**then** the arc is committed using the dead target's last known
`TargetTransform.position` as P2. The throw proceeds. The boomerang flies to a
geometric point in world space, not to a living entity. Dead targets do not
deregister from `TargetRegistry` mid-tick; `Unregister` fires on their
`OnDisable`/destruction, which happens at end-of-frame after physics
resolution. The boomerang does not "know" its target — it knows a frozen
`Vector2`. No corrective re-acquisition occurs. **→ AC flag**: same-tick
target death must not cause a `NullReferenceException`; throw completes to a
geometric endpoint.

**EC-2 Target dies mid-flight (between arc commit and apex).** **If** the
target whose position defines P2 is destroyed while the boomerang is
`Outbound`, **then** the boomerang continues to P2 unaltered. P2 is a
`Vector2` on the boomerang instance, not a live reference. If the dead
target's collider is still present during the tick of death (Unity defers
collider removal to end-of-frame), the boomerang may register a contact hit
on it that tick; `IHittable.TakeDamage` on a dead entity is the receiving
system's responsibility to handle defensively, not G3's.

**EC-3 Ship moves off-screen during the inbound leg.** **If** the player
moves the ship such that `IShipAnchor.WorldPosition` is temporarily outside
the play-area rectangle while the boomerang is `Inbound`, **then** F2
recomputes `P3_live` using that out-of-bounds position. Catch detection
(CR-9) fires when the boomerang's collider overlaps `ShipCollider`
regardless of where that collider is on screen. There is no bounds check on
catch. G1 is responsible for enforcing ship bounds; G3 trusts the anchor.

**EC-4 Target outside camera; even straight-line throw exceeds play area.**
**If** CR-3 clamps `arc_radius` to 0.0 and the resulting straight-line path
from P0 to P2 still has samples outside the play-area rectangle, **then** the
throw is suppressed this tick: state remains in `Idle`, cooldown is not
reset, no throw audio or VFX fires. Treat as non-valid target this tick
(same as CR-10). If the target re-enters the play area later, CR-4 selects
it normally.

### Motion Math

**EC-5 `arc_flight_time` is zero or sub-minimum.** **If**
`GameStatsContext.arc_flight_time` is ≤ 0 or below `0.1 s` at throw commit,
**then** the value is clamped to `0.1 s` before use in F1/F2 and stored on
the boomerang instance. Division by zero in `t_norm` is thereby impossible.
The clamp is applied in `ArmedForThrow` before arc parameters are written.
A non-fatal data-validation warning (development builds only) is raised if
the raw stat value is below 0.1 s. The 0.1 s floor is a tuning-safety
constant, not a design target — legitimate tuning ranges start at 0.5 s.
**→ AC flag**: unit-test `arc_flight_time` values of 0.0, -1.0, 0.05 all
resolve to `t_norm` in [0, 1] with no exception.

**EC-6 Pierce-falloff reduces damage to zero.** **If** `pierce_falloff`
and `n` are such that `base_damage × (1 - pierce_falloff)^n` falls below 1.0
before floor, **then** F3's `max(1, floor(...))` ensures the result is 1.
**This case cannot produce a zero-damage hit.** F3 guarantees `damage(n) ≥ 1`
for all valid inputs. No additional handling needed.

**EC-7 `arc_radius = 0` produces degenerate control point.** **If**
`arc_radius` is clamped to exactly 0.0 by CR-3, **then**
`P1 = midpoint(P0, P2)` and the Bezier degenerates to a straight line. F1
and F2 remain valid; no divide-by-zero or NaN. `perp_right` from
`normalize(P2 - P0)` of a zero vector returns `Vector2.zero` in Unity,
propagating safely through the Bezier as `P1 = midpoint(P0, P2)`. Straight-line
throw fires normally.

**EC-8 Ship and target at identical world position.** **If** P0 == P2
exactly, **then** `forward = normalize(Vector2.zero) = Vector2.zero` and
`perp_right = Vector2.zero`. All Bezier samples collapse to the single
point. The boomerang sits at throw origin, `t_norm` increments normally,
`PeakTurn` fires after `arc_flight_time`, inbound begins homing on the ship.
Contact detection fires if any hittable collider overlaps the boomerang
position. Ugly but not a crash. The only realistic way to trigger this is if
the ship is inside the target's collider, which is a separate collision
concern owned by G1. No special handling in G3.

### Pool Integrity

**EC-9 Pool exhaustion — primary pool.** **If** a code defect causes
`primaryPool.Acquire()` to return `null` during `ArmedForThrow` (unreachable
under correct invariants per CR-11), **then** the throw is silently
suppressed: state returns to `Idle`, cooldown is not reset, no throw audio
or VFX. An `Assert.IsNotNull` (development builds only) surfaces the defect.
The game continues.

**EC-10 Pool exhaustion — chain pool.** **If** `chainPool.Acquire()`
returns `null` during CR-7 (unreachable under correct invariants), **then**
the chain boomerang is not spawned. The primary continues its arc normally.
`ChainTriggered` is NOT raised. `Assert.IsNotNull` fires in development
builds. No player-visible indication.

### Chain Lifecycle

**EC-11 No valid chain target at chain-trigger moment.** **If** CR-7
activates but after excluding the entity just hit, `TargetRegistry` returns
zero valid candidates, **then** chain activation is aborted: chain-pool
instance is NOT acquired. `ChainTriggered` is NOT raised. The primary
continues its arc. CR-7 condition 3 prevents the acquire. This is a normal
runtime state, not an error.

**EC-12 Chain-boomerang still in flight when primary is caught.** **If**
the primary is caught (`CaughtLate`) while a chain is still in
`ChainOutbound` or `ChainReturning`, **then** the chain continues its
independent state machine to `Vanished` without interruption. The chain's
lifecycle is not subordinate to the primary's. The primary's catch seeds the
cooldown timer; the next primary throw may fire while the chain is still
alive. CR-11 limits active boomerangs to ≤ 2 total; the chain occupies one
slot until `Vanished`. Primary pool and chain pool are independent — a
primary and a chain can coexist, which is the intended design.
**→ AC flag**: integration test — catching the primary while a chain is in
flight does not release the chain to the pool prematurely.

**EC-13 Detonate AoE hits an entity already pierced this arc.** **If** the
detonate AoE (F4) fires at catch and entities within
`return_detonate_radius` were already struck by pierce damage (F3) during
the same arc, **then** F4 applies `base_damage` to those entities again.
**This is intentional double-damage within one throw** — the pierce softens,
the detonate finishes. The detonate is a distinct damage event with its own
`IHittable.TakeDamage` call and its own `BoomerangHit` raise with
`pierceIndex = -1` (sentinel distinguishing detonate hits from pierce hits
for subscribers). This is the catch-reward mechanic the player is
incentivized to set up by catching over a cluster (P2, P4) — by design.

**EC-14 Frame-ordering when primary hits multiple entities in one fixed
tick.** **If** the boomerang's collider overlaps two or more entity
colliders in a single `Physics2D` resolution step within one fixed tick,
**then** contacts are processed in the order `Physics2D` enumerates them.
For option 1 (Kinematic Rigidbody2D + MovePosition), this is
`OnTriggerEnter2D` callback order — deterministic within a frame but
implementation-defined by Unity's broadphase ordering. For option 2
(`OverlapCircleNonAlloc` polling), the order is the buffer-fill order,
also deterministic. In either case: **the first entity enumerated is
`n = 0` and triggers the chain check (CR-7).** Both options produce stable
reproducible ordering across identical runs. Per Pillar 2, this is
sufficient — players cannot distinguish which of two simultaneously-hit
entities is "first" at this timescale. **→ AC flag**: two-entity
simultaneous-contact test — exactly one `ChainTriggered` fires;
`pierceIndex` values are 0 and 1.

### Cross-System

**EC-15 Pause, menu, or scene transition during flight.** **If** C3's
fixed-tick loop is paused while the boomerang is in any active state,
**then** the boomerang freezes at its last computed position. G3 subscribes
to C3's `GameTick` event and receives no ticks when C3 is paused. Render
interpolation (`Update` lerp) also freezes. On resume, ticking continues
from the frozen state. No state corruption. Scene transitions that unload
the current scene trigger `OnDisable` on `BoomerangController`, which
unsubscribes and returns pool instances before scene unload.
**→ AC flag**: pause-resume determinism test — pausing 2 s mid-flight and
resuming produces the same arc completion as uninterrupted.

**EC-16 Stat changes (tree purchase, mod unlock) mid-flight.** **If** a
skill-tree node or mod activates while a primary boomerang is in flight,
**then** the in-flight instance is unaffected. `GameStatsContext` is
snapshotted once at `ArmedForThrow` and stored immutably on the boomerang.
G3 does not re-query C6 during flight. The next throw snapshots the updated
context. Enforced by the architecture (snapshot model) and required by
Pillar 2 — mid-flight behavior changes would make arcs unpredictable. Chain
boomerangs inherit the parent's snapshot; they also cannot see mid-flight
stat changes.

**EC-17 Player ship destroyed during primary boomerang flight.** **If** G1
signals ship destruction while the primary is `Outbound`, `PeakTurn`, or
`Inbound`, **then** `BoomerangController` is notified via an
`IShipDestroyedListener` callback (exact mechanism finalized in the G1/G3
interface ADR). On notification: boomerang transitions immediately to a
transient `Dissolving` state (brief VFX-only cleanup), then releases to the
primary pool. Cooldown is irrelevant — the run has ended. `BoomerangCaught`
is NOT raised. An in-flight chain also transitions to `Dissolving` and
releases to its pool. Run-end cleanup must release both pool slots before
scene teardown. *Note: `Dissolving` is documented here as an edge-case
branch and not included in the formal States & Transitions diagrams, which
cover the happy-path throw lifecycle only.* **→ AC flag**: verify pool
`ActiveCount == 0` after run-end cleanup.

**EC-18 Boomerang enters and exits a trigger before Physics2D resolves the
contact.** **If** `arc_flight_time` is short and `fixedDeltaTime` is large
enough that the boomerang travels more than one entity's trigger diameter
in a single tick, **then** `OnTriggerEnter2D` may not fire for that entity
(tunnel-through). For option 1 (Kinematic Rigidbody2D + MovePosition):
Unity Physics2D's sweep-based `MovePosition` detects tunneling through
trigger volumes if `Rigidbody2D.CollisionDetectionMode2D` is set to
`Continuous`. This must be set. For option 2 (`OverlapCircleNonAlloc`
polling): tunneling is not detectable by a single-frame overlap check; the
ADR selecting option 2 must address this by either (a) scaling the polling
collider radius to expected per-tick travel distance, or (b) using
`Physics2D.CircleCast` sweep instead of an overlap check. The ADR decides;
this GDD flags the requirement. **→ AC flag** (Week-1 prototype): throw at
minimum `arc_flight_time` (0.1 s) against a small asteroid collider at
maximum ship-to-target distance; confirm hit registers.

**EC-19 Chain boomerang's spawn position overlaps ship collider.** **If**
the entity struck by the primary's first outbound contact is at a position
that places the chain-boomerang's spawn point inside `ShipCollider`
(extreme case: enemy has moved into ship-collider bounds), **then** the
chain boomerang is spawned normally and begins `ChainOutbound`. The ship
collider is NOT a valid contact for the chain — the chain has no catch-check
against the ship (only the primary does, per CR-9). The chain's
`ChainOutbound` does not run CR-9. Any ship-collider overlap is transient
(sub-tick). No erroneous damage or catch triggers on the ship from the
chain. G3 must ensure the chain's contact detection layer-mask excludes the
ship layer. **→ AC flag**: layer-mask configuration test — chain boomerang
`OnTriggerEnter2D`/`OverlapNonAlloc` filter never hits the ship collider.

## Dependencies

| System | Direction | Nature of Dependency |
|--------|-----------|---------------------|
| **C2 Object Pooling** | This depends on C2 | Structural — framework-level. Primary pool (pre-warm 1) + chain pool (pre-warm 1). Hard — G3 cannot function without pooling. |
| **C3 Fixed-Timestep Tick** | This depends on C3 | Structural — simulation cadence. Hard — all motion + contact detection runs in C3's tick callback. |
| **C6 Stat Resolver** | This depends on C6 | Data — consumes `GameStatsContext` snapshot. Hard — G3 has no standalone stats; all boomerang behavior is stat-driven. |
| **G1 Player Ship Controller** | This depends on G1 | Data — throw anchor + catch collider. Hard — boomerang has no origin without ship. |
| **G2 Camera System** | This depends on G2 (for bounds) + requests to G2 (for shake) | Data (bounds) + Request (shake). Bounds-dep is hard (CR-3); shake-dep is soft (cosmetic if disabled). |
| **G4 Mod System** | G4 depends on this | Event — G4 subscribes to G3's `BoomerangThrown/Hit/ChainTriggered/ReturnStarted/Caught/ChainVanished` events. G3 does not know G4 exists. |
| **G5 Asteroid Mining** | G5 depends on this | Interface — G5 implements `IAsteroidHittable.TakeDamage(int, Vector2)`. G3 calls the interface; damage-source discovery is G5's side. |
| **G6 Enemy System** | G6 depends on this | Interface — G6 implements `IEnemyHittable.TakeDamage(int, Vector2)`. Same pattern as G5. |
| **G8 Boss Encounter** | G8 depends on this | Interface — G8 implements `IBossHittable.TakeDamage` (identical to or extending `IEnemyHittable`). |
| **P1a Skill Tree** | Indirect — through C6 | Data (indirect) — P1a pushes stat deltas via `IUpgradeSource` to C6. G3 never queries P1a. |
| **P1b Node Catalog** | Indirect — through C6 | Data (indirect) — each node effect modifies `GameStatsContext`. Boomerang-relevant nodes must cite G3's stat names (`base_damage`, `arc_radius`, `pierce_falloff`, etc.). |
| **V1 Juice Layer** | V1 depends on this | Request — G3 calls `IJuiceRequester.Request(JuiceEventType, Vector2, float intensityScalar)` on throw/contact/return-start/catch/chain-trigger/detonate/chain-vanish. |
| **A1 Audio System** | A1 depends on this | Request — G3 calls `IAudioRequester.Request(SfxEventType)` on same event set as V1. |
| **U1 HUD** | U1 depends on this | Data — U1 polls `BoomerangState` struct for cooldown-wheel display; subscribes to `BoomerangCaught` for reset animation. Soft — game works if HUD is missing. |
| **M2 Tutorial / Onboarding** | M2 depends on this | Event — M2 observes `BoomerangThrown/Hit/Caught` events for coaching state machine. Soft — only active during onboarding runs. |

**Bidirectional note**: Reverse entries (this system listed as "depends on
G3" in those GDDs) must be added when the dependency-listed GDDs are
authored. Registry cross-references will be added in Phase 5 after approval.

## Tuning Knobs

All designer-adjustable values below. Knobs sourced from `GameStatsContext`
are ultimately controlled by P1a tree nodes and G4 mod stat deltas flowing
through C6. G3-internal constants are hardcoded initially — some may be
promoted to M1 accessibility settings (reduced-motion, etc.) in later
revisions.

| Parameter | MVP Start | Safe Range | Effect of Increase | Effect of Decrease | Source |
|-----------|-----------|------------|--------------------|--------------------|--------|
| `base_damage` | 1 (first run); scales with tree | 1–∞ integer tiers | Stronger hits; fewer contacts to clear the field | Unplayable early; pierce-falloff flattens to "all 1s" | `GameStatsContext` (P1a + G4 via C6) |
| `throw_cooldown` | 0.8 s | 0.3–5.0 s | Slower cadence; more repositioning window; weakens feel if too long | Spam rate; feel degrades; visual overlap | `GameStatsContext` (P1a via C6) |
| `arc_radius` (peak deviation) | 5.0 world units | 0.0–20.0 wu | Wider arc, bigger pierce-cluster coverage, slower feel | Straight-line feel ("not boomerang-like"); 0.0 is degenerate straight-line | `GameStatsContext` (P1a via C6) |
| `arc_flight_time` | 0.8 s outbound (same inbound) | 0.1–5.0 s (0.1 s is hard safety floor — EC-5) | Slower arc, feels weak | Snap-throw feel; tunnel-through risk (EC-18); feel degrades below 0.3 s | `GameStatsContext` (P1a via C6) |
| `pierce_falloff` | 0.35 | 0.0–1.0 | Sharper falloff; late-pierce nodes may become valueless (Pillar 1 risk) | Flatter falloff; pierce becomes overpowered; "lawnmower" late-game exploit risk | `GameStatsContext` (P1a via C6) |
| `chain_count` | 0 (locked until node purchased); 1 once unlocked | 0–1 at MVP | `> 1` is undefined at MVP (chain doesn't cascade — second chain is meaningless unless semantics changes) | 0 disables the entire chain archetype | `GameStatsContext` (P1a + G4 via C6) |
| `return_detonate_radius` | 0.0 (locked until node purchased); 3.0 wu post-unlock | 0.0–10.0 wu | Catch becomes AoE-spam; P2 incentive to catch-over-cluster strengthens | 0.0 disables the detonate archetype | `GameStatsContext` (P1a + G4 via C6) |
| `base_shake_intensity` | **0.15** *(TBD — OQ-F6A)* | 0.05–0.5 | Shake too strong at base tier; eye-strain; WebGL laptop risk | Shake too subtle; P4 weight surface weakens | G3 internal constant → possibly M1 reduced-motion toggle |
| `max_shake_intensity` | **0.6** *(TBD — OQ-F6A)* | 0.2–1.0 | Shake ceiling too high at extreme tiers; eye-strain at 40+ damage hits | Ceiling too low; P1 "I'm unstoppable" feel doesn't land at late tree | G3 internal constant → possibly M1 reduced-motion toggle |
| `detonate_shake_multiplier` | 2.0 *(OQ-F6B resolved)* | 1.0–3.0 | Detonate overshadows pierce impact; pacing issue | Detonate feels same as pierce; P4 distinctness erodes | G3 internal constant |
| `min_arc_flight_time` (safety floor) | 0.1 s | not tunable | N/A | N/A — do not reduce below 0.1 s without fixing EC-5 | G3 internal safety constant |
| `target_select_epsilon` | 0.01 wu² | not tunable | N/A | N/A — finer epsilon risks float-comparison non-determinism | G3 internal math constant |
| `target_registry_capacity` | 64 | 32–256 | Less frequent resize exposure; slight memory cost | Resize at 50+ entity densities — allocation spike risk | TargetRegistry internal constant |

### Knob Interactions (balance watch items)

- `arc_flight_time` short + `arc_radius` large → wider arcs in less time →
  higher tunnel-through risk (EC-18). Maintain
  `arc_flight_time / arc_radius ≥ 0.1 s/wu` as a soft guideline.
- `pierce_falloff` low + `base_damage` high → late-game "lawnmower" where
  an entire field clears in one throw → playtest for Pillar 1 feel-jump
  discipline (CD Note #3 in systems-index).
- `throw_cooldown` low + `arc_flight_time` low → rapid-fire feel; risks
  violating CR-1 "one primary in flight" window of opportunity.
- `chain_count = 1` with `return_detonate_radius > 0` → chain fires on
  outbound contact THEN primary detonates on catch = two AoE moments per
  throw. Intended and satisfying — a late-game build payoff.
- `base_damage` × `(1 - pierce_falloff)^n` interaction forms the mid-tree
  feel landscape. Playtest tuning target: a fully-statted pierce+damage
  build at Zone-2-clear tree should produce a sequence like 12, 8, 5, 3, 2, 1,
  1... where the first 5-6 pierces are *distinct* (Pillar 1 discipline).

## Visual/Audio Requirements

**Weight Events Table ownership decision (resolves CD-SYSTEMS constraint #1)**:
G3 owns the Weight Events Table below as the authoritative source. V1 (Juice
Layer) is **primary executor**; A1 (Audio System) is **subordinate**, consuming
the same table as a read-only reference. This resolves the "who owns per-event
budget arbitration" question from CD-SYSTEMS: G3 specifies the weight surface,
V1 implements visuals, A1 implements audio, and neither invents events
independently.

### Weight Events Table

| Event | Visual Feedback | Audio Feedback | Priority | Intensity Scaling |
|-------|----------------|---------------|----------|-------------------|
| **BoomerangThrown** | On ship anchor: 2–3 angular sparks of electric-cyan radiating along the departure vector. Sub-frame duration (1–2 frames), additive blend, no trail. No screen event. Boomerang Point Light2D activates at full in-run intensity (0.4–0.6) at the throw frame. | Dry mechanical release: taut mid-frequency scrape (~2–4 kHz, 40–60 ms) followed by low-pitched whoosh rising in pitch as the boomerang accelerates. Not percussive — a forged tool under tension, not a gun. | Tier-2 (should-have) | Flat — throw is invariant regardless of stats. |
| **Arc travel — Outbound** | Continuous electric-cyan trail behind the boomerang. Trail width scales with speed (`arc_flight_time` shorter = wider apparent trail). At full fuel: sharp electric-cyan, 0.3–0.5 world-unit width, 6–10 frames fade. At 25% fuel: desaturated to steel-blue-grey per art bible Section 2.1; trail shortens to 3–5 frames fade. No particles during travel — the trail IS the effect. Boomerang Point Light2D illuminates its own path. | Rising tonal whirr: pitched hum that rises slightly as boomerang approaches apex. Subtle "blade through air" texture — not whistle (too light), not roar (too heavy). Reference: a weighted disc spinning through dense atmosphere. | Tier-1 (MVP mandatory) | Flat per throw. Trail intensity shifts with fuel state (art bible rule) — game-state signal, not damage signal. |
| **Arc travel — Inbound** | Same trail as outbound but mirrored lateral offset (left side). Trail color unchanged. No additional VFX by default. If Explode-on-return archetype active: slow-pulsing amber-orange core glow builds on the boomerang sprite over the inbound leg (2–3 frame pulse cycle, additive blend, amber hue — NOT cyan). Signals the detonation is loaded. | Inbound whoosh has lower, heavier character than outbound — returning with intent, not curiosity. If Explode-on-return active: low sub-bass rumble underlies the return whirr, building for the last 0.3 s. | Tier-1 (inbound travel); Tier-2 (Explode-on-return buildup) | Flat per throw. Detonate buildup glow intensity: flat (binary loaded/unloaded state). |
| **BoomerangHit — pierceIndex 0 (clean first hit)** | White-yellow contact flash (saturation ceiling per art bible): 3-frame full-flash on contact point, ~1.5–2× boomerang width. Simultaneous burst of 6–8 angular shards (steel-grey + rust-accent), 0.1–0.15 s fade, radiating at irregular angles (irregular = supporting register, per shape language). Hitstop: boomerang sprite freezes for 2 render frames (Game Feel section). No screen shake at minimum `base_damage`. | Heavy wedge-on-stone crack: dense low-mid impact thud (~150–400 Hz body) with high-frequency fracture-tail overtone (~3–6 kHz, ~80 ms decay). Forged tool meeting dense ore — a *crack* with mass behind it. | Tier-1 (MVP mandatory) | Scales with `damage_dealt / base_damage` via F6 pierce formula. Full intensity at n=0. Shake: `base_shake × (1 + log2(dmg/base_dmg))`, clamped 0–`max_shake`. |
| **BoomerangHit — pierceIndex 1–2 (reduced pierces)** | Same shard burst pattern but reduced: 3–5 shards (not 6–8), flash at 60–75% brightness (not full ceiling). Hitstop: 1 render frame. | Same crack SFX but reduced gain: ~60–70% of clean-hit amplitude. Audio tail shortens slightly — the boomerang is spent. | Tier-1 | Scales with `damage_dealt / base_damage`. Shard count: `ceil(6 × (damage_dealt / base_damage))`, minimum 3. Flash brightness proportional to `shake_intensity`. |
| **BoomerangHit — pierceIndex 3+ (floor damage = 1)** | Minimal: 2 shards max, brief cyan-tinted spark (boomerang is passing through on momentum alone — cyan because the tool's will is still present, but mass is gone). No full white-yellow flash. No hitstop. | Dry thin scrape: high-frequency fracture-tail only, body thud removed. Clearly audible but clearly lighter — a different sound, not just quieter. | Tier-1 (these hits must NOT disappear — feedback minimum is mandatory per P4) | Flat at floor damage. No scaling below `damage(n) = 1` — this is the floor state. |
| **ChainTriggered (chain-boomerang spawn)** | At spawn position: brief cyan ring-burst expanding outward (2–3 frames, radius = 0.5 world units, additive blend, electric-cyan matching boomerang hue). Chain boomerang is visually identical to primary in shape but at 70% sprite scale, slightly less trail opacity. Visual subordination makes the field readable: one clear primary, one clear secondary. | Shorter, higher-pitched version of throw SFX — same scrape + whoosh texture but compressed to ~50% duration and +2–3 semitones. Distinct as "secondary thing launched" without competing with primary's ongoing audio. | Tier-2 (should-have) | Flat — chain spawn is binary. |
| **ChainVanished (chain returns to spawn)** | At vanish position: small dissipation — 2–3 cyan sparks inward-collapsing (reverse direction from spawn ring). Brief, 1–2 frames. No lingering trail. | Soft descending tone: chain's whoosh loses pitch and fades in ~60 ms. Gone; audio confirms it without drama. | Tier-3 (post-MVP polish) | Flat. |
| **BoomerangReturnStarted (PeakTurn transition)** | **No dedicated VFX.** The trail's lateral offset shifting from right to left (outbound→inbound arc) IS the visual signal — the shape of the arc reads the turn. Adding a flash at apex would imply a contact event (none has occurred). Pillar 3 compliance: arc shape alone must communicate the turn. | **No dedicated SFX.** The whoosh pitch shifts as inbound begins — same audio as arc travel, but directional Doppler character reverses subtly. | N/A — no dedicated budget item. Arc shape + audio continuity carry this event. | N/A |
| **BoomerangCaught (primary catch, no detonate)** | On ship collider: 2-frame white-yellow flash at ship anchor (saturation ceiling — catch is a weight surface). Ring of 4–6 angular sparks outward from catch point, cyan-tinted (not steel-grey — this is the weapon, not debris). | Primary catch chime: low resonant tone — a forged wedge seating into its slot. Dense, not bright. Sub-bass body (~60–120 Hz, ~150 ms decay) with brief mid-frequency settling click (~800 Hz, ~20 ms). Catch is the end of a cycle; it must sound like a door closing, not an explosion. | Tier-1 (MVP mandatory) | **Catch chime: flat (invariant).** Catch always feels the same regardless of stats — catch is a ritual, not a damage event. The scaling lives in the detonate, not the chime. |
| **BoomerangCaught + detonate (return_detonate_radius > 0)** | Simultaneous with catch flash/sparks above: amber-orange AoE ring expanding from catch point, 0.2–0.3 s fade, radius = `return_detonate_radius` in world units. Screen shake: detonate F6 formula — `base_shake × 2.0 × (1 + log2(dmg/base_dmg))`. Optional slow camera zoom-out: 2–3% FoV expansion over 0.3 s, snapping back over 0.5 s — **TBD: at-risk for WebGL** (may read as judder; cut and compensate with VFX if so). | Detonate concussive bass burst (~80–200 Hz, ~200 ms) layered OVER the catch chime, not replacing it. Chime confirms the catch; concussion confirms the AoE. Two distinct audio events in the same frame. Detonate SFX gain: TBD at prototype. | Tier-1 (detonate blast if archetype active) | Detonate shake: F6 detonate variant (2× base baseline). Detonate SFX gain: TBD at Week-1 prototype. |

### Hitstop Implementation Note

Hitstop is **render-layer only**. The boomerang sprite's rendered position
freezes for N render frames while C3's fixed-tick continues. Motion integration
does not pause; render interpolation is skipped during hitstop frames (position
lerp held at contact position). This keeps physics consistent while delivering
the feel signal. Exact implementation is V1's responsibility per the V1-primary
decision. V1 must confirm this approach with `unity-specialist` during its GDD
authoring.

### Asset Dependencies

Specific assets required from V1 (Juice Layer pool) and A1 (Audio System
pool). These are G3-sourced specifications; V1 and A1 own production.

**V1 pool assignments:**
- `vfx_boomerang_throw_spark_small` — 2–3 angular cyan sparks, 2-frame animation, additive. Pre-warm 2.
- `vfx_boomerang_trail_full_loop` — trail sprite sheet, ≥ 2 frames (full-fuel cyan, low-fuel steel-blue-grey), blended by sprite-shader lerp per art bible Section 2.1. **Most performance-critical VFX asset — must fit within 8 MB atlas budget.**
- `vfx_boomerang_impact_shard_small` — 6–8 angular shards (steel-grey + rust accent), 3-frame fade. Pre-warm 12 (covers two simultaneous 6-shard bursts).
- `vfx_boomerang_impact_flash_small` — white-yellow full-saturation radial flash, 3-frame fade. Pre-warm 4.
- `vfx_boomerang_catch_ring_small` — 4–6 angular cyan sparks. Pre-warm 2.
- `vfx_boomerang_detonate_ring_medium` — amber-orange expanding AoE ring, 4–6 frame fade, radius keys to world-unit scale. Pre-warm 1.
- `vfx_chain_spawn_ring_small` — cyan ring burst. Pre-warm 1 (one chain at a time per CR-11).

**A1 pool assignments:**
- `sfx_boomerang_throw_scrape` — single-shot. Pre-warm 1.
- `sfx_boomerang_travel_whirr_loop` — looped, pitch parameter exposed. Pre-warm 1.
- `sfx_boomerang_impact_crack_heavy` — gain 1.0 (n=0 hits). Pre-warm 2.
- `sfx_boomerang_impact_crack_mid` — gain 0.65 (n=1–2 hits). Pre-warm 4 (simultaneous pierce chain).
- `sfx_boomerang_impact_scrape_light` — gain 0.4 (n=3+ floor hits). Pre-warm 4.
- `sfx_boomerang_catch_chime` — resonant forged-metal seat sound. Pre-warm 1.
- `sfx_boomerang_detonate_blast` — concussive sub-bass burst. Pre-warm 1.
- `sfx_chain_throw_short` — compressed throw SFX. Pre-warm 1.

> **📌 Asset Spec** — Visual/Audio requirements are defined. After the art
> bible is approved (AD-ART-BIBLE pending), run
> `/asset-spec system:g3-boomerang-weapon` to produce per-asset visual
> descriptions, dimensions, and AI generation prompts from this section.

## Game Feel

### Feel Reference

**Target reference**: The Spear of Leonidas in *Assassin's Creed Odyssey*
heavy-attack throw — not for the thematic match, but for the specific
combination of committed arc trajectory, visible pre-release pause (startup),
and the way the camera micro-stutters on landed impact. The player sends the
weapon and it arrives with authority that was already visible in the
departure.

**Anti-reference**: The Kunai in *Dead Cells* (default). Fast, spammy, zero
weight — the projectile exists only as a number-delivery mechanism. No
anticipation, no recovery, no sense that the thing you threw had any mass.
The boomerang must never feel like that. If a playtester reaches for the word
"spammy," the feel has failed.

### Input Responsiveness

G3 is auto-fire — the player has no throw button. "Input" is indirect: the
ship's position at throw commit determines the arc, and the ship's live
position during inbound determines the catch window. The two primary
feel-relevant "inputs" are (a) ship arriving inside the catch window, and
(b) the player *reading* that a throw has been committed.

| Action | Max Input-to-Response Latency | Notes |
|--------|-------------------------------|-------|
| Ship repositions into catch window | One fixed tick (≤ 20 ms at 50 Hz) | Catch detection runs every fixed tick (CR-9). Overlap this tick → catch this tick. Zero latency above fixed-tick cadence. |
| Throw commit becomes visible to player | One fixed tick + one render frame (≤ 36 ms worst case at 50 Hz + 60 fps) | `ArmedForThrow` commits and raises `BoomerangThrown` in the tick; V1 throw-spark fires same tick; render interpolation displays at next render frame. |
| Cooldown-wheel reflects catch | One render frame after `BoomerangCaught` fires (≤ 16 ms) | U1 subscribes to `BoomerangCaught` for wheel reset animation trigger. |
| Detonate AoE fires (if archetype active) | Same frame as catch (CR-9 step 2) | AoE and catch are atomic in `CaughtLate`. No additional frame. |

### Animation Feel Targets

"Startup" for auto-fire events = time from triggering condition to first
visible motion. "Frames" = render frames at 60 fps (~ 16.6 ms each).

| Event | Startup | Active (primary action) | Recovery | Feel Goal |
|-------|---------|------------------------|----------|-----------|
| Throw (release effect on ship) | 0 frames (auto-fire, no anticipation anim at MVP; TBD for Tier-3 wind-up) | 2 frames (spark burst) | 0 frames | Committed release, no build-up |
| Outbound arc flight | 0 | `arc_flight_time` (default 0.8 s ≈ 48 frames) | 0 (trail fades per schedule) | Weighted transit, predictable |
| PeakTurn transition | 0 | 1 fixed tick (≤ 20 ms) | 0 (inbound begins next tick) | Continuous, no corner (CR-2) |
| Inbound arc flight | 0 | `arc_flight_time` (same as outbound) | 0 | Returning with intent |
| Catch (absorb on ship) | 0 | 2 frames (flash + sparks) | 0 | Seated, settled — door closing, not explosion |
| Chain spawn | 0 | 2 frames (ring burst) | 0 | Secondary launch, subordinate to primary |
| Chain vanish | 0 | 1–2 frames (inward collapse) | 0 | Quiet dissipation |
| Hitstop at n=0 | 0 | **2 render frames (~33 ms)** | 0 | Mass registered |
| Hitstop at n=1–2 | 0 | **1 render frame (~16 ms)** | 0 | Reduced but perceivable |
| Hitstop at n=3+ (floor damage) | 0 | **0 frames** | 0 | Passing through on momentum alone |

### Impact Moments

Using F6 values: `base_shake_intensity = 0.15` (TBD),
`max_shake_intensity = 0.6` (TBD), `detonate_shake_multiplier = 2.0`.

| Event | Hitstop | Screen Shake | Camera Impact | Controller Rumble | Time-Scale Slowdown |
|-------|---------|-------------|---------------|-------------------|---------------------|
| BoomerangThrown | 0 | None | None | Light pulse: 80 ms, low amplitude (TBD) | None |
| Hit n=0 (clean) | 2 frames | F6 pierce: `0.15` at base_damage=5. Direction: along inbound-toward-ship vector. Duration: 100 ms. | Micro-shudder: 1-frame nudge, 2-pixel max, no pan | Right trigger: 120 ms, 40% amplitude (TBD) | None |
| Hit n=1–2 | 1 frame | `intensity ≈ 0.04–0.09` (log-scaled F6) | Same micro-shudder but 1-pixel max | Right trigger: 60 ms, 20% amplitude (TBD) | None |
| Hit n=3+ (floor) | 0 | `0.0` (clamped from negative — F6 note) | None | None | None |
| ChainTriggered | 0 | None | None | None | None |
| BoomerangReturnStarted | 0 | None | None | None | None |
| BoomerangCaught (no detonate) | 0 | `0.0` — catch chime is audio-weight; visual weight is flash + sparks, not shake | None | Left trigger: 80 ms, 25% amplitude (TBD) — seated pulse, not strike pulse | None |
| BoomerangCaught + detonate | 0 | F6 detonate variant: `0.30` at base_damage=5. Radial direction (outward from catch point). Duration: 150 ms. | Slow zoom-out: 2–3% FoV expansion over 0.3 s, snap back over 0.5 s (TBD — **at-risk for WebGL**; may cut if it reads as judder) | Both triggers: 200 ms, 60% amplitude (TBD) | None |
| ChainVanished | 0 | None | None | None | None |

**TBD flags (Week-1 prototype validation)**:
- All rumble amplitudes/durations are placeholders — validate against physical gamepad at prototype stage.
- Detonate camera zoom-out: at-risk for WebGL. Cut and compensate with VFX if it reads as judder.
- `base_shake_intensity = 0.15` and `max_shake_intensity = 0.6` require calibration against G2's actual shake-implementation units and camera scale.

### Weight and Responsiveness Profile

The boomerang is **heavy and committed**. Once thrown it cannot be recalled —
the player is bound to the arc they set in motion. This commitment is the
entire point; it is the thing P2 (positional mastery) is built on. No cancel-throw, no mid-flight
steer, no abort. The player chose their position; the weapon executes.

Control level is **low during flight, high before throw**. The player's
control window is the cooldown period between catch and next throw — this is
where positioning decisions are made. In flight, zero direct control over
the boomerang and full control over the ship (for catch-window setup). This
rhythm — control, release, read, reposition, catch — is the pulse of every
run.

Snap quality is **crisp at throw and catch, smooth in transit**. Throw-spark
and catch-flash are instantaneous (2-frame events). Arc flight is a smooth
Bezier interpolation. The contrast between the snap-moments and the flowing
transit between them is what gives each throw cycle its shape. Lose the snap
and the cycle feels mushy.

Acceleration model is **arcade committed**: the boomerang has no
acceleration ramp — it begins at full speed and maintains it. Speed variation
is expressed through `arc_flight_time` changes (shorter = higher apparent
speed), not within-flight acceleration curves.

Failure texture is **repositioning, not reaction time**. Missing the catch
window means the player was in the wrong position for the inbound arc — not
that they reacted slowly. The fix is spatial, not reflexive. This is P2 (positional mastery) made
tactile: mistakes read as "I was standing in the wrong place" not "I wasn't
fast enough." The feel design reinforces this — no visual or audio punishment
for missing the catch-collider window; the cooldown still starts; the only
cost is a sub-optimal arc next throw.

### Feel Acceptance Criteria

- [ ] No playtester describes the boomerang as "floaty" or "instant." The arc transit must feel like a thrown object with mass.
- [ ] No playtester describes a clean first hit (n=0) as "soft." White-yellow flash, 2-frame hitstop, and impact crack must register as a perceivable weight event even on `base_damage = 1` first run.
- [ ] A playtester watching floor-damage hits (n=3+) and clean hits (n=0) side-by-side perceives them as different **in kind**, not just in degree. Zero-hitstop + scrape-only audio + minimal sparks at n=3+ must read as "this hit barely registered" against n=0's full flash + thud.
- [ ] A playtester can identify the catch moment by audio alone, with no visual reference. The catch chime must be sufficiently distinct from impact SFX to survive audio-only identification.
- [ ] Gamepad playtesters do NOT describe the throw-to-catch cycle as "weightless." Rumble events (throw, clean hit, catch) must collectively produce a physical texture across the cycle.
- [ ] A playtester can anticipate the catch window and reposition toward it during the inbound leg.

## UI Requirements

G3 has minimal direct UI coupling. U1 HUD polls a read-only `BoomerangState`
struct and subscribes to `BoomerangCaught` for its cooldown-wheel. All other
visual feedback lives in V1 (Juice Layer) and is not HUD.

| Information | Display Location | Update Frequency | Condition |
|-------------|------------------|------------------|-----------|
| Cooldown wheel (reads `BoomerangState.cooldown_remaining`) | HUD overlay | Every render frame during Idle countdown | Only visible when `cooldown_remaining > 0` |
| Throw cooldown reset animation | Cooldown wheel | On `BoomerangCaught` event | Triggered once per catch |
| Damage numbers (per pierce + detonate) | Contact-point world position | Per `BoomerangHit` event | Spawned via V1 pool with pre-allocated string pool (TD mandate #8) |
| Boomerang position + trail | World layer, not HUD | Every render frame during flight | Owned by V1 Juice Layer, not U1 |

> **📌 UX Flag — G3 Boomerang Weapon**: G3's UI surface is intentionally
> minor. U1 HUD GDD owns the full HUD spec. Run `/ux-design` for the in-run
> HUD screen during Phase 4 (Pre-Production) before writing epics. Stories
> that reference HUD elements should cite `design/ux/in-run-hud.md`, not this
> GDD directly.

## Cross-References

Declarative list of every external dependency this GDD relies on. All target
GDDs are currently undesigned; when authored, they must include the reverse
dependency entry.

| This Document References | Target GDD | Specific Element Referenced | Nature |
|--------------------------|-----------|----------------------------|--------|
| `GameStatsContext` snapshot per throw | `design/gdd/c6-stat-resolver.md` *(undesigned)* | `GameStatsContext` + `IUpgradeSource` contract | Data dependency |
| `Pool<T>` framework | `design/gdd/c2-object-pooling.md` *(undesigned)* | `Pool.Acquire()` / `Pool.Release()` API | Structural dependency |
| `GameTick` event subscription | `design/gdd/c3-fixed-timestep-tick.md` *(undesigned)* | `OnTick(float fixedDeltaTime)` | State trigger |
| `IShipAnchor.WorldPosition` + `ShipCollider` | `design/gdd/g1-player-ship-controller.md` *(undesigned)* | Ship anchor + catch collider interface | Data dependency |
| `ICameraShakeRequester` + `ICameraBounds` | `design/gdd/g2-camera-system.md` *(undesigned)* | Shake-request contract + play-area bounds | Ownership handoff (shake budget) + Data dependency (bounds) |
| `BoomerangThrown/Hit/ChainTriggered/ReturnStarted/Caught/ChainVanished` events | `design/gdd/g4-mod-system.md` *(undesigned)* | C# event subscriptions; `pierceIndex = -1` sentinel for detonate | State trigger |
| `IAsteroidHittable.TakeDamage` | `design/gdd/g5-asteroid-mining.md` *(undesigned)* | Damage-intake interface | Ownership handoff (damage routing) |
| `IEnemyHittable.TakeDamage` | `design/gdd/g6-enemy-system.md` *(undesigned)* | Damage-intake interface | Ownership handoff |
| `IBossHittable.TakeDamage` | `design/gdd/g8-boss-encounter.md` *(undesigned)* | Damage-intake interface (extends or identical to `IEnemyHittable`) | Ownership handoff |
| `IJuiceRequester.Request` + Weight Events Table consumption | `design/gdd/v1-juice-layer.md` *(undesigned)* | Request contract + `JuiceEventType` taxonomy | Ownership handoff (VFX budget) |
| `IAudioRequester.Request` + Weight Events Table consumption (subordinate to V1) | `design/gdd/a1-audio-system.md` *(undesigned)* | Request contract + `SfxEventType` taxonomy | Ownership handoff (audio budget) |
| `BoomerangState` read-only struct + `BoomerangCaught` subscription | `design/gdd/u1-in-run-hud.md` *(undesigned)* | Struct polled by HUD; cooldown-wheel animation trigger | Data dependency |

## Acceptance Criteria

39 criteria across 6 groups. Classifications follow `.claude/docs/coding-standards.md`:
**[Logic]** and **[Integration]** are BLOCKING gates; **[Visual/Feel]**, **[UI]**,
and **[Config]** are ADVISORY (lead sign-off). Criteria tagged **🔹 Week-1 Gate**
must pass at the kinematic prototype milestone before G3 moves out of prototype
scope.

### Core Rules Coverage

- [Logic] **GIVEN** the state machine is in `Idle`, cooldown ≤ 0, and at least one valid `IBoomerangTarget` exists in `TargetRegistry`, **WHEN** a fixed tick fires, **THEN** the machine transitions to `ArmedForThrow` within the same tick and the arc is committed before any subsequent tick. *Covers CR-1.*
- [Logic] **GIVEN** `arc_radius = 5.0 wu` and ship at (0,0) targeting (10,0), **WHEN** `t_norm = 0.5`, **THEN** the boomerang world position is (5.0, -5.0) ± 0.01 wu and the inbound leg mirrors on the left-perpendicular. *Covers CR-2.*
- [Logic] **GIVEN** a computed arc would exceed the play-area rectangle, **WHEN** `ArmedForThrow` runs the bounds check, **THEN** `arc_radius` is reduced iteratively until all samples lie within bounds; if even `arc_radius = 0` exits bounds, the throw is suppressed, state returns to `Idle`, no event fires, cooldown is not decremented. *Covers CR-3.*
- [Logic] **GIVEN** a `TargetRegistry` with one `IsAlive == false` entity and zero live entities, **WHEN** CR-4 runs, **THEN** the returned `TargetHandle.IsValid == false` and no throw fires. *Covers CR-4.*
- [Logic] **GIVEN** the boomerang is `Outbound` and contacts an `IHittable` at `pierceIndex = n`, **WHEN** the contact is processed, **THEN** `IHittable.TakeDamage` is called with `damage(n)` per F3, `BoomerangHit(TargetHandle, n, damageDealt)` is raised exactly once, and contact index increments to `n+1` without halting or altering trajectory. *Covers CR-5.*
- [Logic] **GIVEN** the boomerang has contacted 3 targets (n=0,1,2) in one arc, **WHEN** it encounters a 4th target (n=3), **THEN** the arc is unaltered, `damage(3) = max(1, floor(base_damage × (1 - pierce_falloff)^3))` is applied, no pierce cap or suppression. *Covers CR-6.*
- [Integration] **GIVEN** `chain_count == 1`, primary is `Outbound`, and ≥ 1 valid target exists other than the first-contact entity, **WHEN** the primary's first outbound contact fires (`pierceIndex == 0`), **THEN** exactly one chain-boomerang is acquired, `ChainTriggered(TargetHandle, spawnPosition)` is raised, the chain selects the nearest valid non-contact target, and no second chain spawns on the chain's own first contact. *Covers CR-7.*
- [Logic] **GIVEN** the primary is `Outbound` and has reached the `arc_flight_time` apex (P2 geometric point), **WHEN** the apex tick fires, **THEN** the machine transitions to `PeakTurn` without pausing or re-acquiring — even if P2's originating entity is dead. *Covers CR-8.*
- [Integration] **GIVEN** the primary is `Inbound` and `return_detonate_radius > 0`, **WHEN** the boomerang collider overlaps the ship collider, **THEN** atomically within `CaughtLate`: (1) AoE `base_damage` is applied to every `IHittable` within `return_detonate_radius`, (2) cooldown is seeded from `throw_cooldown`, (3) instance returns to pool, (4) `BoomerangCaught(catchPosition, detonateTargetsHit)` is raised. *Covers CR-9.*
- [Logic] **GIVEN** `TargetRegistry.GetCandidateCount() == 0` while in `Idle`, **WHEN** any number of fixed ticks fire, **THEN** no throw, no audio/VFX, cooldown unchanged, state remains `Idle`. *Covers CR-10.*
- [Integration] **GIVEN** both boomerang pools active, **WHEN** primary and chain are simultaneously in flight, **THEN** `primaryPool.ActiveCount + chainPool.ActiveCount ≤ 2` holds every tick and no acquire fires while both slots are occupied. *Covers CR-11.*

### Formula Coverage

- [Logic] **GIVEN** `P0 = (0,0)`, `P2 = (10,0)`, `arc_radius = 5.0`, `arc_flight_time = 0.8 s`, **WHEN** `pos_out(t_norm)` is evaluated at 0.0 / 0.5 / 1.0, **THEN** positions equal (0,0), (5.0,-5.0), (10,0) ± 0.01 wu; `t_norm` clamps to [0,1] for inputs outside range. *Covers F1.*
- [Logic] **GIVEN** primary is `Inbound`, apex at `P2_in`, `arc_radius = 5.0`, ship moves 2.0 wu laterally between consecutive ticks, **WHEN** `pos_in(s_norm)` is evaluated each tick, **THEN** `P1_in` recomputes from live ship position every tick, `pos_in` at `s_norm = 1.0` equals current `IShipAnchor.WorldPosition` ± 0.01 wu, arc remains C-shaped (left-perpendicular axis never flips). *Covers F2.*
- [Logic] **GIVEN** `base_damage = 5`, `pierce_falloff = 0.35`, **WHEN** F3 evaluated for n = 0..5, **THEN** output is `[5, 3, 2, 1, 1, 1]` exactly; no output < 1 for any non-negative integer n or any `pierce_falloff` in [0.0, 1.0]. *Covers F3.*
- [Logic] **GIVEN** `base_damage = 7`, `return_detonate_radius = 3.0 wu`, two `IHittable` entities within radius at catch, **WHEN** `CaughtLate` fires, **THEN** both entities receive `TakeDamage(7)` (not F3-reduced), a `BoomerangHit` event is raised for each with `pierceIndex = -1` (detonate sentinel), no entity outside radius receives damage. *Covers F4.*
- [Logic] **GIVEN** `throw_cooldown = 1.0 s`, `fixedDeltaTime = 0.02 s`, **WHEN** `CaughtLate` resolves and 50 ticks elapse, **THEN** `cooldown_remaining` reaches exactly 0.0, does not go negative, holds at 0.0 thereafter, CR-1 condition 2 evaluates true on tick 50. *Covers F5.*
- [Logic] **GIVEN** `base_damage = 5`, `base_shake_intensity = 0.15`, `max_shake_intensity = 0.6`, **WHEN** F6 pierce variant evaluated for `damage_dealt` of 5 / 3 / 1 / 10, **THEN** outputs are `0.15`, `≈0.039`, `0.0` (clamped), `0.30`. **AND** F6 detonate variant for `damage_dealt = 5` → `0.30`; for `10` → `0.60` (clamped at max). *Covers F6 pierce + detonate variants.*

### Edge Case Coverage

- [Logic] **GIVEN** a target's `IsAlive` flips to false between CR-4 selection and arc-commit within `ArmedForThrow`, **WHEN** the arc commits and the boomerang flies to P2, **THEN** no `NullReferenceException`; throw completes to P2 as a geometric endpoint; no live-reference dereference after arc commit. *Covers EC-1.*
- [Logic] **GIVEN** `arc_flight_time` is set to 0.0, -1.0, or 0.05, **WHEN** `ArmedForThrow` processes the stat snapshot, **THEN** the value is clamped to 0.1 s, `t_norm` stays in [0.0, 1.0] during flight, no divide-by-zero. *Covers EC-5.*
- [Integration] **GIVEN** primary is `Inbound` while chain is in `ChainReturning`, **WHEN** primary is caught (`CaughtLate` resolves), **THEN** chain continues uninterrupted to `Vanished`, `ChainVanished` is raised at spawn position, `chainPool.ActiveCount` drops to 0 only after `Vanished`. *Covers EC-12.*
- [Integration] **GIVEN** primary contacts exactly two `IBoomerangTarget`s overlapping in the same Physics2D resolution step, **WHEN** contacts are processed, **THEN** exactly one `ChainTriggered` fires (for the first-enumerated contact at `pierceIndex = 0`); the second entity's `BoomerangHit` has `pierceIndex = 1`; no duplicate `ChainTriggered`. *Covers EC-14.*
- [Integration] **GIVEN** primary is `Outbound` with `Time.timeScale = 1.0` and C3 ticking, **WHEN** `Time.timeScale = 0` for 2 real seconds then restored, **THEN** the boomerang resumes from frozen position; arc-completion tick count equals the reference (no-pause) run; final catch position matches a deterministic reference. *Covers EC-15.*
- [Integration] **GIVEN** player ship is destroyed while primary is `Outbound`, **WHEN** `IShipDestroyedListener` callback fires, **THEN** both primary and any in-flight chain transition to `Dissolving` and release to pools; after scene teardown `primaryPool.ActiveCount == 0 && chainPool.ActiveCount == 0`; `BoomerangCaught` is NOT raised. *Covers EC-17.*
- [Integration] **GIVEN** `arc_flight_time = 0.1 s` (minimum), small asteroid collider at maximum ship-to-target distance within play bounds, **WHEN** primary is thrown, **THEN** the asteroid's `IHittable.TakeDamage` is called exactly once (no tunnel-through); `BoomerangHit` raised with `pierceIndex = 0`. *Covers EC-18.* **🔹 Week-1 Gate**
- [Logic] **GIVEN** chain is `ChainOutbound` and the ship collider layer is in scene, **WHEN** chain's contact detection runs (per selected ADR option), **THEN** no hit event is raised against the ship-layer collider; chain's Physics2D layer mask excludes the ship layer in all configurations. *Covers EC-19.*
- [Logic] **GIVEN** `return_detonate_radius > 0` and ≥ 1 `IHittable` was pierced this arc, **WHEN** `CaughtLate` fires and F4 applies, **THEN** each entity within detonate radius receives a `TakeDamage` call with `base_damage` and a `BoomerangHit` with `pierceIndex = -1`; this is distinct from and cumulative with prior pierce damage this arc. *Covers EC-13 (detonate double-damage intent).*

### Cross-System Integration

- [Integration] **GIVEN** `TargetRegistry` pre-populated with 50 candidates, **WHEN** CR-4 runs, **THEN** the nearest candidate (squared distance, leftmost-X tiebreak within ε = 0.01 wu²) is selected; the call completes within 0.05 ms on WebGL IL2CPP; no heap allocation (no LINQ, no collection-returning method). *Covers G3↔TargetRegistry.*
- [Integration] **GIVEN** G4 subscribes to `BoomerangHit`, `ChainTriggered`, `BoomerangCaught`, `ChainVanished` before a full cycle, **WHEN** a complete cycle executes (throw, outbound pierce, chain trigger, inbound, catch with detonate), **THEN** each handler fires exactly the expected number of times with correct arguments (`pierceIndex = -1` on detonate, not a regular pierce index). *Covers G3↔G4 events.*
- [Integration] **GIVEN** P1a pushes a stat delta to C6 that increases `base_damage` by 2 between two throws, **WHEN** `ArmedForThrow` snapshots `GameStatsContext` for the second throw, **THEN** the second throw uses the updated `base_damage`; the in-flight first throw's damage is not retroactively changed; G3 contains no direct reference to P1a. *Covers G3↔C6 snapshot contract.*
- [Integration] **GIVEN** G5 and G6 each implement `IHittable`, **WHEN** the boomerang contacts one of each in the same arc, **THEN** `IHittable.TakeDamage` is called on both with correct pierce-falloff damage for their respective `pierceIndex`; G3 raises `BoomerangHit` for each; neither receives a call during detonate unless within `return_detonate_radius`. *Covers G3↔G5 and G3↔G6 interface.*
- [Integration] **GIVEN** G8 implements `IBossHittable` (extending or identical to `IEnemyHittable`), **WHEN** the boomerang contacts the boss during `Outbound`, **THEN** `IBossHittable.TakeDamage` is called with correct pierce-falloff damage and `BoomerangHit` is raised; boss contact is treated identically to enemy contact at G3 layer. *Covers G3↔G8 interface.*
- [Integration] **GIVEN** V1 subscribes to G3's throw/contact/return/catch/chain events via `IJuiceRequester`, **WHEN** each event fires during a full cycle, **THEN** `IJuiceRequester.Request` is called with correct `JuiceEventType` and `intensityScalar`; no VFX is missed; no VFX fires for events that did not occur (e.g., no detonate VFX if `return_detonate_radius == 0`). *Covers G3↔V1.*
- [Integration] **GIVEN** A1 subscribes via `IAudioRequester`, **WHEN** a full cycle with pierce + catch executes, **THEN** `sfx_boomerang_impact_crack_heavy` plays for n=0, `sfx_boomerang_impact_crack_mid` for n=1–2, `sfx_boomerang_impact_scrape_light` for n=3+, `sfx_boomerang_catch_chime` at catch, and no audio fires for arc-travel states where no Request call is specified. *Covers G3↔A1.*
- [Integration] **GIVEN** U1 polls `BoomerangState` and subscribes to `BoomerangCaught`, **WHEN** a cycle completes, **THEN** `BoomerangState.cooldown_remaining` decrements monotonically from `throw_cooldown` to 0.0 across render frames; `BoomerangCaught` triggers U1's cooldown-wheel reset within one render frame of catch. *Covers G3↔U1.*

### Performance Budget

- [Logic] **GIVEN** 6 simultaneous `IHittable` entities overlap the boomerang's collider in a single fixed tick (worst-case pierce event), **WHEN** the tick is profiled, **THEN** total GC allocation for that tick is 0 bytes and GC pause duration is < 10 ms. *Covers TD-FEASIBILITY GC criterion.*
- [Logic] **GIVEN** `TargetRegistry` contains 50 candidates, **WHEN** CR-4 is called 1000 times in an EditMode benchmark, **THEN** mean duration < 0.05 ms and 0 bytes heap allocation per call (`GC.GetTotalMemory` before/after batch). *Covers CR-4 budget.*
- [Logic] **GIVEN** the boomerang is `Outbound` during a frame with 6 simultaneous pierce contacts, **WHEN** G3's `OnTick` callback is profiled in WebGL IL2CPP, **THEN** total CPU time charged to G3 within a single tick ≤ 2 ms. *Covers per-tick frame budget.*
- [Config] **GIVEN** implementation ships without gameplay-value literals (damage, cooldown, arc parameters, shake intensities, falloff values) embedded in C# outside `GameStatsContext` or the designated tuning-knob constants file, **WHEN** a grep for magic numbers in G3 files is performed, **THEN** all gameplay values source exclusively from `GameStatsContext`, G3-internal named constants, or config assets — no inline literals. *Covers data-driven discipline.*

### Game Feel (subjective — playtest sign-off)

- [Visual/Feel] **GIVEN** a playtester observes a full throw-to-catch cycle for the first time (no prior session context), **WHEN** asked "how would you describe the boomerang's movement?", **THEN** no playtester uses the words "floaty" or "instant." The default `arc_flight_time = 0.8 s` and arc weight VFX must both contribute to mass perception. **🔹 Week-1 Gate**
- [Visual/Feel] **GIVEN** a playtester is shown a clean n=0 first hit at `base_damage = 1`, **WHEN** they describe the impact, **THEN** no playtester describes it as "soft." The flash + 2-frame hitstop + impact crack must register as a perceptible weight event independent of damage value. **🔹 Week-1 Gate**
- [Visual/Feel] **GIVEN** a playtester sees an n=3+ floor hit (0-frame hitstop, scrape-only audio, 2 shards max) next to an n=0 clean hit, **WHEN** asked to compare, **THEN** the playtester perceives them as different **in kind**, not merely degree. Both events must be visually unambiguous without GDD context. **🔹 Week-1 Gate**
- [Visual/Feel] **GIVEN** a playtester is performing standard throw cycles with audio only (screen covered/eyes closed), **WHEN** the catch occurs, **THEN** the playtester identifies the catch moment by `sfx_boomerang_catch_chime` alone — correctly distinguishing it from impact SFX in ≥ 4 of 5 attempts. **🔹 Week-1 Gate**
- [Visual/Feel] **GIVEN** a gamepad playtester completes 5 consecutive cycles, **WHEN** asked to describe the physical texture of the cycle, **THEN** no gamepad playtester uses "weightless." Rumble at throw, clean hit, and catch must collectively produce a perceivable tactile rhythm (amplitudes TBD at prototype). **🔹 Week-1 Gate**
- [Visual/Feel] **GIVEN** a new playtester has completed 3 throw cycles, **WHEN** the boomerang is `Inbound`, **THEN** the playtester repositions the ship toward the return arc without prompting. **🔹 Week-1 Gate**

### Test Locations

Per `.claude/docs/coding-standards.md` Test Evidence table:

- **Logic** tests → `tests/unit/g3-boomerang/*.cs` (EditMode, NUnit)
- **Integration** tests → `tests/integration/g3-boomerang/*.cs` (PlayMode, NUnit)
- **Visual/Feel** evidence → `production/qa/evidence/g3-boomerang-weekN/` (playtest notes + screenshots + lead sign-off)
- **Config** smoke check → `production/qa/smoke-[date].md` (grep CI step)

## Open Questions

| # | Question | Owner | Deadline | Resolution Path |
|---|----------|-------|----------|-----------------|
| OQ-F6A | Final values for `base_shake_intensity` and `max_shake_intensity` (currently placeholders 0.15 / 0.6) | user + art-director + technical-artist | Week-1 prototype | Validate against G2's shake-implementation units, camera scale, target screen resolutions. May promote to M1 reduced-motion toggle. |
| OQ-KINEMATIC | Kinematic motion implementation — option 1 (Kinematic `Rigidbody2D` + `MovePosition` + `CollisionDetectionMode2D.Continuous`) vs option 2 (pure `Transform.position` + `OverlapCircleNonAlloc`/`CircleCast` polling) | technical-director | Before any G3 implementation begins | Authored ADR via `/architecture-decision` (kinematic boomerang motion) |
| OQ-SHIP-DEATH-CALLBACK | `IShipDestroyedListener` callback mechanism (EC-17) — does the chain-boomerang also receive the callback independently, or does the primary's controller propagate `Dissolving` to the chain? | lead-programmer | During G1/G3 interface ADR | G1/G3 interface ADR |
| OQ-CHAIN-SCALING | Post-MVP `chain_count > 1` semantics — does a 2nd chain fire on the primary's *second* outbound contact (lifting CR-7's first-contact-only rule), or does it spawn multiple chains from the first contact? | game-designer + creative-director | Post-MVP; only if a "multi-chain" mod is proposed | Not applicable at MVP. Max 2 arcs on screen at launch. |
| OQ-DETONATE-ZOOM | Catch-detonate camera zoom-out (2–3% FoV expansion over 0.3 s) feasibility on WebGL | technical-artist | Week-1 prototype | Cut and compensate with VFX if it reads as judder on browser builds. |
| OQ-WEIGHT-FLOOR-FEEL | Do n=3+ floor-damage hits feel **distinct in kind** from clean hits (per Feel Acceptance Criterion 3), or merely **reduced in degree**? | creative-director via playtest | Week-1 prototype (Feel Gate) | Playtest sign-off. If playtesters perceive only "reduced," increase shape/audio differentiation at floor-hit spec before MVP ships. |
