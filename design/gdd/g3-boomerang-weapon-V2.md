# G3 Boomerang Weapon — V2

> **Status**: Implementation Spec (simplified from V1)
> **Author**: user + Claude
> **Last Updated**: 2026-04-24
> **Replaces**: g3-boomerang-weapon.md for implementation purposes

## Overview

The boomerang is the game's only weapon. It auto-targets the nearest enemy or asteroid, travels on a scripted Bezier arc, passes through anything in its path (pierce), and returns to the player. The player does not aim or fire — skill lives in positioning to maximize contacts and catch the return arc.

Three implementation decisions for this version:
1. Movement via Kinematic `Rigidbody2D` + `MovePosition` (Option 1 from V1 ADR).
2. All logic runs in `Update()` / `FixedUpdate()` — no custom tick system.
3. Boomerang instances are `Instantiate`/`Destroy` — no object pool.

---

## Stats (from GameStatsContext)

| Field | Baseline | Description |
|---|---|---|
| `BaseDamage` | 1 | Integer damage for first pierce hit (n=0). |
| `ThrowCooldown` | 0.8 s | Time between catch and next throw. Seeded at catch. |
| `ArcRadius` | 5.0 wu | Peak lateral deviation of the Bezier arc from the straight line. |
| `ArcFlightTime` | 0.8 s | Duration of each leg (outbound and inbound). Hard floor: 0.1 s. |
| `PierceFalloff` | 0.35 | Per-hit damage retention loss. 0 = no falloff. 1 = only first hit full damage. |
| `ChainCount` | 0 | If ≥ 1, spawns a second boomerang on first outbound contact. MVP max = 1. |

C6StatResolver provides these via `GetContext()`. Boomerang snapshots them once at throw commit.

---

## Scripts

### `BoomerangController` (on Player GameObject)
- Drives the state machine.
- Holds serialized reference to `PlayerController` and `C6StatResolver`.
- Holds serialized reference to the boomerang prefab.
- Finds nearest `BoomerangTarget` using `BoomerangTarget.All` static list.
- Instantiates/destroys `BoomerangProjectile`.

### `BoomerangProjectile` (on the boomerang prefab)
- Kinematic `Rigidbody2D` + `CircleCollider2D` (IsTrigger = true).
- Receives arc parameters from `BoomerangController` at spawn.
- Moves via `rb.MovePosition()` each `FixedUpdate`.
- Reports `OnTriggerEnter2D` contacts back to `BoomerangController` via direct method call.
- Interpolates render position in `Update` using `previousPosition` / `currentPosition` lerp.

### `BoomerangTarget` (on enemy/asteroid/boss GameObjects)
- Maintains a `static List<BoomerangTarget> All` — each instance adds itself in `OnEnable`, removes in `OnDisable`.
- Has `bool IsAlive` property.
- Has `void TakeDamage(int damage, Vector2 contactPoint)` method.

### `ChainBoomerangProjectile` (on chain boomerang prefab — spawned when `chain_count > 0`)
- Identical physics setup to `BoomerangProjectile`.
- Self-contained: receives arc parameters at spawn, flies to target, returns to spawn position, `Destroy(gameObject)` on arrival.
- Reports contacts via same callback pattern. Does NOT trigger another chain.

---

## State Machine (Primary Boomerang)

```
Idle → ArmedForThrow → Outbound → PeakTurn → Inbound → CaughtLate → Idle
```

| State | What happens | Exit condition |
|---|---|---|
| **Idle** | Cooldown counts down each Update. | Cooldown ≤ 0 AND ≥ 1 valid target → `ArmedForThrow` |
| **ArmedForThrow** | Find nearest target. Snapshot `GameStatsContext`. Compute arc (F1 points). Instantiate `BoomerangProjectile`. | Next frame → `Outbound` |
| **Outbound** | Projectile integrates along outbound Bezier. Contacts fire via `OnTriggerEnter2D`. | `t ≥ ArcFlightTime` → `PeakTurn` |
| **PeakTurn** | One frame: assign inbound state on projectile. | Immediately → `Inbound` |
| **Inbound** | Projectile integrates along inbound Bezier, homing on player's live position. Catch-check each Update. | Boomerang close enough to player → `CaughtLate` |
| **CaughtLate** | One frame: seed cooldown timer. Destroy projectile. | Next frame → `Idle` |

---

## Core Rules

**CR-1 Throw conditions.** Throw fires when ALL are true:
1. State is `Idle`.
2. `cooldownRemaining ≤ 0`.
3. At least one valid target in `BoomerangTarget.All` with `IsAlive == true`.

**CR-2 Arc shape.** Two-leg quadratic Bezier (F1 outbound, F2 inbound). Arc sweeps to the RIGHT of the ship→target vector on outbound, to the LEFT on inbound. Computed once at throw commit from ship position and target position. Inbound re-computes its control point each tick from the live player position.

**CR-4 Target acquisition.** At throw commit, iterate `BoomerangTarget.All` once. Track minimum squared distance to player. If two candidates are within ε = 0.01 wu², tiebreak by smallest world-X. Skip any with `IsAlive == false`. Return the winning reference (or null if none).

**CR-5 Contact.** On `OnTriggerEnter2D` (any `BoomerangTarget` layer):
1. Compute `damage(n)` via F3 where n = contact index for this arc.
2. Call `target.TakeDamage(damage, contactPoint)`.
3. Increment contact index.
4. If this is the primary's first outbound contact (`n == 0`, state is `Outbound`) and `ChainCount > 0`: fire chain (CR-7).

**CR-6 Pierce.** Arc is unaltered on contact. The boomerang passes through every target. No pierce cap.

**CR-7 Chain.** Fires when CR-5 step 4 is triggered:
1. Find nearest valid `BoomerangTarget` excluding the entity just hit. If none, abort silently.
2. Instantiate `ChainBoomerangProjectile` at contact position.
3. Provide arc parameters: origin = contact position, target = chain target. Inherit parent's stat snapshot.
4. Chain returns to its **spawn position** and destroys itself — it does NOT return to the player.
5. Chain's first contact does NOT trigger another chain.

**CR-8 Return phase.** When `t ≥ ArcFlightTime` during `Outbound`, `BoomerangController` transitions to `PeakTurn` then `Inbound`. No re-acquisition.

**CR-9 Catch.** Each Update during `Inbound`, check distance from boomerang world position to `PlayerController.transform.position`. If `≤ catchRadius` (tuning knob, e.g. 0.8 wu):
1. Seed `cooldownRemaining = ThrowCooldown`.
2. Destroy `BoomerangProjectile`.
3. Transition to `CaughtLate` → `Idle`.

**CR-10 No target.** If CR-4 returns null, no throw fires, cooldown does not reset, state stays `Idle`.

---

## Formulas

### F1 — Outbound Arc Position (Quadratic Bezier)

```
forward    = normalize(targetPos - shipPos)
perp_right = -Vector2.Perpendicular(forward)   // Unity returns (-y, x) = screen-left; negate for right

P0 = shipPos        (frozen at throw commit)
P2 = targetPos      (frozen at throw commit)
P1 = (P0 + P2) / 2 + perp_right * (ArcRadius * 2)

t_norm = Clamp01(elapsed / ArcFlightTime)

pos_out(t_norm) = (1 - t_norm)^2 * P0
                + 2 * (1 - t_norm) * t_norm * P1
                + t_norm^2 * P2
```

`perp_right`, `P0`, `P1`, `P2` are stored on the projectile instance. `t_norm * 2` ensures the Bezier peak deviation equals `ArcRadius` exactly.

**Worked example** — ship (0,0), target (10,0), ArcRadius = 5:
- `forward = (1,0)`, `perp_right = (0,-1)`
- `P1 = (5,0) + (0,-1)*10 = (5,-10)`
- At `t_norm = 0.5`: `pos = (5.0, -5.0)` — peak deviation = 5.0 wu ✓

### F2 — Inbound Arc Position (Quadratic Bezier, homing)

```
perp_left = -perp_right    (frozen, computed from F1)

P2_in   = P2               (frozen apex position)
P3_live = playerController.transform.position   (sampled every FixedUpdate)
P1_in   = (P2_in + P3_live) / 2 + perp_left * (ArcRadius * 2)

s_norm = Clamp01(elapsed_inbound / ArcFlightTime)

pos_in(s_norm) = (1 - s_norm)^2 * P2_in
               + 2 * (1 - s_norm) * s_norm * P1_in
               + s_norm^2 * P3_live
```

`P1_in` recomputes every tick from the live player position — this gives the homing arc feel.

### F3 — Pierce Damage Falloff

```
damage(n) = max(1, floor(BaseDamage * (1 - PierceFalloff)^n))
```

`n` = 0-indexed contact count for this arc. Resets to 0 each new throw.

**Worked example** — BaseDamage = 5, PierceFalloff = 0.35:

| n | damage(n) |
|---|---|
| 0 | 5 |
| 1 | 3 |
| 2 | 2 |
| 3 | 1 |
| 4+ | 1 (floor) |

### F5 — Cooldown Timer

```
cooldownRemaining = max(0, cooldownRemaining - Time.deltaTime)
```

Seeded to `ThrowCooldown` when `CaughtLate` resolves. Counts down in `Idle`. Throw fires when ≤ 0.

---

## Edge Cases

**EC-1 Target dies between selection and arc commit.** Arc commits using the last known position as a `Vector2` — no live reference held post-commit. No NullRef.

**EC-5 `ArcFlightTime` at or below zero.** Clamp to 0.1 s in `ArmedForThrow` before storing on the projectile. Prevents divide-by-zero in `t_norm`. Log a warning in development builds.

**EC-7 `ArcRadius = 0`.** `P1 = midpoint(P0, P2)` — Bezier degenerates to a straight line. Valid, no crash.

**EC-8 Ship and target at same position.** `normalize(Vector2.zero)` returns `Vector2.zero` in Unity. All Bezier samples collapse to one point. Boomerang sits still, then returns. Ugly but not a crash.

**EC-11 No valid chain target.** If all candidates are the entity just hit or dead, chain is silently aborted. No chain spawned, primary continues.

**EC-12 Chain still in flight when primary is caught.** Chain continues independently to its spawn position and destroys itself. Primary catch seeds cooldown normally.

**EC-15 Pause during flight.** Unity's `timeScale = 0` stops `Update` and `FixedUpdate`. Boomerang freezes at last position. Resumes correctly on unpause.

---

## Tuning Knobs

| Parameter | MVP Start | Safe Range | Notes |
|---|---|---|---|
| `BaseDamage` | 1 | 1–∞ (int) | From C6/tree |
| `ThrowCooldown` | 0.8 s | 0.3–5.0 s | From C6/tree |
| `ArcRadius` | 5.0 wu | 0.0–20.0 wu | From C6/tree |
| `ArcFlightTime` | 0.8 s | 0.1–5.0 s (0.1 s floor) | From C6/tree |
| `PierceFalloff` | 0.35 | 0.0–1.0 | From C6/tree |
| `ChainCount` | 0 | 0–1 | From C6/tree. 0 = chain disabled |
| `catchRadius` | 0.8 wu | 0.4–2.0 wu | G3 internal constant on BoomerangController |
| `minArcFlightTime` | 0.1 s | — | Safety floor, not tunable |

---

## Dependencies

| System | What G3 needs |
|---|---|
| `PlayerController` | `transform.position` (throw anchor + catch target). `CircleCollider2D` layer for trigger exclusion. |
| `C6StatResolver` | `GetContext()` called once at throw commit. |
| `BoomerangTarget` | `IsAlive`, `TakeDamage(int, Vector2)`, static `All` list. |

G3 does not depend on: camera system, audio, VFX, UI, object pool, or fixed-timestep system.

---

## Acceptance Criteria

### Core mechanics
- [ ] Boomerang throws automatically when cooldown ≤ 0 and a valid target exists.
- [ ] Arc sweeps right on outbound, left on inbound.
- [ ] At t_norm = 0.5 with ship (0,0) / target (10,0) / ArcRadius 5.0: position is (5.0, -5.0) ± 0.01 wu.
- [ ] Inbound leg homes on live player position — moving the player changes the catch point.
- [ ] Catch fires when boomerang is within `catchRadius` of player during `Inbound`. Cooldown seeds. Projectile destroyed.
- [ ] If no valid target exists, no throw fires and cooldown does not reset.
- [ ] Pierce: boomerang passes through all contacted targets. Arc unaltered.
- [ ] Pierce falloff: BaseDamage=5, PierceFalloff=0.35 → damage sequence [5, 3, 2, 1, 1, 1]. No hit ever deals 0 damage.

### Chain
- [ ] With ChainCount=1: first outbound contact spawns a ChainBoomerangProjectile at contact position.
- [ ] Chain targets nearest valid non-contact target. If none, no chain spawns.
- [ ] Chain returns to its spawn position and destroys itself — does not return to player.
- [ ] Chain's first contact does NOT spawn another chain.

### Safety
- [ ] ArcFlightTime values of 0.0, -1.0, 0.05 all clamp to 0.1 s with no exception.
- [ ] Target destroyed mid-flight: no NullReferenceException; boomerang completes arc to P2.
- [ ] Chain still in flight when primary caught: chain continues independently, primary cooldown seeds normally.
