# C6 Stat Resolver & Upgrade Aggregation

> **Status**: Revised — pending re-review or Approved (see review log)
> **Creative Director Review (CD-GDD-ALIGN)**: APPROVED 2026-04-24
> **Independent Design Review**: NEEDS REVISION resolved 2026-04-24 — 6 blocking items addressed, 8 recommended revisions applied
> **Author**: user + /design-system orchestration
> **Last Updated**: 2026-04-24
> **Implements Pillar**: P4 The Tree IS the Game (primary — this is the layer that makes P4 mechanically real); P1 Multiplicative Dopamine (integer-tier discipline enforced here); P2 Mastery in where the player moves (arc_flight_time safety floor enforced here)

## Overview

C6 Stat Resolver & Upgrade Aggregation is the stat-resolution infrastructure layer that decouples the progression hub from every stat-consuming gameplay system in the game. At run-start, C6 queries all registered `IUpgradeSource` producers — P1a (Skill Tree Architecture) and G4 (Mod System) at MVP — aggregates their stat deltas into a single frozen `GameStatsContext` value struct, and holds it for the duration of the run. Gameplay systems (G1 Player Ship Controller, G3 Boomerang Weapon, G5 Asteroid Mining, G6 Enemy System, G7 Wave & Spawn Director) query this context directly; none of them reference P1a or G4. `GameStatsContext` holds all player-affecting stat parameters: boomerang behavior (damage, arc shape, cooldown, pierce, chain, detonate), ship controller parameters, and any mining or combat modifiers that tree nodes or mods expose. The context is immutable for the duration of the run — producers are queried only at run-start, and mid-run changes take effect at the next run-start. This immutability is what makes P3 (Read the Arc) enforceable: G3 snapshots the context once per throw with confidence that no mid-flight stat change will invalidate its arc computation.

## Player Fantasy

C6 is infrastructure. Players never interact with it, see it, or name it. What they feel is its product: the run-start moment when the tree becomes real.

The miner presses Launch. The boomerang leaves the ship. There is no stat sheet, no floating "+1 Damage" tooltip. There is only the boomerang landing against the first asteroid and the sound it makes — heavier, or faster, or punching through to the ore behind. That difference is C6 working. The miner cannot articulate it. They feel it through the throw.

This is the fantasy of *inheritance* — last session's decisions, loaded into this run before the fuel clock starts. The run-start moment is the delivery receipt: everything the tree promised is already there. The miner steps into the field carrying the work they did in the shop, and the field is already easier than it was last time. Precisely as hard as the tree they built suggests it should be.

**Pillar alignment:**
- **P1 Multiplicative Dopamine** — the felt shift at run-start is the integer-tier jump made manifest. C6's aggregation is why the first throw of run N+1 feels different from run N.
- **P5 The Tree IS the Game** — C6 is the mechanism that fulfils P5's promise. Without a reliable translation layer between tree purchases and gameplay behavior, P5 is aspiration; with C6, it is a contract.

*The player is not supposed to notice C6. They are supposed to trust the tree, because C6 makes the tree trustworthy.*

## Detailed Design

### Core Rules

**CR-1 — IUpgradeSource contract (pull model).**
C6 drives all queries. Producers (`IUpgradeSource` implementors) are passive data suppliers between aggregation calls. A producer does not push deltas; it waits to be queried. The interface requires one method: return a flat list of `StatDelta` structs when C6 calls it. A producer may return an empty list if no upgrades have been purchased — this is valid and treated as zero-delta.

**`StatDelta` struct (discriminated union — zero allocation, no boxing):**

```csharp
public readonly struct StatDelta
{
    public readonly string FieldKey;      // Must be a const from StatKeys static class — see below
    public readonly DeltaMode Mode;       // Additive or Multiplicative
    public readonly DeltaValueType Type;  // Int or Float (discriminator)
    public readonly int   IntValue;       // Valid when Type == Int
    public readonly float FloatValue;     // Valid when Type == Float
}

public enum DeltaMode      { Additive, Multiplicative }
public enum DeltaValueType { Int, Float }
```

**Canonical key requirement:** All `FieldKey` values must be sourced from a shared `static class StatKeys` containing `const string` fields (e.g., `StatKeys.BaseDamage`, `StatKeys.ArcRadius`). Arbitrary string literals returned from `IUpgradeSource` are forbidden — this prevents GC allocation from string construction and protects against field-name drift. C6 compares keys via `string.Equals(key, StatKeys.X, StringComparison.Ordinal)`.

**CR-2 — Registration via C5 bootstrap injection.**
C6 does not self-wire. C5 (Scene & Mode Flow) holds the ordered producer list and injects P1a and G4 into C6 at scene-load during its bootstrap step, before any gameplay tick fires. This is the only registration mechanism. There is no `Register`/`Deregister` API on C6. Producers cannot register themselves. The injection order is fixed: P1a first, G4 second. This order is binding and determines multiplier application priority (see CR-4).

**Injection method (resolves OQ-C6-7):** C6 exposes one public injection entry point: `Bootstrap(IUpgradeSource[] producers)`. C5 calls this method exactly once during scene-load bootstrap, passing `new IUpgradeSource[] { p1a, g4 }`. The internal producer list is stored as `IUpgradeSource[]` (fixed-size array — no `List<T>` to avoid heap overhead). `Bootstrap()` is callable only in S0; a second call in any other state logs an error and is ignored.

**CR-3 — Aggregation is triggered once per run by C5 only.**
C6 exposes a single `TriggerAggregation()` method. C5 calls it at run-start, after bootstrap injection but before the player ship and any gameplay system begins ticking. No other system may call this method. Mid-run re-aggregation is explicitly forbidden: if `TriggerAggregation()` is called while a run is already active, C6 ignores the call and logs a warning.

**CR-4 — Aggregation sequence per field.**
For each field in the `GameStatsContext` inventory, C6 applies deltas in this fixed order:

1. Begin with the registered baseline value for the field.
2. Sum all Additive deltas from all producers (both P1a and G4). Addition is commutative — producer order does not affect the additive sum.
3. Apply all Multiplicative factors from all producers in injection order (P1a's multipliers first, G4's second). Multiplicative factors are applied sequentially: a factor of 0.9 from P1a followed by 0.8 from G4 produces `baseline × 0.9 × 0.8`.
4. Run the validation pass (CR-8). Clamping occurs after all producers have contributed, never inside the aggregation loop.
5. Write the validated result into `GameStatsContext`.

Additive-first, multiplicative-second rationale: a tree node that adds "+2 arc_radius" and a mod that applies "×1.1 arc_radius" produces a legible result — the 10% applies to the already-boosted radius, which matches the player's mental model when reading both sources.

**CR-5 — Integer-tier fields are additive-only.**
`base_damage` and `chain_count` accept only Additive deltas. If a producer supplies a Multiplicative delta for either field, C6 rejects that specific delta, logs an error with producer identity and field name, and continues with the remaining valid deltas. C6 does not round or truncate a rejected multiplicative delta — it is discarded.

**Known design constraint:** This rule is intentional for early-game feel (P1 Multiplicative Dopamine requires felt 1→2→3 tier jumps, not diluted multipliers). However, future mid-tree node concepts requiring a *conditional* multiplier on `base_damage` (e.g., "double damage on first contact per throw") cannot exist in the C6 pipeline and must be implemented as G3 special-case behavior. This is a deliberate tradeoff: CR-5 is the pillar-enforcement mechanism for P1; G3's contact logic is the escape hatch for conditional effects that cannot be expressed as simple flat deltas.

**CR-6 — Unknown field names are rejected, not silently dropped.**
If a producer supplies a delta for a field name not present in the `GameStatsContext` field inventory, C6 logs an error (producer identity + field name + value) and discards the delta. The rest of that producer's valid deltas are still applied. This protects against a P1b node referencing a field that was renamed or never added to the context.

**CR-7 — Producer fault isolation.**
If a producer's delta query throws an unhandled exception, C6 logs the error, skips that entire producer's deltas, and proceeds with the remaining producers. The run starts with whatever context the remaining producers produce (which may be baseline-only). C6 emits a prominent diagnostic log identifying the faulting producer. This is not a silent fallback — a human must see it.

**CR-8 — Validation rules (applied after full aggregation).**

| Rule | Field | Condition | Action |
|---|---|---|---|
| VAL-1 | `arc_flight_time` | Result < 0.1 s | Clamp to 0.1 s. Log warning with producer chain. Safety-critical (P2 / division-by-zero in G3 Bezier). Applied first before all other clamps. |
| VAL-2 | All float fields | Result outside registered [min, max] | Clamp to range. Log warning (balance tuning will routinely brush against limits during development). |
| VAL-3 | `base_damage`, `chain_count` | Non-integer delta supplied by producer | Reject delta, log error, continue. |
| VAL-4 | `base_damage` | Result < 1 | Clamp to 1. `pierce_damage_falloff` formula depends on base_damage ≥ 1; zero-damage hits violate P4 (every hit must produce visible feedback). |
| VAL-5 | `chain_count` | Result > 1 (MVP ceiling) | Clamp to 1. Log warning (not error — ceiling rises post-MVP when chain cascade mechanics are designed). |
| VAL-6 | All float fields | Multiplicative factor ≤ 0.0 | Reject multiplier, log error. A zero factor zeroes the field; a negative inverts it. Neither is ever a correct player-facing value. |
| VAL-7 | (order) | — | VAL-1 runs first. All other field clamps run after, confirmed fields first, then provisional. |

**CR-9 — GameStatsContext is a blittable readonly value struct.**
All fields are value types (int, float). No reference types, no arrays. This constraint is permanent: if a future field requires a reference type, it does not belong in `GameStatsContext`. Consumers take a struct copy on read — no heap allocation, no reference aliasing, no possibility of mid-run mutation from the consumer side. C6's internal aggregation loop uses index-based `for` iteration over the injected producer list (not `foreach`) per the hot-path allocation prohibition in technical preferences.

**CR-10 — Consumer access pattern: Inspector-serialized `[SerializeField]` reference.**
Each gameplay system that consumes `GameStatsContext` (G1, G3, G5, G6, G7) holds a `[SerializeField] private C6StatResolver _statResolver` field, wired in the scene by C5 or the scene setup. Consumers call `_statResolver.GetContext()` at their trigger point; they do not hold a static reference, use `FindObjectOfType`, or access a singleton. This pattern is required by `coding-standards.md` (all public methods unit-testable via dependency injection — tests inject a real or test-double C6 instance via the serialized field using reflection or a constructor overload).

**CR-11 — Context is immutable for the duration of the run.**
After `TriggerAggregation()` completes, `GameStatsContext` does not change until the next run-start. Tree purchases in the between-run shop update P1a's internal state but do not re-trigger C6. The new context takes effect at the next run's `TriggerAggregation()` call. This immutability is what makes P2 (Mastery in where the player moves) enforceable: G3 snapshots the context once per throw knowing no mid-flight stat change will alter arc geometry.

**CR-12 — First-run baseline behavior.**
If producers are injected but return empty delta lists (no upgrades purchased — first run), C6 produces a context containing only baseline values. This is valid and expected. If no producers are injected at all, behavior is identical to baseline-only. Neither condition is an error.

---

### GameStatsContext Field Inventory

All fields in the `GameStatsContext` struct. **Confirmed** fields are locked by G3's registry entries and cannot be changed without a registry update. **Provisional** fields are expected by downstream system GDDs; their baselines are TBD until those GDDs are authored.

| Field | Type | Baseline | Range | Status | Primary Consumer |
|---|---|---|---|---|---|
| `base_damage` | int | 1 | 1–∞ (ceiling TBD in P1b) | **Confirmed** (G3) | G3 |
| `throw_cooldown` | float | 0.8 s | 0.1–10.0 s | **Confirmed** (G3) | G3 |
| `arc_radius` | float | 5.0 wu | 0.0–20.0 wu | **Confirmed** (G3) | G3 |
| `arc_flight_time` | float | 0.8 s | **HARD FLOOR 0.1 s**, ceiling 10.0 s | **Confirmed** (G3) | G3 |
| `pierce_falloff` | float | 0.35 | 0.0–1.0 | **Confirmed** (G3) | G3 |
| `chain_count` | int | 0 | 0–1 (MVP max) | **Confirmed** (G3) | G3 |
| `return_detonate_radius` | float | 0.0 wu | 0.0–10.0 wu | **Confirmed** (G3) | G3 |
| `ship_move_speed` | float | TBD (G1) | TBD (G1) | **Provisional-G1** | G1 |
| `ship_damage_resistance` | float | TBD (G1) | 0.0–1.0 | **Provisional-G1** | G1 |
| `ship_invulnerability_duration` | float | TBD (G1) | TBD (G1) | **Provisional-G1** | G1 |
| `mining_yield_multiplier` | float | 1.0 | **V_min = 0.1** (not 0.0) — a multiplier of 0.0 produces zero yield; ceiling TBD in P1b | **Provisional-G5** | G5 |
| `mining_crack_threshold_delta` | int | 0 | -(baseline crack threshold)–+N | **Provisional-G5** | G5 |
| `enemy_stagger_duration_multiplier` | float | 1.0 | 0.0–N | **Provisional-G6** | G6 |

Provisional field baselines are TBD until their owning consumer GDD is authored. Until confirmed, C6 initialises provisional fields to the values shown. No tree node or mod may reference a provisional field in production code until that field's status changes to Confirmed.

---

### States and Transitions

| State | Name | Entry | Behavior |
|---|---|---|---|
| **S0** | Uninitialized | Scene load, before C5 bootstrap | C6 exists. No producers injected. `GameStatsContext` is zeroed-default. |
| **S1** | Ready | C5 bootstrap injection completes | Producers injected and available. Context is unresolved (baseline-only). Awaiting `TriggerAggregation()`. |
| **S2** | Run-Active | `TriggerAggregation()` called by C5 | Context resolved and frozen. Consumers read via `GetContext()`. Re-aggregation calls ignored with warning. |
| **S3** | Run-End | C5 calls `NotifyRunEnd()` | Context marked stale. Gameplay consumers stop reading. U4 Run Results may still read for display. |

**Transitions:**

- **S0 → S1**: C5 completes bootstrap injection.
- **S1 → S2**: C5 calls `TriggerAggregation()` at run-start.
- **S2 → S3**: C5 calls `NotifyRunEnd()` when run ends (player death or zone clear).
- **S3 → S1**: C5 begins a new run (player presses Launch from shop). Producers remain injected; context clears.
- **Any → S0**: Scene teardown / application quit. Producer list cleared.

Happy path per run: `S1 → S2 → S3 → S1 → …`

---

### Interactions with Other Systems

| System | Direction | Contract |
|---|---|---|
| **C5 Scene & Mode Flow** | C5 → C6 | C5 injects producers at bootstrap. C5 calls `TriggerAggregation()` at run-start. C5 calls `NotifyRunEnd()` at run-end. Sole lifecycle controller. C5 holds a serialized Inspector-wired reference to C6. |
| **P1a Skill Tree Architecture** | P1a → C6 (at query) | Implements `IUpgradeSource`. Injected first. Returns deltas for purchased tree nodes. Does not hold a reference to C6. |
| **G4 Mod System** | G4 → C6 (at query) | Implements `IUpgradeSource`. Injected second. Returns deltas for active mod loadout. Multipliers applied after P1a. |
| **G1, G3, G5, G6, G7** | Consumer → C6 (read-only) | Each calls `C6.GetContext()` to obtain a struct copy. G3 calls once per throw at `ArmedForThrow`. G1 calls at run-start. G5/G6/G7 call at their appropriate trigger points. None reference P1a or G4 directly. |
| **U2 Skill Tree UI / U3 Meta UI** | U → C6 (read-only pull) | U2 may call `C6.GetContext()` on user-input triggers (node panel open) to display current resolved stats. Not a frame poll. |
| **Not connected** | — | E1, E2, E3, S1, V1, A1, G2, M1, M2 have no interaction with C6. |

## Formulas

### D.1 — General Field Resolution Formula (`FIELD-RESOLVE`)

The formula that computes any single `GameStatsContext` field from its baseline value and producer-supplied deltas:

```
V_final = clamp( (V_baseline + SUM_add) × PROD_mul, V_min, V_max )

SUM_add  = Σ a_i    (all valid Additive deltas from all producers)
PROD_mul = Π m_j    (all valid Multiplicative factors, P1a first, G4 second)
```

For integer-tier fields (`base_damage`, `chain_count`): `PROD_mul` is always 1.0 (no multiplicative mode — CR-5). Clamping is the final step after all producers have been applied — never inside the aggregation loop.

**Variables:**

| Symbol | Type | Range | Description |
|---|---|---|---|
| `V_baseline` | int or float | Field-specific (Field Inventory) | Registered baseline value; never modified by producers |
| `a_i` | int or float | Unbounded (signed) | One Additive delta from one producer for this field; typed to match the field |
| `SUM_add` | int or float | Unbounded | Total additive contribution; 0 if no valid additive deltas for this field |
| `m_j` | float | (0.0, ∞) | One Multiplicative factor; 1.0 = no-op; ≤ 0.0 rejected (VAL-6) |
| `PROD_mul` | float | (0.0, ∞) | Running product of all valid multipliers; 1.0 if none supplied |
| `V_min` | int or float | Field-specific | Lower clamp. `arc_flight_time`: 0.1 s (safety floor). `base_damage`: 1. Others: from Field Inventory. |
| `V_max` | int or float | Field-specific | Upper clamp. May be ∞ where P1b ceiling is TBD (e.g., `base_damage`). |
| `V_final` | int or float | `[V_min, V_max]` | Written into `GameStatsContext`; guaranteed within range |

**Output range:** `[V_min, V_max]` per field. Always in range after the validation pass.

---

### D.2 — base_damage Integer-Tier Combat Output (`BASE-DAMAGE-TIER-STEP`)

C6 resolves `base_damage` to integer B via FIELD-RESOLVE. The `pierce_damage_falloff` formula (registered in entities.yaml, owned by G3) produces per-contact damage from B. C6's responsibility is computing B correctly; the table below documents what each B means for felt combat output, to guide P1b node design.

First-hit damage at n=0 is simply `B`. The felt tier-jump is flat +1 per tree purchase, but it compounds across the full pierce arc:

| `base_damage (B)` | Pierce arc sequence (F=0.35, n=0–4) | Total 5-hit arc damage |
|---|---|---|
| 1 | [1, 1, 1, 1, 1] | 5 |
| 3 | [3, 2, 1, 1, 1] | 8 |
| 5 | [5, 3, 2, 1, 1] | 12 |
| 8 | [8, 5, 3, 2, 1] | 19 |
| 10 | [10, 7, 5, 3, 2] | 27 |

**Variables:**

| Symbol | Type | Range | Description |
|---|---|---|---|
| `B` | int | 1–∞ (ceiling TBD in P1b) | Resolved `base_damage` from FIELD-RESOLVE; always ≥ 1 per VAL-4 |
| `n` | int | 0–∞ | Contact index within one arc; defined in `pierce_damage_falloff` (entities.yaml) |
| `F` | float | 0.0–1.0 | `pierce_falloff` from `GameStatsContext`; default 0.35 |

**Output range:** B ∈ [1, ∞). First-hit equals B exactly. → P1b must register the tree `base_damage` ceiling in entities.yaml when the node catalog is designed.

**Ceiling absence is an active degenerate path (see OQ-C6-1):** Until P1b registers a ceiling, `V_max = ∞` for `base_damage`. A P1b node catalog without a deliberate cap could produce very large B values (e.g., B = 100+), resulting in one-shot kills on all enemies regardless of positioning. This is not an implementation defect — it is an explicitly accepted design gap that P1b must close. Any sprint that includes P1b implementation work must also close OQ-C6-1 before shipping production nodes.

---

### D.3 — arc_flight_time Safety Clamp (`ARC-FLIGHT-CLAMP`)

```
arc_flight_time_final = clamp( (T_baseline + SUM_add_T) × PROD_mul_T, T_floor, T_ceiling )
```

An instantiation of FIELD-RESOLVE (D.1) with named safety constants. Called out as a separate named formula because `T_floor` is safety-critical (division-by-zero in G3 Bezier evaluation if violated) and VAL-1 requires this clamp to run before all other field validations.

**Variables:**

| Symbol | Type | Range | Description |
|---|---|---|---|
| `T_baseline` | float | — | 0.8 s; from constant `boomerang_arc_flight_time_default` (entities.yaml) |
| `SUM_add_T` | float | Unbounded (signed) | Sum of Additive arc-flight-time deltas from all producers. May be negative (speed-up nodes). |
| `PROD_mul_T` | float | (0.0, ∞) | Product of Multiplicative factors targeting `arc_flight_time` |
| `T_floor` | float | **0.1 s (constant, non-tunable)** | Hard safety floor. From constant `boomerang_min_arc_flight_time_safety` (entities.yaml). If this value must change, the G3 Bezier interpolation must be refactored first. |
| `T_ceiling` | float | 10.0 s | Soft design ceiling; prevents runaway slow-arc nodes |
| `arc_flight_time_final` | float | [0.1, 10.0] | Written into `GameStatsContext`; passed to G3 for F1/F2 Bezier evaluation |

**Output range:** Strictly [0.1, 10.0]. The 0.1 s floor cannot be reduced without a G3 engineering change.

**Worked example — aggressive speed build hitting the safety floor:**
- `T_baseline` = 0.8 s; three speed-up nodes at −0.25 s each → `SUM_add_T` = −0.75; one ×0.5 mod → `PROD_mul_T` = 0.5
- Pre-clamp: (0.8 − 0.75) × 0.5 = 0.025 s
- `arc_flight_time_final` = clamp(0.025, 0.1, 10.0) = **0.1 s** — VAL-1 fires; warning logged with producer chain.

---

### D.4 — Full Resolution Worked Example

*Three tree nodes (P1a: +1 `base_damage`, +1.0 `arc_radius`, −0.1 `throw_cooldown`) + one active mod (G4: ×0.85 `throw_cooldown`).*

| Field | Baseline | SUM_add | Post-add | PROD_mul | Pre-clamp | `V_final` |
|---|---|---|---|---|---|---|
| `base_damage` | 1 | +1 | 2 | 1.0 | 2 | **2** |
| `arc_radius` | 5.0 | +1.0 | 6.0 | 1.0 | 6.0 | **6.0 wu** |
| `throw_cooldown` | 0.8 | −0.1 | 0.7 | ×0.85 | 0.595 | **0.595 s** |
| `arc_flight_time` | 0.8 | 0 | 0.8 | 1.0 | 0.8 | **0.8 s** |
| all others | baseline | — | — | 1.0 | — | unchanged |

No validation warnings in this example. `throw_cooldown` note: the ×0.85 mod applies to the post-additive 0.7 s (not the raw baseline 0.8 s) — this is the CR-4 additive-first guarantee. What G3 receives at the first throw: `base_damage=2`, `arc_radius=6.0 wu`, `throw_cooldown=0.595 s`, `arc_flight_time=0.8 s`.

## Edge Cases

### State Machine

- **If `TriggerAggregation()` is called while C6 is in S0 (before C5 bootstrap injection completes):** Log an error identifying the caller; ignore the call; remain in S0. Silent success would mask a C5 initialization ordering bug.
- **If `TriggerAggregation()` is called a second time while C6 is in S2 (run already active):** Ignore the call; log a warning. Existing resolved context is unchanged. (CR-3)
- **If `NotifyRunEnd()` is called while C6 is in S1 (aggregation never triggered for this run):** Log a warning; transition to S3 anyway. Prevents C6 stranding if C5 reaches an error path that skipped run-start.
- **If `NotifyRunEnd()` is called while C6 is in S0:** Log a warning; ignore the call.
- **If `GetContext()` is called in S0 or S1 (before aggregation):** Return the zeroed-default `GameStatsContext` struct and log a warning with current state. Do not throw. Critical: a zeroed context has `arc_flight_time = 0.0` — G3 must guard against sub-floor context and suppress any throw.
- **If `GetContext()` is called in S3 (run ended, context stale):** Return the last resolved context and log a warning. U4 Run Results is explicitly permitted to read in S3; other consumers calling in S3 are bugs the warning surfaces.
- **If scene teardown fires while C6 is in S2 (run active):** Clear the producer list; transition to S0; emit a warning. Consumers holding a struct copy are safe — value type with no heap references (CR-9).

### Producer Contract

- **If a producer returns duplicate `(field, mode)` pairs in a single query response:** Both are applied without deduplication; log a warning naming producer identity and field. A silent double-count on P1a would double the effect of a purchased node.
- **If a producer returns a delta with a null or empty field name:** Treat as unknown field per CR-6 — log error, discard that delta; apply remaining valid deltas from that producer.
- **If a producer returns a Multiplicative delta for `base_damage` or `chain_count`:** Reject that specific delta per CR-5; log error with producer identity and field name. Any Additive deltas from the same producer for the same field are unaffected and applied normally.
- **If the injected producer list contains a null reference (unassigned Inspector slot):** Null-check each producer before calling. Log a configuration error identifying the null slot; skip it; query remaining non-null producers.
- **If the same producer object reference is injected twice (C5 bootstrap bug):** Detect duplicates by reference equality at injection time; log an error; reject the second injection. Duplicate injection silently doubles that producer's additive deltas and squares its multiplicative factors.
- **If both P1a and G4 fault during query (CR-7 applied per-producer independently):** Both are skipped; two distinct diagnostic log entries are emitted — one per producer. The run starts with baseline-only context. The two fault logs must be distinguishable from the normal first-run baseline-only case (CR-11) so diagnosis is unambiguous.

### Consumer Responsibilities

- **If G3 calls `GetContext()` before aggregation fires (S0 or S1):** Receives zeroed-default struct. G3 must not proceed to a throw if `arc_flight_time < T_floor` — treat sub-floor context as "not ready" and suppress the throw entirely.
- **If G3 holds a struct copy from S2 past the S3 → S1 transition (new run started while G3 codepath is still alive):** The copy is safe (no heap references) but represents previous-run stats. G3 must re-call `GetContext()` at each run's `TriggerAggregation()` trigger point and not persist copies across run boundaries.
- **If G1, G5, G6, or G7 poll `GetContext()` per-frame instead of caching at run-start:** Functionally correct (S2 context is frozen; each call returns an identical copy) but wasteful. Consumers must cache the struct copy at their trigger point, not poll per-frame.
- **If U2 calls `GetContext()` in S1 on the very first run (no prior resolved context exists):** Returns the zeroed-default struct. U2 must handle zeroed values and display baseline stats rather than NaN or meaningless zero values.

## Dependencies

C6 has no runtime dependencies of its own — it does not call into engine subsystems, physics, rendering, or any other game system. Its only dependencies are the systems that wire into it as lifecycle controllers, stat producers, or stat consumers.

| System | Relationship | Interface | Hard or Soft |
|---|---|---|---|
| **C5 Scene & Mode Flow** | C5 controls C6's lifecycle | C5 calls `C6.TriggerAggregation()` at run-start and `C6.NotifyRunEnd()` at run-end. C5 holds an Inspector-wired serialized reference to C6. | **Hard** — C6 cannot begin a resolved run without C5. |
| **P1a Skill Tree Architecture** | P1a is a stat producer | P1a implements `IUpgradeSource`. C5 bootstrap-injects P1a as the first producer. C6 queries P1a once per run-start. P1a does not hold a reference to C6. | **Hard** — primary stat producer for all persistent tree upgrades. |
| **G4 Mod System** | G4 is a stat producer | G4 implements `IUpgradeSource`. C5 bootstrap-injects G4 as the second producer. C6 queries G4 once per run-start after P1a. G4 does not hold a reference to C6. | **Hard** — stat producer for mod-archetype stat modifications. |
| **G1 Player Ship Controller** | G1 is a stat consumer | G1 calls `C6.GetContext()` at run-start to obtain ship-controller parameters (`ship_move_speed`, `ship_damage_resistance`, `ship_invulnerability_duration` — provisional). | **Soft** — G1 can fall back to hardcoded baselines if ship fields are zeroed. |
| **G3 Boomerang Weapon** | G3 is a stat consumer | G3 calls `C6.GetContext()` once per throw at `ArmedForThrow` to snapshot all 7 confirmed boomerang fields. G3 must not re-query C6 mid-flight. | **Hard** — G3's arc, damage, and timing are entirely driven by `GameStatsContext`. |
| **G5 Asteroid Mining** | G5 is a stat consumer | G5 calls `C6.GetContext()` for `mining_yield_multiplier` and `mining_crack_threshold_delta` (provisional; query timing TBD in G5 GDD). | **Soft** — G5 falls back to baseline mining if provisional fields are zeroed. |
| **G6 Enemy System** | G6 is a stat consumer | G6 calls `C6.GetContext()` for `enemy_stagger_duration_multiplier` (provisional). | **Soft** — G6 falls back to baseline stagger if provisional field is zeroed. |
| **G7 Wave & Spawn Director** | G7 is a stat consumer | G7 calls `C6.GetContext()` for any wave-density or spawn-rate parameters the tree may expose (fields TBD in G7 GDD). | **Soft** — no confirmed G7-specific fields yet. |

**Bidirectional consistency:** All systems listed above are confirmed in the systems index as depending on C6. Once G1, G5, G6, and G7 GDDs are authored, each must list C6 as a dependency with the consumer interface described above.

**Provisional contract:** P1a and G4 are not yet designed. The `IUpgradeSource` interface contract defined in Section C (CR-1 through CR-7) is the provisional specification. P1a's and G4's GDDs must reference this GDD as the source of the interface contract they implement.

**C5 ordering conditionality (required C5 GDD constraint):** The player fantasy ("everything the tree promised is already there before the fuel clock starts") depends on P1a's purchase-commit step completing — and being durable in P1a's delta-return state — before C5 calls `TriggerAggregation()`. This ordering constraint must be written into the C5 GDD as a formal bootstrap sequencing requirement: P1a's save-load/state-restore step is atomic with respect to C5's bootstrap sequence, and `TriggerAggregation()` is called only after all producers have fully loaded their state. If this ordering is violated, C6 will produce a valid context that silently reflects pre-purchase state, and the player will perceive the run-start moment as broken.

**Consumer access (note for systems-index):** The current systems-index entry for C6 lists its dependency as "(none at runtime)." This is a documentation inconsistency — C5 is a Hard runtime dependency (lifecycle controller). The systems-index should be updated to list C5 as a runtime dependency of C6.

## Tuning Knobs

C6's tuning knobs are the baseline values and validation range bounds for every `GameStatsContext` field — the numbers a designer adjusts during balance iteration without code changes. The aggregation rules (additive-first, multiplicative-second, injection order) are architectural constants, not tuning knobs.

### Confirmed Field Baselines and Ranges

| Knob | Current Value | Safe Range | What breaks at extremes |
|---|---|---|---|
| `base_damage` baseline | 1 (integer tier) | 1–∞ (ceiling TBD in P1b) | Below 1: VAL-4 clamps — impossible to reach. Too high as a baseline: player enters mid-power and never feels the Pillar 1 ramp from the first run. |
| `throw_cooldown` baseline | 0.8 s | 0.3–5.0 s | < 0.3 s: boomerang spam diminishes feel. > 5.0 s: fuel depletes before enough throws occur; run too short to demonstrate tree power. |
| `arc_radius` baseline | 5.0 wu | 0.5–15.0 wu | < 0.5 wu: near-straight-line arc. > 15.0 wu: arc clips play area; CR-3 bounds suppressor fires frequently; throws are suppressed. |
| `arc_flight_time` baseline | 0.8 s | 0.3–3.0 s | < 0.1 s: **VAL-1 hard floor fires** — G3 Bezier safety. > 3.0 s: boomerang hangs too long; return-timing feel degrades; P3 weight undercut by sluggishness. |
| `pierce_falloff` baseline | 0.35 | 0.0–1.0 | 0.0: no falloff — every contact deals full `base_damage` (valid extreme; pierce becomes very powerful). 1.0: only first hit deals full damage; remaining contacts deal exactly 1 (pierce feels weak). |
| `chain_count` baseline | 0 | 0–1 (MVP ceiling) | 0: chain archetype inactive until tree purchase. 1: exactly one chain fires per qualifying first-contact hit. Above 1: chain cascade mechanics — not designed for MVP; VAL-5 clamps to 1. |
| `return_detonate_radius` baseline | 0.0 wu | 0.0–8.0 wu | 0.0: detonate inactive. > 8.0 wu: AoE radius exceeds typical enemy cluster spacing; one-shots most enemies regardless of positioning; P2 mastery undermined. |

### Provisional Field Placeholders (TBD until owning GDD authored)

| Knob | Placeholder | Confirmed by |
|---|---|---|
| `ship_move_speed` baseline + range | TBD (G1) | G1 GDD |
| `ship_damage_resistance` baseline + range | TBD (G1) — range expected [0.0, 1.0] | G1 GDD |
| `ship_invulnerability_duration` baseline + range | TBD (G1) | G1 GDD |
| `mining_yield_multiplier` baseline + ceiling | Placeholder 1.0; ceiling TBD | G5 GDD + P1b |
| `mining_crack_threshold_delta` baseline + range | Placeholder 0; lower bound TBD (G5 crack baseline) | G5 GDD |
| `enemy_stagger_duration_multiplier` baseline + ceiling | Placeholder 1.0; ceiling TBD | G6 GDD |

### Architectural Constants (Not Tunable Without Code Change)

| Constant | Value | Why |
|---|---|---|
| `arc_flight_time` hard floor (`T_floor`) | 0.1 s | Safety-critical — G3 Bezier divides by `arc_flight_time`; changing this requires a G3 code change. Sourced from `boomerang_min_arc_flight_time_safety` (entities.yaml). |
| `chain_count` MVP ceiling | 1 | Chain cascade above 1 is undesigned at MVP. Raise post-MVP only after OQ-CHAIN-SCALING (G3 GDD) is resolved. |
| Producer injection order | P1a first, G4 second | Determines multiplicative application order across all float fields. Changing this alters results for any field where both producers supply multipliers. Architecture-level decision. |

## Visual/Audio Requirements

[To be designed]

## UI Requirements

[To be designed]

## Acceptance Criteria

All criteria are automatable as Unity Test Framework EditMode unit tests. Target file: `tests/unit/c6-stat-resolver/c6_stat_resolver_tests.cs`

- **AC-1 (CR-1 — Pull model):** GIVEN two mock producers injected via `Bootstrap()`, WHEN `TriggerAggregation()` fires, THEN each mock's `GetDeltas()` call-count is exactly 1 (verified via mock call-count assertion). *Structural note:* the guarantee that producers never call back into C6 is enforced by the `IUpgradeSource` interface definition (no C6 parameter or return type is present) — this is a compile-time structural guarantee, not a runtime assertion.

- **AC-2a (CR-2 — No registration API):** GIVEN C6's public API, WHEN inspected via `typeof(C6StatResolver).GetMethod("Register")` and related reflection calls, THEN no method named `Register`, `AddProducer`, or `Deregister` is present (one assertion per name, explicit failure message per name).
- **AC-2b (CR-2 — Bootstrap injection point):** GIVEN `Bootstrap(IUpgradeSource[] producers)` is called with a known producer, WHEN `TriggerAggregation()` fires subsequently, THEN the context reflects that producer's deltas — confirming Bootstrap is the sole injection mechanism.

- **AC-3 (CR-3 — Single aggregation guard):** GIVEN C6 has completed one successful `TriggerAggregation()` call (S2 state confirmed), WHEN `TriggerAggregation()` is called a second time, THEN: (a) the warning log count increases by exactly 1 compared to the count recorded before the second call; (b) the mock producer `GetDeltas()` call-count does not increase from its value at the time of the second call; (c) `GetContext()` returns a struct with values byte-identical to those returned immediately before the second call.

- **AC-4 (CR-4 — Additive-first sequence):** GIVEN P1a supplies `+0.5 throw_cooldown` (Additive) and G4 supplies `×0.85 throw_cooldown` (Multiplicative), baseline 0.8 s, WHEN `TriggerAggregation()` fires, THEN resolved `throw_cooldown` = 1.105 s (`(0.8 + 0.5) × 0.85`), not 1.18 s (`(0.8 × 0.85) + 0.5`).

- **AC-5 (CR-5 — Integer-tier additive-only):** GIVEN a producer supplies a Multiplicative delta for `base_damage` AND also supplies `+2.0 arc_radius` (Additive) in the same delta list, WHEN `TriggerAggregation()` fires, THEN: the multiplier is discarded; an error log names the producer and field; `arc_radius` = 7.0 wu (baseline 5.0 + 2.0), confirming the invalid delta did not suppress the producer's valid contributions.

- **AC-6 (CR-6 — Unknown field rejection):** GIVEN a producer supplies a delta for `"invented_field"` AND also supplies `+1 base_damage` (Additive, valid) in the same delta list, WHEN `TriggerAggregation()` fires, THEN: the unknown delta is discarded with an error log containing producer identity and field name; `base_damage` = 2 (baseline 1 + 1), confirming rejection of the unknown field did not suppress the valid delta.

- **AC-7a (CR-7 — Both producers fault):** GIVEN both P1a and G4 throw exceptions during `GetDeltas()`, WHEN `TriggerAggregation()` fires, THEN: two distinct error logs are emitted (each identifying the faulting producer by name); `GetContext()` returns all 7 confirmed fields at their registered baseline values (identical to the AC-9 result). The two fault logs must be distinguishable from the normal first-run baseline-only case (CR-12) — e.g., by log category or message prefix. *(Sub-condition (b) from the original AC-7 is a duplicate of AC-11 and has been removed.)*
- **AC-7c (VAL-4 — base_damage clamped to 1):** GIVEN a producer supplies a total additive delta of −1 on `base_damage` (driving result to 0), WHEN `TriggerAggregation()` fires, THEN `base_damage` = 1 (VAL-4 floor clamp) and exactly one clamp warning log is emitted.
- **AC-7d (VAL-5 — chain_count MVP ceiling):** GIVEN a producer supplies a total additive delta of +3 on `chain_count` (driving result to 3), WHEN `TriggerAggregation()` fires, THEN `chain_count` = 1 (VAL-5 MVP ceiling clamp) and exactly one clamp warning log is emitted (not error — ceiling rises post-MVP).
- **AC-7e (VAL-6 — Negative multiplier rejected):** GIVEN a producer supplies a Multiplicative factor of −0.5 on `arc_radius`, WHEN `TriggerAggregation()` fires, THEN the multiplier is rejected (VAL-6), exactly one error log is emitted, and `arc_radius` = 5.0 wu (baseline, unaffected).

- **AC-8 (CR-11 — Run-duration immutability):** GIVEN C6 is in S2 with `base_damage = 2` (baseline 1 + P1a additive delta of +1), WHEN P1a's internal state is mutated to append an additional +5 to its `base_damage` delta (new cumulative delta: +6), AND `GetContext()` is called immediately after the mutation, THEN `GetContext().base_damage` = 2 (unchanged for this run). WHEN `NotifyRunEnd()` is then called followed by a new `TriggerAggregation()`, THEN `GetContext().base_damage` = 7 (baseline 1 + cumulative delta 6). *Assumption: P1a's delta list is cumulative (append) not replace — the test mock must implement this explicitly.*

- **AC-9 (CR-12 — First-run baseline-only):** GIVEN both producers return empty delta lists, WHEN `TriggerAggregation()` fires, THEN all 7 confirmed fields equal their baselines (`base_damage=1`, `throw_cooldown=0.8`, `arc_radius=5.0`, `arc_flight_time=0.8`, `pierce_falloff=0.35`, `chain_count=0`, `return_detonate_radius=0.0`) with no error or warning logs emitted. *Log assertion implementation:* use `LogAssert.NoUnexpectedReceived()` called after `TriggerAggregation()`. Do not call `LogAssert.Expect()` before the call — any warning or error output from C6 constitutes a failure.*

- **AC-10 (D.4 — FIELD-RESOLVE worked example):** GIVEN P1a supplies `+1 base_damage`, `+1.0 arc_radius`, `−0.1 throw_cooldown` (Additive) and G4 supplies `×0.85 throw_cooldown` (Multiplicative), WHEN `TriggerAggregation()` fires, THEN `GetContext()` returns exactly: `base_damage=2`, `arc_radius=6.0 wu` (±0.0001), `throw_cooldown=0.595 s` (±0.0001), `arc_flight_time=0.8 s` (±0.0001), `pierce_falloff=0.35` (±0.0001), `chain_count=0`, `return_detonate_radius=0.0 wu` (±0.0001). All 7 confirmed fields must be asserted explicitly — no catch-all "other fields at baseline."

- **AC-11 (D.3 — ARC-FLIGHT-CLAMP hard floor):** GIVEN P1a supplies three additive deltas of `−0.25 s` on `arc_flight_time` (cumulative: −0.75 s) and G4 supplies one `×0.5` Multiplicative factor on `arc_flight_time`, with no other deltas from either producer on any field, WHEN `TriggerAggregation()` fires, THEN: `arc_flight_time` = 0.1 s (VAL-1 clamp; pre-clamp was 0.025 s); exactly one VAL-1 warning log is emitted identifying both P1a and G4 as contributing producers; all 6 remaining confirmed fields (`base_damage=1`, `arc_radius=5.0`, `throw_cooldown=0.8`, `pierce_falloff=0.35`, `chain_count=0`, `return_detonate_radius=0.0`) equal their baseline values. All 7 confirmed fields must be asserted explicitly.

- **AC-12a (Edge case — zero-context guard, S0):** GIVEN C6 is in S0 (no producers injected, no `Bootstrap()` call), WHEN `GetContext()` is called, THEN: a zeroed-default struct is returned with all fields at C# default values — specifically `arc_flight_time = 0.0f`, `base_damage = 0`, etc.; exactly one warning log is emitted per call identifying state S0; no exception is thrown. *Note:* `arc_flight_time = 0.0f` here is intentional and correct — the validation pass (VAL-1) does not run during `GetContext()`, only during `TriggerAggregation()`. G3 is responsible for suppressing throws when the context is pre-aggregation. This is NOT a VAL-1 violation.
- **AC-12b (Edge case — zero-context guard, S1):** GIVEN C6 is in S1 (`Bootstrap()` has been called but `TriggerAggregation()` has not), WHEN `GetContext()` is called, THEN: a zeroed-default struct is returned (same as AC-12a); exactly one warning log is emitted per call identifying state S1; no exception is thrown. *The S1 warning message must be distinguishable from the S0 warning — include the state name in the log.*
- **AC-13 (Edge case — TriggerAggregation in S0):** GIVEN C6 is in S0 (before `Bootstrap()` has been called), WHEN `TriggerAggregation()` is called, THEN: C6 remains in S0; exactly one error log is emitted identifying the caller (or indicating no producers are registered); `GetContext()` still returns a zeroed-default struct. C6 must NOT silently succeed and return a baseline context — that would mask a C5 initialization ordering bug.
- **AC-14 (Edge case — duplicate delta pair from same producer):** GIVEN a producer returns two deltas for the same `(FieldKey, Mode)` pair (e.g., two Additive `base_damage` deltas of `+1` each), WHEN `TriggerAggregation()` fires, THEN: both deltas are applied (result: `base_damage = 3` — baseline 1 + 1 + 1); exactly one warning log is emitted naming the producer and the duplicated `(FieldKey, Mode)` pair. Deduplication is explicitly NOT performed.
- **AC-15 (Edge case — null producer in Bootstrap list):** GIVEN `Bootstrap()` is called with an array containing one valid producer and one `null` entry, WHEN `TriggerAggregation()` fires, THEN: the null slot is skipped; exactly one configuration error log is emitted identifying the null slot position; the valid producer's deltas are applied normally (result reflects the valid producer's contributions).

## Open Questions

- **OQ-C6-1** — What is the design ceiling for `base_damage`? P1b must specify the maximum integer tier reachable through tree purchases and register it as a constant in entities.yaml. Until then, `base_damage` has no upper clamp. *Owner: P1b GDD authoring.*

- **OQ-C6-2** — Does any tree node or mod use multiplicative mode on float fields at MVP? If the answer from P1b and G4 is "no multiplicative deltas exist," Mode B in the aggregation rules is implemented but never exercised. This is acceptable — Mode B stays in the spec for future-proofing. *Owner: G4 GDD authoring.*

- **OQ-C6-3** — Does E1 (Fuel Economy) need any fields in `GameStatsContext`? If a tree node gives "5% more fuel per kill," that modifier needs a home. Flag for E1 GDD authoring. *Owner: E1 GDD authoring.*

- **OQ-C6-4** — The three Provisional-G1 fields (`ship_move_speed`, `ship_damage_resistance`, `ship_invulnerability_duration`) have no baseline values yet. G1's GDD must confirm baselines so C6 can initialise defaults correctly. *Owner: G1 GDD authoring.*

- **OQ-C6-5** — If C6 is in S0 when C5 triggers aggregation (fresh first install, no producers registered yet), the result is baseline-only context (CR-11). C5's run-start flow must not treat an unregistered C6 as an error. *Owner: C5 GDD authoring.*

- **OQ-C6-7** — ~~Exact C5 bootstrap injection method name: constructor parameter vs. a dedicated `C5BootstrapInject(IUpgradeSource[])` method.~~ **RESOLVED 2026-04-24**: Injection method is `Bootstrap(IUpgradeSource[] producers)` — a dedicated public method on C6 called once by C5 during scene-load bootstrap. Constructor injection is not viable for Unity MonoBehaviours. AC-2 is now unblocked.
