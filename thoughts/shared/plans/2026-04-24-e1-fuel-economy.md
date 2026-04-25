# E1 — Fuel Economy: Implementation Plan

## Overview

Implements a continuously-decaying fuel value that ends the run on expiry. All five tuning parameters live in `C6StatResolver` / `GameStatsContext` so the upgrade system has a single authoritative source for every player-facing stat. The run-end flow is stubbed with `Debug.LogWarning` until C5 is built. HUD output is stubbed with comments pending U1.

## Current State

- C6 stat pipeline (`StatKeys`, `Specs`, `GameStatsContext`, `BuildContext`) is fully functional with 6 fields.
- No fuel fields in C6.
- No `FuelManager` in the codebase.
- `BoomerangTarget.TakeDamage` deactivates the GameObject but calls nothing else.
- No run-end flow (C5 not built). No HUD (U1 not built — GitHub issue pending `gh auth login`).

## Desired End State

- `FuelManager` singleton ticks fuel down each frame by `context.FuelDecayRate`.
- At ≤ 3 seconds remaining, fires low-fuel stub (TODO A1).
- At ≤ 0, logs `[FuelManager] Run ended: fuel depleted.` (TODO C5).
- Any `BoomerangTarget` death directly calls `FuelManager.Instance.OnTargetKilled()`.
- Kill extension formula: `FuelPerKill * DiminishingReturnsFactor^killCount`, floored at `FuelMinExtension` — but only applied when `FuelPerKill > 0`. At baseline (FuelPerKill = 0) kills give exactly 0s; `FuelMinExtension` becomes the floor only once a skill node raises `FuelPerKill`.
- All five E1 parameters resolve through C6 — no inspector tuning on `FuelManager`.
- Unity Console shows no errors on Play.

## What We're NOT Doing

- Real run-end scene reload (C5 stub only).
- HUD fuel bar (U1 stub only).
- Audio (A1 stub only).
- Mining fuel extensions (G5 not built; `FuelPerMine` deferred to G5 plan).
- Enemy vs. asteroid kill distinction (all `BoomerangTarget` deaths treated equally for now).

---

## Phase 1 — Extend C6 for Fuel Stats

### `StatKeys.cs` — add five constants

```csharp
public const string FuelStartingAmount           = "fuel_starting_amount";
public const string FuelDecayRate                = "fuel_decay_rate";
public const string FuelPerKill                  = "fuel_per_kill";
public const string FuelDiminishingReturnsFactor = "fuel_diminishing_returns_factor";
public const string FuelMinExtension             = "fuel_min_extension";
```

### `C6StatResolver.cs` — Specs table

Add a note above the `Specs` dictionary, then add five entries:

```csharp
// All player-facing and upgradeable stats are registered here so P1a/P1b have a single
// authoritative extension point. Never put tuning values in MonoBehaviour inspector fields
// if they are meant to be upgrade-influenced.
private static readonly Dictionary<string, FieldSpec> Specs = new Dictionary<string, FieldSpec>
{
    // existing entries ...
    { StatKeys.FuelStartingAmount,           new FieldSpec(20f,  5f,    120f,  false) },
    { StatKeys.FuelDecayRate,                new FieldSpec(1.0f, 0.1f,  5.0f,  false) },
    { StatKeys.FuelPerKill,                  new FieldSpec(0.0f, 0.0f,  30.0f, false) },
    { StatKeys.FuelDiminishingReturnsFactor, new FieldSpec(0.8f, 0.0f,  1.0f,  false) },
    { StatKeys.FuelMinExtension,             new FieldSpec(0.1f, 0.0f,  10.0f, false) },
};
```

### `GameStatsContext.cs` — five new fields + constructor params

```csharp
public readonly float FuelStartingAmount;
public readonly float FuelDecayRate;
public readonly float FuelPerKill;
public readonly float FuelDiminishingReturnsFactor;
public readonly float FuelMinExtension;

// extend constructor with five new float params, assign in body
```

### `C6StatResolver.BuildContext()` — five new ResolveFloat calls

```csharp
return new GameStatsContext(
    // existing params ...
    fuelStartingAmount:           ResolveFloat(StatKeys.FuelStartingAmount),
    fuelDecayRate:                ResolveFloat(StatKeys.FuelDecayRate),
    fuelPerKill:                  ResolveFloat(StatKeys.FuelPerKill),
    fuelDiminishingReturnsFactor: ResolveFloat(StatKeys.FuelDiminishingReturnsFactor),
    fuelMinExtension:             ResolveFloat(StatKeys.FuelMinExtension)
);
```

### Success Criteria

#### Automated
- [ ] Project compiles with no errors.
- [ ] `Print Resolved Context` context menu shows all five new fuel fields at their baseline values.
- [ ] No warnings in Console on Play.

---

## Phase 2 — FuelManager MonoBehaviour

New file: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/FuelManager.cs`

```csharp
using UnityEngine;

/// <summary>
/// Tracks run fuel. All tuning values come from GameStatsContext via C6StatResolver —
/// no inspector-settable tuning here. All player-facing stats live in C6 so P1a/P1b
/// have a single authoritative place to define upgrade effects.
/// </summary>
public class FuelManager : SingletonMonoBehaviour<FuelManager>
{
    [SerializeField] private C6StatResolver _resolver;

    private GameStatsContext _context;
    private float _fuel;
    private int _killCount;
    private bool _hasWarnedLowFuel;
    private bool _runEnded;
    private bool _initialized;

    private const float LowFuelThreshold = 3f;

    private void Start()
    {
        if (_resolver == null)
        {
            Debug.LogWarning("[FuelManager] _resolver is not set. Assign C6StatResolver in the inspector.", this);
            return;
        }
        _context = _resolver.GetContext();
        _fuel = _context.FuelStartingAmount;
        _killCount = 0;
        _hasWarnedLowFuel = false;
        _runEnded = false;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized || _runEnded) return;

        _fuel -= _context.FuelDecayRate * Time.deltaTime;
        CheckThresholds();
    }

    private void CheckThresholds()
    {
        if (!_hasWarnedLowFuel && _fuel <= LowFuelThreshold)
        {
            _hasWarnedLowFuel = true;
            // TODO A1: play low-fuel warning sound here
        }

        if (_fuel <= 0f)
            TriggerRunEnd();
    }

    /// <summary>
    /// Called directly by BoomerangTarget on death.
    /// Extension is only applied when FuelPerKill > 0 (i.e., a skill node has upgraded it).
    /// At baseline (FuelPerKill = 0) kills give 0s; FuelMinExtension is only the floor
    /// once FuelPerKill is above zero.
    /// </summary>
    public void OnTargetKilled()
    {
        if (!_initialized || _runEnded) return;

        if (_context.FuelPerKill > 0f)
        {
            float extension = _context.FuelPerKill
                * Mathf.Pow(_context.FuelDiminishingReturnsFactor, _killCount);
            _fuel += Mathf.Max(_context.FuelMinExtension, extension);
        }
        _killCount++;
    }

    private void TriggerRunEnd()
    {
        _runEnded = true;
        // TODO U1 (#1): update HUD fuel bar to empty here
        // TODO C5: trigger scene/run-end flow here
        Debug.LogWarning("[FuelManager] Run ended: fuel depleted.");
    }
}
```

### Success Criteria

#### Automated
- [ ] Project compiles with no errors.

#### Manual
- [ ] Play mode: fuel visible decaying in a debug watch or `Debug.Log`; `[FuelManager] Run ended` appears after ~20s.
- [ ] No `_resolver not set` warning when scene is properly wired (Phase 4).

---

## Phase 3 — Wire Kill Signal in BoomerangTarget

### `BoomerangTarget.cs` — `TakeDamage()`

At the point where `IsAlive` is set false, add one direct call:

```csharp
FuelManager.Instance.OnTargetKilled();
```

### Success Criteria

#### Automated
- [ ] Project compiles with no errors.

#### Manual
- [ ] Killing a target in Play mode throws no errors.
- [ ] With `FuelPerKill` temporarily bumped in `Specs` to `3.0f` for testing: confirm `_fuel` increases after a kill (add a temporary `Debug.Log(_fuel)` in `OnTargetKilled`, remove after).

---

## Phase 4 — Scene Wiring

- Add a `FuelManager` GameObject to `Game.unity`.
- Assign `C6StatResolver` in the `_resolver` inspector slot.
- Save the scene.

### Success Criteria

#### Manual
- [ ] No `[FuelManager] _resolver is not set` warning on Play.
- [ ] Full loop verified: play → fuel drains → 3s threshold log appears → fuel hits 0 → run-end log appears.

---

## References

- Research: `thoughts/shared/research/2026-04-24-e1-fuel-economy-codebase-map.md`
- C6 Plan: `thoughts/shared/plans/2026-04-24-c6-stat-resolver.md`
- E1 spec: `design/gdd/systems-index.md:36-44`
- U1 HUD: GitHub issue #1
