# INC-42 — E2Manager Enemy Death Wiring: Implementation Plan

## Overview

Create an `E2Manager` singleton and wire a direct call to `E2Manager.Instance.OnEnemyDied()` inside `BoomerangTarget.TakeDamage()`, mirroring the existing `FuelManager.OnTargetKilled()` integration. This establishes the infrastructure plumbing that INC-46 will use to implement actual currency drop behavior.

---

## Current State Analysis

### What Exists
- `BoomerangTarget.TakeDamage(int damage, Vector2 contactPoint)` — sole enemy death entry point. `contactPoint` (world position) is already available but unused inside the method. (`_Scripts/Gameplay/BoomerangTarget.cs:13–19`)
- `FuelManager` — the established on-kill singleton pattern. Direct call from `BoomerangTarget`. Initialized by `RunManager.StartRun()` with `Initialize(GameStatsContext)`. Guard flags (`_initialized`, `_runEnded`). (`_Scripts/Gameplay/FuelManager.cs`)
- `SingletonMonoBehaviour<T>` — base class for all singleton MonoBehaviours, uses Odin `SerializedMonoBehaviour`. (`_Scripts/Utility/Singletons.cs:44`)
- `RunManager.StartRun()` — calls `FuelManager.Initialize(context)` at line ~58. (`_Scripts/Gameplay/RunManager.cs`)

### What Is Missing
- No `E2Manager.cs` exists anywhere in `_Scripts`.
- No `E2Manager` call from `BoomerangTarget`.
- No `E2Manager.Initialize` call from `RunManager`.

### Key Constraints
- **Direct call only**: `EventRouter` is explicitly forbidden by `src/CLAUDE.md`. Direct singleton call matches `FuelManager` pattern.
- **XP is out of scope**: `XPLedger` does not exist yet (blocked by INC-25).
- **Currency logic is out of scope**: INC-46 implements `OnEnemyDied` business logic (yield calculation, DollarPickup spawn). This plan only creates the stub.
- **Enemy type**: Only one enemy type exists. Hardcode `"Asteroid"` for now; INC-14 will vary it when ore tiers ship.

---

## Desired End State

After this plan is complete:
1. `E2Manager.cs` exists as a `SingletonMonoBehaviour<E2Manager>` with `Initialize(GameStatsContext)`, `OnEnemyDied(Vector2 position, string enemyType)` (stub), and `EndRun()`.
2. `BoomerangTarget.TakeDamage()` calls `E2Manager.Instance.OnEnemyDied(contactPoint, "Asteroid")` after the `FuelManager` call.
3. `RunManager.StartRun()` calls `E2Manager.Instance.Initialize(context)` alongside `FuelManager.Initialize(context)`.
4. A GameObject in the gameplay scene carries the `E2Manager` component (mirroring FuelManager's placement).
5. Killing an enemy in Play Mode triggers `E2Manager.OnEnemyDied` with no console errors.

---

## What We're NOT Doing

- **XP drop wiring** — blocked by INC-25 (`XPLedger` does not exist). Will be added when INC-25 ships.
- **Currency drop logic** — INC-46 implements `OnEnemyDied` body: yield calculation (`oreTier × CurrencyYieldMultiplier`), spawning `DollarPickup` prefab.
- **Ore tier field on BoomerangTarget** — INC-46 adds `public int oreTier`. This plan hardcodes `"Asteroid"` string for type identification only.
- **CurrencyYieldMultiplier stat pipeline** — INC-46 extends `StatKeys`, `GameStatsContext`, and `C6StatResolver`.
- **DollarPickup changes** — INC-46 adds `SetValue(int)` to `DollarPickup`.

---

## Implementation Approach

Follow the `FuelManager` pattern exactly — same class structure, same initialization handshake with `RunManager`, same direct-call wiring in `BoomerangTarget`. The `OnEnemyDied` method is a no-op stub so INC-46 can fill it in without touching the wiring.

---

## Phase 1: Create E2Manager

### Overview
Create `E2Manager.cs` as a `SingletonMonoBehaviour<E2Manager>` following the `FuelManager` skeleton. `OnEnemyDied` is an empty stub; the guard flags prevent accidental calls outside a run.

### Changes Required

#### 1. New File: `E2Manager.cs`
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/E2Manager.cs`

```csharp
using UnityEngine;

public class E2Manager : SingletonMonoBehaviour<E2Manager>
{
    private bool _initialized;
    private bool _runEnded;

    public void Initialize(GameStatsContext context)
    {
        _initialized = true;
        _runEnded = false;
    }

    public void EndRun()
    {
        _runEnded = true;
    }

    public void OnEnemyDied(Vector2 position, string enemyType)
    {
        if (!_initialized || _runEnded) return;
        // INC-46 will implement: yield calculation + DollarPickup spawn
    }
}
```

### Success Criteria

#### Automated Verification
- [ ] Unity compiles with no errors: check via `mcp__coplay-mcp__check_compile_errors`

#### Manual Verification
- [ ] `E2Manager.cs` appears in the Unity Project window under `_Scripts/Gameplay/`

---

## Phase 2: Wire E2Manager into BoomerangTarget and RunManager

### Overview
Add the direct call from `BoomerangTarget.TakeDamage()` and the `Initialize` call in `RunManager.StartRun()`. Place E2Manager component on a scene GameObject.

### Changes Required

#### 1. `BoomerangTarget.TakeDamage` — add E2Manager call
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/BoomerangTarget.cs`
**Change**: Add `E2Manager.Instance.OnEnemyDied(contactPoint, "Asteroid");` after the `FuelManager` call.

```csharp
public void TakeDamage(int damage, Vector2 contactPoint)
{
    if (!IsAlive) return;
    IsAlive = false;
    gameObject.SetActive(false);
    FuelManager.Instance.OnTargetKilled();
    E2Manager.Instance.OnEnemyDied(contactPoint, "Asteroid");
}
```

#### 2. `RunManager.StartRun` — initialize E2Manager
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/RunManager.cs`
**Change**: Add `E2Manager.Instance.Initialize(context);` directly after the existing `FuelManager.Instance.Initialize(context);` call.

Also call `E2Manager.Instance.EndRun()` wherever `FuelManager`'s equivalent end-of-run method is called (check `RunManager` for the EndRun/StopRun pattern and mirror it).

#### 3. Scene placement — add E2Manager component
**Action**: Locate the GameObject in the gameplay scene that holds `FuelManager`. Add an `E2Manager` component to the same GameObject (or a sibling manager object) so the singleton is present at runtime.

### Success Criteria

#### Automated Verification
- [ ] Unity compiles with no errors: `mcp__coplay-mcp__check_compile_errors`

#### Manual Verification
- [ ] Enter Play Mode in the gameplay scene — no "Instance is null" or NullReferenceException logged in the Unity console
- [ ] Kill one enemy — no console errors fire
- [ ] (Optional) Add a temporary `Debug.Log($"E2Manager.OnEnemyDied pos={position} type={enemyType}");` to `OnEnemyDied`, verify it fires on kill, then remove the log before committing

---

## Testing Strategy

### Manual Testing Steps
1. Open the gameplay scene in Unity.
2. Enter Play Mode.
3. Throw the boomerang at an enemy and destroy it.
4. Confirm: no NullReferenceException, no "Instance is null" warnings in the console.
5. Confirm: `E2Manager.OnEnemyDied` is reached (temporary Debug.Log or breakpoint).
6. Confirm: existing fuel behavior (`FuelManager`) still works correctly after adding the second call.

---

## References

- Ticket: `thoughts/shared/tickets/INC-42.md`
- Research: `thoughts/shared/research/2026-04-25-INC-42-enemy-death-event-e2.md`
- Pattern reference: `_Scripts/Gameplay/FuelManager.cs`
- Death entry point: `_Scripts/Gameplay/BoomerangTarget.cs:13–19`
- Singleton base: `_Scripts/Utility/Singletons.cs:44`
- Run initialization: `_Scripts/Gameplay/RunManager.cs`
- Follow-on ticket (currency logic): INC-46
