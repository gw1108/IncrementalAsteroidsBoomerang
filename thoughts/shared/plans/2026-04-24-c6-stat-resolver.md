# C6 Stat Resolver — Implementation Plan

> **Date**: 2026-04-24
> **Source GDD**: `design/gdd/c6-stat-resolver_V2.md`

---

## Overview

Implement the C6 Stat Resolver: the stat-aggregation layer that sits between progression systems (P1a Skill Tree, G4 Mod System) and gameplay systems (G3 Boomerang, G1 Ship, etc.). At run-start, C6 queries registered producers, resolves deltas into a frozen `GameStatsContext`, and holds it for the duration of the run.

---

## Current State

The `_Scripts` folder has only `Player/PlayerController.cs` and `Utility/` helpers. No C6, C5, G3, or any gameplay systems exist yet. This is entirely greenfield.

---

## Desired End State

A fully implemented and Unity-console-clean `C6StatResolver` MonoBehaviour that:
- Holds producers as `[SerializeField] UpgradeSource[]` wired in the inspector
- Resolves stats via `TriggerAggregation()` using additive-first, then multiplicative formula
- Enforces all six validation rules (VAL-1 through VAL-6)
- Maintains the S1→S2→S3 state machine
- Returns a frozen `GameStatsContext` to consumers via `GetContext()`

**Verification:** Zero errors or warnings in the Unity console during normal flow; all edge-case paths log warnings/errors as specified.

---

## What We Are NOT Doing

- Not implementing P1a, G4, C5, G3, G1, G5, G6 — only the C6 layer and its contracts
- Not implementing provisional fields in GameStatsContext (G1/G5/G6 fields are TBD until their GDDs confirm them)
- Not implementing `PreviewContext()` (OQ-C6-6 — deferred to P1a/U2 GDD)
- Not creating a scene or prefab wiring (that's C5's job)

---

## Implementation Approach

Two phases: pure-C# data types first (no Unity dependencies), then the MonoBehaviour resolver + abstract base class + a stub producer for manual editor testing.

---

## Phase 1: Core Types

**Goal:** All data types exist and compile.

**Directory:** `_Scripts/Gameplay/Stats/`

### Files to Create

#### 1. `DeltaEnums.cs`
Two enums in one file.

```csharp
public enum DeltaMode      { Additive, Multiplicative }
public enum DeltaValueType { Int, Float }
```

#### 2. `StatKeys.cs`
Confirmed fields only — no provisional fields until their GDDs land.

```csharp
public static class StatKeys
{
    public const string BaseDamage           = "base_damage";
    public const string ThrowCooldown        = "throw_cooldown";
    public const string ArcRadius            = "arc_radius";
    public const string ArcFlightTime        = "arc_flight_time";
    public const string PierceFalloff        = "pierce_falloff";
    public const string ChainCount           = "chain_count";
    public const string ReturnDetonateRadius = "return_detonate_radius";
}
```

#### 3. `StatDelta.cs`

```csharp
public readonly struct StatDelta
{
    public readonly string         FieldKey;
    public readonly DeltaMode      Mode;
    public readonly DeltaValueType Type;
    public readonly int            IntValue;
    public readonly float          FloatValue;

    public StatDelta(string fieldKey, DeltaMode mode, int value)
    {
        FieldKey = fieldKey; Mode = mode;
        Type = DeltaValueType.Int; IntValue = value; FloatValue = 0f;
    }

    public StatDelta(string fieldKey, DeltaMode mode, float value)
    {
        FieldKey = fieldKey; Mode = mode;
        Type = DeltaValueType.Float; FloatValue = value; IntValue = 0;
    }
}
```

#### 4. `GameStatsContext.cs`
Readonly struct — confirmed fields only.

```csharp
public readonly struct GameStatsContext
{
    public readonly int   BaseDamage;           // baseline 1
    public readonly float ThrowCooldown;        // baseline 0.8 s
    public readonly float ArcRadius;            // baseline 5.0 wu
    public readonly float ArcFlightTime;        // baseline 0.8 s; hard floor 0.1 s
    public readonly float PierceFalloff;        // baseline 0.35
    public readonly int   ChainCount;           // baseline 0; MVP max 1
    public readonly float ReturnDetonateRadius; // baseline 0.0 wu

    public GameStatsContext(
        int baseDamage, float throwCooldown, float arcRadius,
        float arcFlightTime, float pierceFalloff, int chainCount,
        float returnDetonateRadius)
    {
        BaseDamage = baseDamage; ThrowCooldown = throwCooldown;
        ArcRadius = arcRadius; ArcFlightTime = arcFlightTime;
        PierceFalloff = pierceFalloff; ChainCount = chainCount;
        ReturnDetonateRadius = returnDetonateRadius;
    }
}
```

### Success Criteria — Phase 1

#### Automated:
- [ ] Project compiles with zero errors or warnings (check Unity console after import)

---

## Phase 2: UpgradeSource Base Class, C6StatResolver, and Test Stub

**Goal:** Full aggregation logic, state machine, validation rules, and a wirable test producer.

### `UpgradeSource.cs` — Abstract Base Class

Producers (P1a, G4) extend this instead of implementing an interface. C6 holds a `[SerializeField]` array of these, wired in the inspector. Injection order is set by array order.

```csharp
public abstract class UpgradeSource : MonoBehaviour
{
    public abstract StatDelta[] GetDeltas();
}
```

### `C6StatResolver.cs`

#### State Machine

States: **Ready (S1) → RunActive (S2) → RunEnd (S3)**. C6 starts in S1 after `Awake`. S0 (Uninitialized) is removed — producers are wired at scene load via `[SerializeField]`, so there is no separate bootstrap step.

```csharp
private enum ResolverState { Ready, RunActive, RunEnd }
```

#### Baseline Registry

Private `FieldSpec` dictionary populated in `Awake`. Not configurable at runtime.

```csharp
private readonly struct FieldSpec
{
    public readonly float Baseline;
    public readonly float Min;
    public readonly float Max;
    public readonly bool  IntegerTier; // true for base_damage, chain_count — additive-only
}
```

#### Public API

```csharp
[SerializeField] private UpgradeSource[] _producers; // ordered: P1a first, G4 second

public void  TriggerAggregation()   // S1 → S2
public void  NotifyRunEnd()         // S2 → S3
public void  Reset()                // S3 → S1 (Ready); clears context, producers unchanged
public GameStatsContext GetContext()
```

**`Reset()` contract:** Valid only in S3. Transitions to S1 (Ready) and clears the resolved context. Producers remain wired — no re-injection needed. C5 call sequence per run: `TriggerAggregation()` → (run plays) → `NotifyRunEnd()` → (end-of-run UI) → `Reset()` → `TriggerAggregation()`.

If `Reset()` is called outside S3, log an error and no-op.

#### Aggregation Logic

Formula: `V_final = clamp( (V_baseline + SUM_add) × PROD_mul, V_min, V_max )`

1. For each producer in array order: call `GetDeltas()`, catch any exception → log error, skip that producer's deltas entirely, continue.
2. Reject invalid deltas per validation rules below.
3. Per field: sum all Additive deltas → compound all Multiplicative factors in array order (integer-tier fields: skip all multipliers, `PROD_mul = 1.0`).
4. Apply VAL-1 clamp first (`arc_flight_time` floor 0.1 s), then general clamps.
5. Construct and store `GameStatsContext`.

#### Validation Rules

| Rule | Condition | Action |
|---|---|---|
| VAL-1 | `arc_flight_time` result < 0.1 s | Clamp to 0.1 s, log warning. Runs before all other clamps. |
| VAL-2 | Any float field outside [min, max] | Clamp, log warning. |
| VAL-3 | Non-integer delta on `base_damage` or `chain_count` | Reject delta, log error, continue with other deltas. |
| VAL-4 | `base_damage` result < 1 | Clamp to 1. |
| VAL-5 | `chain_count` result > 1 | Clamp to 1, log warning. |
| VAL-6 | Multiplicative factor ≤ 0.0 | Reject multiplier, log error. |

#### Edge-Case Behaviors

| Situation | Behavior |
|---|---|
| `TriggerAggregation()` in S2 | Warning logged, existing context unchanged. |
| `TriggerAggregation()` in S3 | Error logged, stay S3. |
| `NotifyRunEnd()` in S1 | Warning logged, transition to S3 anyway. |
| `Reset()` outside S3 | Error logged, no-op. |
| `GetContext()` in S1 | Warning logged, return zeroed default. |
| `GetContext()` in S3 | Warning logged, return last resolved context. |
| Null entry in `_producers` array | Skip it, log config error. |
| Producer throws during `GetDeltas()` | Log error, skip that producer's deltas, continue. |
| Multiplicative delta on integer-tier field | Reject + log error; other valid deltas from same producer still apply. |
| Duplicate `(field, mode)` from one producer | Both applied, warning logged. |
| Scene teardown | `OnDestroy`: transition to S1 (Ready), context cleared. |

### `TestUpgradeSource.cs` — Editor Stub

Extends `UpgradeSource`. Returns hardcoded deltas matching the GDD worked example.

```csharp
#if UNITY_EDITOR
public class TestUpgradeSource : UpgradeSource
{
    public override StatDelta[] GetDeltas() => new[]
    {
        new StatDelta(StatKeys.BaseDamage,    DeltaMode.Additive, 1),
        new StatDelta(StatKeys.ArcRadius,     DeltaMode.Additive, 1.0f),
        new StatDelta(StatKeys.ThrowCooldown, DeltaMode.Additive, -0.1f),
    };
}
#endif
```

Add `[ContextMenu("Print Resolved Context")]` on `C6StatResolver` (also `#if UNITY_EDITOR`) that calls `TriggerAggregation()` then `Debug.Log`s all fields — for one-click verification in Play mode.

### Success Criteria — Phase 2

#### Automated:
- [ ] Project compiles zero errors or warnings.

#### Manual:
- [ ] Add `C6StatResolver` + `TestUpgradeSource` to a test scene GameObject; wire `TestUpgradeSource` into `_producers[0]`.
- [ ] Enter Play mode, right-click C6StatResolver → "Print Resolved Context".
- [ ] Console output matches the GDD worked example:

| Field | Expected |
|---|---|
| `BaseDamage` | 2 |
| `ArcRadius` | 6.0 |
| `ThrowCooldown` | 0.595 |
| `ArcFlightTime` | 0.8 |

- [ ] A second "Print Resolved Context" call logs a warning and does not change values.
- [ ] Remove `TestUpgradeSource` from the array; verify a config error is logged and context resolves to baseline.

---

## File Map

```
_Scripts/
  Gameplay/
    Stats/
      DeltaEnums.cs
      StatKeys.cs
      StatDelta.cs
      GameStatsContext.cs
      UpgradeSource.cs
      C6StatResolver.cs
      TestUpgradeSource.cs   (#if UNITY_EDITOR — remove slot from scene before ship)
```

---

## References

- GDD: `design/gdd/c6-stat-resolver_V2.md`
- Wiring pattern reference: `_Scripts/Player/PlayerController.cs` (`[SerializeField]` for values)
