---
date: 2026-04-25T00:00:00-07:00
researcher: Claude (claude-sonnet-4-6)
git_commit: dfd4ae1d0105e238466410299200fe5a012b3b7d
branch: main
repository: gw1108/IncrementalAsteroidsBoomerang
topic: "INC-29 — UpgradeNode MonoBehaviour: stat deltas, prerequisites, metadata"
tags: [research, codebase, p1a, skill-tree, upgrade-node, c6, stat-resolver, upgrade-source]
status: complete
last_updated: 2026-04-25
last_updated_by: Claude (claude-sonnet-4-6)
---

# Research: INC-29 — UpgradeNode MonoBehaviour

**Date**: 2026-04-25
**Researcher**: Claude (claude-sonnet-4-6)
**Git Commit**: dfd4ae1d0105e238466410299200fe5a012b3b7d
**Branch**: main
**Repository**: gw1108/IncrementalAsteroidsBoomerang

## Research Question

> What infrastructure exists to implement an `UpgradeNode` MonoBehaviour with serialized stat deltas, prerequisite node references, and display metadata (name, description)?

This document is purely descriptive — it maps what exists in the codebase that `UpgradeNode` will build on, and the technical constraints the implementation must satisfy.

---

## Summary

The entire C6 stat-aggregation layer is already implemented. `UpgradeNode` does not yet exist anywhere in the codebase. The component will extend the existing `UpgradeSource` abstract MonoBehaviour (`_Scripts/Gameplay/Stats/UpgradeSource.cs`) and must satisfy one critical serialization constraint: `StatDelta` is a `readonly struct` whose fields cannot be directly serialized by Unity — a `[Serializable]` wrapper class will be needed to expose stat deltas in the Inspector. No P1a GDD exists; the authoritative description of P1a scope is `design/gdd/systems-index.md:76-84`.

---

## Detailed Findings

### 1. UpgradeSource — The Direct Base Class

`_Scripts/Gameplay/Stats/UpgradeSource.cs`

```csharp
public abstract class UpgradeSource : MonoBehaviour
{
    public abstract StatDelta[] GetDeltas();
}
```

- `UpgradeNode` must extend `UpgradeSource` and implement `GetDeltas()`.
- `UpgradeSource` is wired into `C6StatResolver._producers` as a `[SerializeField] private UpgradeSource[] _producers` array.
- The doc comment on `UpgradeSource` names intended implementors: "P1a Skill Tree, G4 Mod System."
- The only other concrete implementation is `TestUpgradeSource` (editor-only stub, wrapped in `#if UNITY_EDITOR`).

### 2. StatDelta — The Unit of Stat Change

`_Scripts/Gameplay/Stats/StatDelta.cs`

```csharp
public readonly struct StatDelta
{
    public readonly string         FieldKey;
    public readonly DeltaMode      Mode;
    public readonly DeltaValueType Type;
    public readonly int            IntValue;
    public readonly float          FloatValue;

    public StatDelta(string fieldKey, DeltaMode mode, int value) { ... }
    public StatDelta(string fieldKey, DeltaMode mode, float value) { ... }
}
```

**Critical constraint**: `StatDelta` is a `readonly struct` with `readonly` fields. Unity's serialization system requires writable fields (`[SerializeField]` cannot serialize `readonly` fields). A `[Serializable]` helper class (e.g., `StatDeltaData`) that mirrors these fields without `readonly` will be needed for inspector editing. `GetDeltas()` would then convert them.

### 3. DeltaMode — Current Enum Values

`_Scripts/Gameplay/Stats/DeltaEnums.cs` (line 1):

```csharp
public enum DeltaMode      { Additive }
public enum DeltaValueType { Int, Float }
```

`DeltaMode` currently has **only `Additive`**. The `C6StatResolver.Aggregate()` method has the multiplicative branch coded (`else mulProds[key] *= value`) but it is unreachable with the current enum. UpgradeNode stat deltas will be Additive-only until a `Multiplicative` value is added.

### 4. StatKeys — Available Fields

`_Scripts/Gameplay/Stats/StatKeys.cs`:

```csharp
// Boomerang stats
BaseDamage           = "base_damage"
ThrowCooldown        = "throw_cooldown"
ArcRadius            = "arc_radius"
ArcFlightTime        = "arc_flight_time"
PierceFalloff        = "pierce_falloff"
ChainCount           = "chain_count"

// E1 Fuel Economy stats
FuelStartingAmount              = "fuel_starting_amount"
FuelDecayRate                   = "fuel_decay_rate"
FuelPerKill                     = "fuel_per_kill"
FuelDiminishingReturnsFactor    = "fuel_diminishing_returns_factor"
FuelMinExtension                = "fuel_min_extension"
```

All 11 keys are present in both `StatKeys` and `C6StatResolver.Specs`. Inspector-facing stat delta entries would reference these string values (either raw strings or via a dropdown enum).

### 5. C6StatResolver — How Producers Are Consumed

`_Scripts/Gameplay/Stats/C6StatResolver.cs:41`:

```csharp
[SerializeField] private UpgradeSource[] _producers;
```

Producers are wired in the inspector. When `TriggerAggregation()` is called (by `RunManager.StartRun()`), the resolver iterates `_producers`, calls `GetDeltas()` on each, and applies the additive-first formula. **Purchased UpgradeNodes must be added to this array** — the mechanism for doing so (purchase transaction logic) is a sibling task in INC-10 (P1a parent ticket), not part of INC-29.

### 6. Existing Code Conventions

From `RunManager.cs` and `FuelManager.cs`:

| Convention | Example |
|---|---|
| Serialized private fields | `[SerializeField] private UpgradeSource[] _producers` |
| Naming | `_camelCase` underscore prefix for private fields |
| Header grouping | `[Header("...")]` (used in PlayerController) |
| Null guard pattern | `Debug.LogError(...)` + early return in `Start()` |
| Non-optional refs | Log error if unset, don't silently continue (per CLAUDE.md) |

### 7. P1a Scope (No P1a GDD Exists)

The only authoritative scope definition is `design/gdd/systems-index.md:76-84`:

> **Coding:** Implements a way to upgrade the player's stats through C6. Nodes are MonoBehaviours. This system owns purchase transaction logic but not the visual grid (U2) or node designs (P1b).

INC-29 covers the data shape only — stat deltas, prerequisites, display metadata. Purchase transaction logic is a separate sub-task (INC-10 scope).

P1b (`systems-index.md:96-102`) will "define 25–30 individual nodes, each specifying stat deltas, unlock prerequisites, and display metadata" — this is human-only and creates UpgradeNode instances, not the MonoBehaviour class itself.

### 8. Prerequisite Node References

No prerequisite infrastructure exists anywhere. The INC-29 description says "a list of prerequisite node references." This will be a `[SerializeField] private UpgradeNode[] _prerequisites` array — a self-referential list wired in the inspector. Unity handles self-referential MonoBehaviour arrays correctly via object references.

### 9. No SkillTree Folder or Manager Exists

There is no `_Scripts/Gameplay/SkillTree/`, `_Scripts/Progression/`, or any containing MonoBehaviour for the skill tree graph. `UpgradeNode` will be a standalone MonoBehaviour placed as a prefab or scene object — the containing architecture is not yet defined.

---

## Code References

- `_Scripts/Gameplay/Stats/UpgradeSource.cs:1-7` — Abstract base class UpgradeNode extends
- `_Scripts/Gameplay/Stats/StatDelta.cs:1-26` — Readonly struct (serialization constraint)
- `_Scripts/Gameplay/Stats/DeltaEnums.cs:1-2` — DeltaMode (Additive only currently)
- `_Scripts/Gameplay/Stats/StatKeys.cs:1-16` — All available stat field keys
- `_Scripts/Gameplay/Stats/GameStatsContext.cs:1-45` — 11 confirmed stat fields
- `_Scripts/Gameplay/Stats/C6StatResolver.cs:41` — `[SerializeField] private UpgradeSource[] _producers`
- `_Scripts/Gameplay/Stats/TestUpgradeSource.cs:1-13` — Editor stub example pattern
- `_Scripts/Gameplay/RunManager.cs:10-11` — `[SerializeField] private _camelCase` convention
- `design/gdd/systems-index.md:76-84` — P1a scope definition
- `design/gdd/systems-index.md:96-102` — P1b node catalog (human-only, creates instances)
- `thoughts/shared/plans/2026-04-24-c6-stat-resolver.md` — C6 plan with StatDelta/UpgradeSource shapes

---

## Architecture Documentation

`UpgradeNode` sits at the intersection of three concerns:
1. **C6 aggregation** — extends `UpgradeSource`, implements `GetDeltas()` to contribute stat deltas
2. **Prerequisite graph** — self-references other `UpgradeNode`s to enforce unlock order
3. **Display** — exposes human-readable name and description for P1b catalog and U2 UI

The C6 contract (`UpgradeSource.GetDeltas()`) is the only externally consumed interface INC-29 must satisfy. The prerequisite list and display metadata are consumed by future systems (purchase logic in INC-10, UI in U2).

---

## Historical Context (from thoughts/)

- `thoughts/shared/plans/2026-04-24-c6-stat-resolver.md` — Original C6 implementation plan. Describes `UpgradeSource` abstract class and `StatDelta` struct shapes that are now implemented as-is. Notes "TestUpgradeSource" as the only intended concrete example for Phase 2.
- `thoughts/shared/research/2026-04-24-e1-fuel-economy-codebase-map.md:74` — Notes that `DeltaMode` has only `Additive` and the multiplicative branch in `C6StatResolver.Aggregate()` is unreachable with the current enum.

---

## Open Questions

| Question | Impact |
|---|---|
| Should `FieldKey` in the inspector be a free-entry string or a dropdown from `StatKeys`? | A dropdown reduces typo risk; free string allows future fields not yet in `StatKeys`. |
| Will `DeltaMode.Multiplicative` be added before or after INC-29 implementation? | Determines whether UpgradeNode's serializable delta wrapper needs a Mode field beyond Additive. |
| Where in the scene hierarchy do UpgradeNode GameObjects live? | No SkillTree manager or container exists yet — needs to be decided before prefab/scene setup. |
| How does the purchase transaction add a purchased UpgradeNode to `C6StatResolver._producers`? | This is INC-10 scope, but the UpgradeNode's public API should anticipate it. |
