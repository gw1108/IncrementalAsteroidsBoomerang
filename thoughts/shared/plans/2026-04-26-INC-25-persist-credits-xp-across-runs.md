# INC-25 — Persist Credits and XP Across Runs: Implementation Plan

## Overview

Add cross-run persistence for both the credits ledger (`PlayerWallet`) and the XP ledger (new `XPLedger` class) using the already-installed FBPP system. Both values load on `Awake` and save on every mutation. The XP ledger must be created as part of this ticket since it does not yet exist.

---

## Current State Analysis

| System | State |
|--------|-------|
| `PlayerWallet` (`_Scripts/Player/PlayerWallet.cs`) | 8-line class: `public int Money` + `AddMoney`. No Awake, no persistence. |
| `XPLedger` | Does not exist. Zero references to XP in `.cs` files. |
| FBPP | **Installed and initialized** in `RunManager.Awake()` with `AutoSaveData = true`. Confirmed via `RunManager.cs:26`. |
| `SaveSystem` | Has `RunData` (empty struct) + JSON file I/O for structured future data. **Separate from FBPP.** |
| `PlayerPrefs` | Zero usage anywhere — FBPP replaces it per `src/CLAUDE.md`. |
| `FuelManager.TriggerRunEnd()` | Stub — only sets `_runEnded = true` + logs warning. |

**Key insight from codebase research:** The research doc incorrectly stated "FBPP is not installed." FBPP is fully operational — initialized in `RunManager.Awake()` with `AutoSaveData = true`. Every `FBPP.SetInt` call auto-flushes to disk, so save-on-mutation is already zero-cost. No manual flush at `TriggerRunEnd()` is required.

### Key Discoveries

- `SingletonMonoBehaviour<T>` (`Singletons.cs:44`) has no `DontDestroyOnLoad` — singletons reset on scene reload. FBPP load-in-Awake handles this correctly.
- `RunManager.Awake()` runs first (before any `Start()`), so FBPP is always initialized before any singleton's `Awake` runs — load order is safe.
- `SaveSystem.WipeRun()` deletes `player_data.json` (the JSON save file). FBPP keys must be cleaned up separately on new-game reset.
- `PlayerWallet.AddMoney` has no clamp or validation. `SubtractMoney` must clamp to 0 to prevent negative credits.

---

## Desired End State

After this plan is complete:

1. Starting a fresh scene: `PlayerWallet.Money` and `XPLedger.XP` load their last-saved values from FBPP.
2. Calling `AddMoney(n)` / `SubtractMoney(n)` / `AddXP(n)` / `SpendXP(n)` immediately persists the new value to the FBPP save file.
3. Force-closing mid-run loses zero credits or XP (save-on-mutation).
4. A new-game reset wipes both keys from FBPP.
5. No Unity console errors or warnings from these systems.

### Verification

- Unity Play Mode: note credits from pickup → exit Play → re-enter Play → credits are restored.
- Same for XP (requires a caller — test via `XPLedger.Instance.AddXP(10)` in a test MonoBehaviour or the console).
- No FBPP errors in the Unity console.

---

## What We're NOT Doing

- Installing or touching FBPP configuration (already done in `RunManager`).
- Migrating `SaveSystem` JSON save to FBPP — those are separate concerns.
- Building the skill tree spending flow (just add `SubtractMoney` as a clean hook).
- Implementing the full run-end scene transition (C5 scope, not INC-25).
- Adding `DontDestroyOnLoad` to any singleton (unnecessary with save-on-mutation).
- Encrypting or salting individual keys (FBPP handles this globally via `ScrambleSaveData`).

---

## Implementation Approach

Use FBPP directly (not `SaveSystem`, not vanilla `PlayerPrefs`). Each ledger loads its value in `Awake` and writes after every mutation. A single `PersistenceKeys.cs` file owns all key constants so they can never diverge between read and write sites.

---

## Phase 1: Persistence Key Constants

### Overview

Create a thin constants class so all FBPP key strings live in exactly one place.

### Changes Required

#### 1. New File: `PersistenceKeys.cs`

**File:** `_Scripts/Utility/PersistenceKeys.cs`

```csharp
public static class PersistenceKeys
{
    public const string Credits = "player_credits";
    public const string XP = "player_xp";

    /// <summary>Wipes all economy ledger keys. Call on new game start.</summary>
    public static void DeleteEconomyKeys()
    {
        FBPP.DeleteKey(Credits);
        FBPP.DeleteKey(XP);
    }
}
```

### Success Criteria

#### Automated Verification
- [ ] File compiles with no errors: open Unity, check Console for compile errors.

#### Manual Verification
- [ ] `PersistenceKeys.Credits` and `PersistenceKeys.XP` resolve correctly in IDE.

---

## Phase 2: Persist Credits in PlayerWallet

### Overview

Add `Awake` (load) and per-mutation save to `PlayerWallet`. Also add `SubtractMoney` since the skill tree will require it and we are already touching this class.

### Changes Required

#### 1. `PlayerWallet.cs`

**File:** `_Scripts/Player/PlayerWallet.cs`

Replace the current 8-line file:

```csharp
using _Scripts.Utility;
using UnityEngine;

/// <summary>Tracks the player's credit balance across runs.</summary>
public class PlayerWallet : SingletonMonoBehaviour<PlayerWallet>
{
    public int Money { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Money = FBPP.GetInt(PersistenceKeys.Credits, 0);
    }

    /// <summary>Adds credits and persists the new balance.</summary>
    public void AddMoney(int amount)
    {
        Money += amount;
        FBPP.SetInt(PersistenceKeys.Credits, Money);
    }

    /// <summary>Subtracts credits (clamped to 0) and persists the new balance.</summary>
    public void SubtractMoney(int amount)
    {
        Money = Mathf.Max(0, Money - amount);
        FBPP.SetInt(PersistenceKeys.Credits, Money);
    }
}
```

**Notes:**
- `Money` becomes a property with private setter to prevent external mutation that bypasses persistence.
- `base.Awake()` must be called — `SingletonMonoBehaviour.Awake()` at `Singletons.cs:79` assigns the instance.
- `DollarPickup.cs` calls `PlayerWallet.Instance.AddMoney(value)` — no change needed there.

### Success Criteria

#### Automated Verification
- [ ] No compile errors in Unity Console.
- [ ] `DollarPickup.cs` still compiles (it only calls `AddMoney` — unchanged signature).

#### Manual Verification
- [ ] Enter Play Mode → collect a pickup → note `Money` value in the Inspector.
- [ ] Exit Play Mode → re-enter Play Mode → `Money` shows the saved value (not 0).
- [ ] Confirm no errors in Console.

**Pause here for manual verification before proceeding to Phase 3.**

---

## Phase 3: Build XPLedger

### Overview

Create `XPLedger` as a new singleton with the same FBPP save/load pattern as `PlayerWallet`. This is a prerequisite for validating the full INC-25 scope.

### Changes Required

#### 1. New File: `XPLedger.cs`

**File:** `_Scripts/Economy/XPLedger.cs`

```csharp
using _Scripts.Utility;
using UnityEngine;

/// <summary>Tracks the player's XP balance across runs.</summary>
public class XPLedger : SingletonMonoBehaviour<XPLedger>
{
    public int XP { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        XP = FBPP.GetInt(PersistenceKeys.XP, 0);
    }

    /// <summary>Adds XP and persists the new balance.</summary>
    public void AddXP(int amount)
    {
        XP += amount;
        FBPP.SetInt(PersistenceKeys.XP, XP);
    }

    /// <summary>Spends XP (clamped to 0) and persists the new balance.</summary>
    public void SpendXP(int amount)
    {
        XP = Mathf.Max(0, XP - amount);
        FBPP.SetInt(PersistenceKeys.XP, XP);
    }
}
```

**Note:** Create the `_Scripts/Economy/` directory if it does not exist.

#### 2. XPLedger GameObject in the scene

`XPLedger` is a `SingletonMonoBehaviour` — it auto-creates its own `GameObject` if no instance is found (`Singletons.cs:65–70`). No prefab wiring is required, but it is good practice to place one in the scene alongside `PlayerWallet` for visibility.

### Success Criteria

#### Automated Verification
- [ ] No compile errors in Unity Console.

#### Manual Verification
- [ ] Enter Play Mode → call `XPLedger.Instance.AddXP(50)` (via a test script or Unity console).
- [ ] Exit Play Mode → re-enter → `XPLedger.Instance.XP == 50`.
- [ ] No Console errors.

**Pause here for manual verification before proceeding to Phase 4.**

---

## Phase 4: New-Game Reset Hook

### Overview

Wire `PersistenceKeys.DeleteEconomyKeys()` into the future new-game path. The path doesn't exist yet, but `SaveSystem.WipeRun()` is the documented wipe entrypoint — extend it.

### Changes Required

#### 1. `SaveSystem.WipeRun()`

**File:** `_Scripts/Utility/SaveSystem.cs`

Add economy key deletion alongside the JSON file delete:

```csharp
public void WipeRun()
{
    File.Delete(_saveFilePath);
    PersistenceKeys.DeleteEconomyKeys();
}
```

### Success Criteria

#### Automated Verification
- [ ] No compile errors.

#### Manual Verification
- [ ] Manually call `new SaveSystem().WipeRun()` in a test script.
- [ ] Confirm `PlayerWallet.Money == 0` and `XPLedger.XP == 0` after a scene reload.

---

## Testing Strategy

### Manual Testing Steps

1. Enter Play Mode. Collect some pickups. Note `PlayerWallet.Instance.Money` value.
2. Exit Play Mode. Re-enter Play Mode. Confirm `Money` is restored (not 0).
3. Call `XPLedger.Instance.AddXP(100)` in a test script. Exit/re-enter. Confirm `XP == 100`.
4. Call `new SaveSystem().WipeRun()`. Exit/re-enter. Confirm both values are 0.
5. Check Unity Console throughout — no errors or warnings from any of these systems.

### No Automated Tests

This project has no C# test infrastructure. Verification is manual.

---

## Performance Considerations

`AutoSaveData = true` in FBPP means each `FBPP.SetInt` write queues a flush. At the scale of credits-per-pickup or XP-per-kill, this is negligible (single int write, not per-frame). No optimization needed.

---

## Migration Notes

Existing save files have no `player_credits` or `player_xp` keys. `FBPP.GetInt(key, 0)` returns 0 for missing keys — safe default, no migration required.

---

## References

- Ticket: `thoughts/shared/tickets/INC-25.md`
- Research: `thoughts/shared/research/2026-04-25-INC-25-persist-credits-xp-across-runs.md`
- FBPP docs: `src/CLAUDE.md` (FBPP section)
- `PlayerWallet`: `_Scripts/Player/PlayerWallet.cs`
- `RunManager` (FBPP init): `_Scripts/Gameplay/RunManager.cs:21`
- `SaveSystem`: `_Scripts/Utility/SaveSystem.cs`
- `SingletonMonoBehaviour`: `_Scripts/Utility/Singletons.cs:44`
