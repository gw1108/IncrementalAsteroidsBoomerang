# C6 Stat Resolver — Research Document

> **Last Updated**: 2026-04-24

---

## Overview

C6 is the stat-resolution layer that sits between the progression systems and the gameplay systems. At run-start, it queries all registered stat producers (P1a Skill Tree, G4 Mod System), combines their deltas into a single frozen `GameStatsContext` struct, and holds it for the duration of the run. Gameplay systems (G1, G3, G5, G6, G7) read from this context; none of them touch P1a or G4 directly.

The context is immutable for the duration of the run. Producers are only queried at run-start. Mid-run tree or mod changes take effect at the next run-start.

---

## Player Fantasy

C6 is invisible to the player. What they feel is its product: the run-start moment when last session's tree decisions load into this run. The boomerang leaves the ship and hits differently — heavier, faster, or punching through ore. The player didn't see a stat sheet. They just felt it.

This is the fantasy of **inheritance** — everything the tree promised is already loaded before the fuel clock starts.

---

## How It Works

### Producers and the IUpgradeSource Contract

C6 uses a pull model. Producers implement `IUpgradeSource` and return a flat list of `StatDelta` structs when queried. They never push. An empty list is valid (no upgrades purchased).

```csharp
public readonly struct StatDelta
{
    public readonly string FieldKey;      // Must be a const from StatKeys static class
    public readonly DeltaMode Mode;       // Additive or Multiplicative
    public readonly DeltaValueType Type;  // Int or Float
    public readonly int   IntValue;
    public readonly float FloatValue;
}

public enum DeltaMode      { Additive, Multiplicative }
public enum DeltaValueType { Int, Float }
```

All `FieldKey` values must come from `StatKeys` (a shared `static class` of `const string` fields). Arbitrary string literals are forbidden.

### Bootstrap and Registration

C5 injects producers into C6 via `Bootstrap(IUpgradeSource[] producers)` during scene-load, before any gameplay tick fires. Injection order is fixed: P1a first, G4 second. This order determines multiplier application priority. C6 has no public `Register`/`Deregister` API — producers cannot self-register.

`Bootstrap()` is only valid in S0 (Uninitialized). A second call in any other state is ignored with an error log.

### Aggregation

C5 calls `TriggerAggregation()` once at run-start, after bootstrap. No other system may call it. If called while a run is already active (S2), the call is ignored with a warning.

**Per-field resolution sequence:**
1. Start with the registered baseline value.
2. Sum all Additive deltas from all producers.
3. Apply all Multiplicative factors in injection order (P1a first, G4 second). Each multiplier compounds: `baseline × 0.9 × 0.8`.
4. Clamp to the field's valid range (validation pass).
5. Write the result into `GameStatsContext`.

**Why additive-first:** A node that adds +2 `arc_radius` and a mod that applies ×1.1 produces a legible result — the 10% applies to the already-boosted value, matching player expectation.

### Integer-Tier Fields Are Additive-Only

`base_damage` and `chain_count` accept only Additive deltas. Multiplicative deltas for these fields are rejected and logged. This enforces the felt 1→2→3 tier jumps for Pillar 1 (Multiplicative Dopamine).

For conditional effects (e.g., "double damage on first contact"), use G3 special-case behavior — not C6.

### Validation Rules

| Rule | Field | Condition | Action |
|---|---|---|---|
| VAL-1 | `arc_flight_time` | Result < 0.1 s | Clamp to 0.1 s. Log warning. Runs first (prevents G3 Bezier divide-by-zero). |
| VAL-2 | All float fields | Result outside [min, max] | Clamp. Log warning. |
| VAL-3 | `base_damage`, `chain_count` | Non-integer delta supplied | Reject delta, log error, continue. |
| VAL-4 | `base_damage` | Result < 1 | Clamp to 1. |
| VAL-5 | `chain_count` | Result > 1 | Clamp to 1. Log warning (ceiling rises post-MVP). |
| VAL-6 | All float fields | Multiplicative factor ≤ 0.0 | Reject multiplier, log error. |

VAL-1 runs first. All other clamps follow.

### Producer Fault Isolation

If a producer throws during query, C6 logs the error, skips that producer's deltas entirely, and continues with the rest. The run starts with whatever the remaining producers contribute (possibly baseline-only). The error must be visible — this is not a silent fallback.

### Context Immutability

After `TriggerAggregation()`, `GameStatsContext` does not change until the next run-start. G3 can snapshot the context once per throw knowing no mid-flight stat change will invalidate arc geometry.

### Consumer Access

Each gameplay system holds a `[SerializeField] private C6StatResolver _statResolver` reference wired in the scene by C5. Consumers call `_statResolver.GetContext()` at their trigger point. They do not use `FindObjectOfType` or a singleton. Consumers should cache the struct copy at run-start — not poll per-frame.

---

## GameStatsContext Field Inventory

**Confirmed** fields are locked by G3's registry. **Provisional** fields are expected by downstream GDDs; their baselines are TBD.

| Field | Type | Baseline | Range | Status | Consumer |
|---|---|---|---|---|---|
| `base_damage` | int | 1 | 1–∞ (ceiling TBD) | Confirmed | G3 |
| `throw_cooldown` | float | 0.8 s | 0.1–10.0 s | Confirmed | G3 |
| `arc_radius` | float | 5.0 wu | 0.0–20.0 wu | Confirmed | G3 |
| `arc_flight_time` | float | 0.8 s | **HARD FLOOR 0.1 s**, ceiling 10.0 s | Confirmed | G3 |
| `pierce_falloff` | float | 0.35 | 0.0–1.0 | Confirmed | G3 |
| `chain_count` | int | 0 | 0–1 (MVP max) | Confirmed | G3 |
| `return_detonate_radius` | float | 0.0 wu | 0.0–10.0 wu | Confirmed | G3 |
| `ship_move_speed` | float | TBD | TBD | Provisional-G1 | G1 |
| `ship_damage_resistance` | float | TBD | 0.0–1.0 | Provisional-G1 | G1 |
| `ship_invulnerability_duration` | float | TBD | TBD | Provisional-G1 | G1 |
| `mining_yield_multiplier` | float | 1.0 | min 0.1, ceiling TBD | Provisional-G5 | G5 |
| `mining_crack_threshold_delta` | int | 0 | TBD | Provisional-G5 | G5 |
| `enemy_stagger_duration_multiplier` | float | 1.0 | 0.0–N | Provisional-G6 | G6 |

No tree node or mod may reference a provisional field in production code until that field is confirmed.

---

## States

| State | Name | Entry | Behavior |
|---|---|---|---|
| **S0** | Uninitialized | Scene load | No producers. Context is zeroed default. |
| **S1** | Ready | C5 calls `Bootstrap()` | Producers injected. Context unresolved (baseline-only). |
| **S2** | Run-Active | C5 calls `TriggerAggregation()` | Context resolved and frozen. Re-aggregation calls ignored. |
| **S3** | Run-End | C5 calls `NotifyRunEnd()` | Context stale. U4 may still read for display. |

**Transitions:**
- S0 → S1: C5 completes bootstrap injection.
- S1 → S2: C5 calls `TriggerAggregation()` at run-start.
- S2 → S3: C5 calls `NotifyRunEnd()` (player death or zone clear).
- S3 → S1: Player presses Launch. Producers remain; context clears.
- Any → S0: Scene teardown.

Happy path: `S1 → S2 → S3 → S1 → …`

---

## Formulas

### General Field Resolution

```
V_final = clamp( (V_baseline + SUM_add) × PROD_mul, V_min, V_max )

SUM_add  = Σ a_i    (all valid Additive deltas from all producers)
PROD_mul = Π m_j    (all valid Multiplicative factors, P1a first, G4 second)
```

For integer-tier fields (`base_damage`, `chain_count`): `PROD_mul = 1.0` always.

| Symbol | Description |
|---|---|
| `V_baseline` | Registered baseline; never modified by producers |
| `a_i` | One Additive delta from one producer |
| `m_j` | One Multiplicative factor; 1.0 = no-op; ≤ 0.0 rejected |
| `V_min`, `V_max` | Field-specific clamp bounds |

### arc_flight_time Safety Clamp

`arc_flight_time_final = clamp( (T_baseline + SUM_add_T) × PROD_mul_T, 0.1, 10.0 )`

The 0.1 s floor is non-negotiable — G3's Bezier evaluation divides by `arc_flight_time`. Changing it requires a G3 code change.

**Example — aggressive speed build hitting the floor:**
- Baseline 0.8 s; three −0.25 s nodes → SUM_add = −0.75; one ×0.5 mod
- Pre-clamp: (0.8 − 0.75) × 0.5 = 0.025 s → clamped to **0.1 s**

### Worked Example

*P1a: +1 `base_damage`, +1.0 `arc_radius`, −0.1 `throw_cooldown`. G4: ×0.85 `throw_cooldown`.*

| Field | Baseline | +Add | ×Mul | Result |
|---|---|---|---|---|
| `base_damage` | 1 | +1 | 1.0 | **2** |
| `arc_radius` | 5.0 | +1.0 | 1.0 | **6.0 wu** |
| `throw_cooldown` | 0.8 | −0.1 | ×0.85 | **0.595 s** |
| `arc_flight_time` | 0.8 | 0 | 1.0 | **0.8 s** |

Note: the ×0.85 mod applies to the post-additive 0.7 s value — additive-first guarantee.

---

## Edge Cases

**State machine:**
- `TriggerAggregation()` in S0: log error, stay in S0, do not produce context.
- `TriggerAggregation()` in S2: ignore, log warning, existing context unchanged.
- `NotifyRunEnd()` in S1 (run never started): log warning, transition to S3 anyway.
- `GetContext()` in S0 or S1: return zeroed-default struct, log warning. G3 must suppress throws when `arc_flight_time < 0.1`.
- `GetContext()` in S3: return last resolved context, log warning. U4 is explicitly permitted.
- Scene teardown during S2: clear producer list, transition to S0.

**Producer contract:**
- Duplicate `(field, mode)` pairs from one producer: both applied, warning logged.
- Null or empty field name: treat as unknown field (CR-6 — log error, discard).
- Multiplicative delta for `base_damage` or `chain_count`: reject and log; other valid deltas from same producer still apply.
- Null producer in bootstrap array: skip it, log configuration error.
- Same producer injected twice: detect via reference equality at bootstrap; reject duplicate, log error.
- Both producers fault: two distinct error logs; run starts with baseline-only context.

**Consumer responsibilities:**
- G3 receiving pre-aggregation context (S0/S1): do not throw — treat `arc_flight_time < 0.1` as "not ready."
- G3 struct copy from S2 that survives into a new run: must re-call `GetContext()` at each run-start, not reuse.

---

## Dependencies

| System | Role | Notes |
|---|---|---|
| **C5 Scene & Mode Flow** | Lifecycle controller | Calls `Bootstrap()`, `TriggerAggregation()`, `NotifyRunEnd()`. Hard dependency. |
| **P1a Skill Tree Architecture** | Stat producer | Implements `IUpgradeSource`. Injected first. |
| **G4 Mod System** | Stat producer | Implements `IUpgradeSource`. Injected second. |
| **G3 Boomerang Weapon** | Stat consumer (hard) | Calls `GetContext()` once per throw at `ArmedForThrow`. Arc, damage, and timing entirely driven by context. |
| **G1 Player Ship Controller** | Stat consumer (soft) | Calls `GetContext()` at run-start for ship parameters (provisional fields). |
| **G5 Asteroid Mining** | Stat consumer (soft) | Reads `mining_yield_multiplier`, `mining_crack_threshold_delta` (provisional). |
| **G6 Enemy System** | Stat consumer (soft) | Reads `enemy_stagger_duration_multiplier` (provisional). |
| **G7 Wave & Spawn Director** | Stat consumer (soft) | Fields TBD in G7 GDD. |
| **U2 Skill Tree UI** | Read-only on user input | Calls `GetContext()` when node panel opens. Not a per-frame poll. |

**C5 ordering requirement:** P1a's purchase-commit/save-restore step must complete before C5 calls `TriggerAggregation()`. If violated, C6 silently produces a pre-purchase context and the player's run-start moment feels broken.

---

## Tuning Knobs

### Confirmed Field Baselines

| Field | Baseline | Safe Range | Risk at Extremes |
|---|---|---|---|
| `base_damage` | 1 | 1–∞ (ceiling TBD) | High baseline: player never feels the P1 ramp from run 1. |
| `throw_cooldown` | 0.8 s | 0.3–5.0 s | < 0.3 s: spam kills feel. > 5.0 s: run too short to demonstrate tree power. |
| `arc_radius` | 5.0 wu | 0.5–15.0 wu | < 0.5 wu: near straight-line. > 15.0 wu: arc clips play area. |
| `arc_flight_time` | 0.8 s | 0.3–3.0 s | < 0.1 s: VAL-1 hard floor (G3 safety). > 3.0 s: boomerang hangs; P3 feel degrades. |
| `pierce_falloff` | 0.35 | 0.0–1.0 | 0.0: all contacts deal full damage (very powerful). 1.0: pierce feels weak. |
| `chain_count` | 0 | 0–1 (MVP) | Above 1: cascade mechanics undesigned at MVP; VAL-5 clamps. |
| `return_detonate_radius` | 0.0 wu | 0.0–8.0 wu | > 8.0 wu: AoE one-shots most enemies; undermines P2 mastery. |

### Architectural Constants (Not Tunable Without Code Change)

| Constant | Value | Why Fixed |
|---|---|---|
| `arc_flight_time` hard floor | 0.1 s | G3 Bezier divides by `arc_flight_time` — changing requires G3 refactor. |
| `chain_count` MVP ceiling | 1 | Chain cascade above 1 is undesigned. Raise post-MVP only. |
| Producer injection order | P1a first, G4 second | Determines multiplicative application order. Architecture-level decision. |

---

## Open Questions

- **OQ-C6-1** — What is the design ceiling for `base_damage`? P1b must register it in entities.yaml. Until then `base_damage` has no upper clamp — P1b node design could produce one-shot kills on all enemies. *Owner: P1b GDD.*

- **OQ-C6-2** — Does any tree node or mod use multiplicative mode on float fields at MVP? If not, Mode B is implemented but never exercised. Acceptable — stays in spec for future nodes. *Owner: G4 GDD.*

- **OQ-C6-3** — Does E1 (Fuel Economy) need any `GameStatsContext` fields (e.g., fuel-per-kill modifier)? *Owner: E1 GDD.*

- **OQ-C6-4** — Provisional G1 fields (`ship_move_speed`, `ship_damage_resistance`, `ship_invulnerability_duration`) have no baselines. G1 GDD must confirm them. *Owner: G1 GDD.*

- **OQ-C6-6** — Does U2 need a stat preview before purchase ("Damage: 4 → 5")? Current design supports reading the current resolved context only. Preview would require a `PreviewContext(pendingDelta)` method or P1a handling it locally. **Known UX risk:** additive-under-multiplier silently changes a purchased node's effective value when a mod is added — quiet breach of the "tree is a contract" (P5) promise. Must be flagged when P1a and U2 GDDs are authored. *Owner: P1a and U2 GDD.*
