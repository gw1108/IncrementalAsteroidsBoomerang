# INC-46 — Drop Currency on Full Destruction: Implementation Plan

## Overview

Wire asteroid destruction to spawn a `DollarPickup` prefab with yield = `oreTier × CurrencyYieldMultiplier`. This requires: a `CurrencyYieldMultiplier` stat added through the triple-extension pipeline, a `SetValue` injection method on `DollarPickup`, an `oreTier` placeholder field on `BoomerangTarget`, and the full body of `E2Manager.OnEnemyDied` (including a signature update from the INC-42 stub).

---

## Current State Analysis

### What Exists
- `BoomerangTarget.TakeDamage(int damage, Vector2 contactPoint)` — sole death entry point. `contactPoint` is available but unused for currency. (`_Scripts/Gameplay/BoomerangTarget.cs:13–19`)
- `Money.prefab` — `SpriteRenderer + CircleCollider2D (trigger, r=0.5) + DollarPickup (value=1)`. Scale `(0.25, 0.25, 0.25)`. **Never spawned at runtime.**
- `DollarPickup.cs` — `[SerializeField] private int value = 1`. On player trigger: calls `PlayerWallet.Instance.AddMoney(value)` then `Destroy`. No public setter or init method. **Not Awake-initialized, so post-Instantiate field write is safe.**
- `PlayerWallet.cs` — `SingletonMonoBehaviour<PlayerWallet>`. `public int Money; public void AddMoney(int amount)`. In-memory only; persistence added by INC-25 independently.
- `StatKeys.cs` — 11 constants (6 boomerang + 5 fuel). No E2/currency keys.
- `GameStatsContext.cs` — 11 matching `readonly` fields. No yield multiplier.
- `C6StatResolver.cs` — `Specs` static dictionary as the extension point for new stats.
- `FuelManager.cs` — canonical singleton pattern: `Initialize(GameStatsContext)`, guard flags, `OnTargetKilled()` direct call from `BoomerangTarget`.
- `AsteroidSpawner.cs:69` — canonical `Instantiate(prefab, pos, Quaternion.identity)` pattern.

### What Is Missing
- `CurrencyYieldMultiplier` stat (StatKeys + GameStatsContext + C6StatResolver all need extending).
- `oreTier` field on `BoomerangTarget` (placeholder `int`, default 1).
- `SetValue(int)` method on `DollarPickup`.
- `E2Manager.OnEnemyDied` business logic: yield calc + DollarPickup spawn.
- `_currencyPickupPrefab` field on `E2Manager` wired to `Money.prefab` in scene.

### Prerequisite: INC-42 E2Manager stub
INC-42 defines `E2Manager` as a `SingletonMonoBehaviour<E2Manager>` with:
```csharp
public void OnEnemyDied(Vector2 position, string enemyType) { ... }  // stub
```
**This plan changes the signature** to `OnEnemyDied(Vector2 position, int oreTier)` — the `string enemyType` is dropped since there is only one enemy type and `oreTier` is what drives yield. The `BoomerangTarget` call site is updated accordingly.

If INC-42 has not yet been executed, Phase 1 of this plan must include E2Manager creation + RunManager wiring per the INC-42 plan before proceeding.

---

## Desired End State

1. Killing any asteroid spawns a `Money.prefab` pickup at `contactPoint` with value = `Mathf.RoundToInt(oreTier × context.CurrencyYieldMultiplier)`.
2. The player walking into the pickup triggers `PlayerWallet.AddMoney` and the pickup destroys itself.
3. `CurrencyYieldMultiplier` defaults to `1.0f` in `GameStatsContext`; upgrading it increases currency yield.
4. All asteroids default to `oreTier = 1`; inspector override is possible per-prefab.
5. No Unity console errors or warnings.

---

## Key Discoveries

- `DollarPickup.value` is `[SerializeField] private int` — not set in `Awake`, so `GetComponent<DollarPickup>().SetValue(n)` immediately after `Instantiate` is safe. (`_Scripts/Gameplay/DollarPickup.cs`)
- `contactPoint` is a `Vector2` — spawn with `new Vector3(contactPoint.x, contactPoint.y, 0f)` (asteroid `transform.position` is unavailable post `SetActive(false)`).
- Triple-extension pattern confirmed: StatKeys → GameStatsContext → C6StatResolver. (`_Scripts/Gameplay/Stats/`)
- `FuelManager.Initialize` called at `RunManager.StartRun():58`. E2Manager must be initialized there too (INC-42 adds this; INC-46 relies on it).
- `SingletonMonoBehaviour<T>` is in `_Scripts/Utility/Singletons.cs:44` — uses Odin `SerializedMonoBehaviour`.

---

## What We're NOT Doing

- **XP drop** — `XPLedger` is out of scope; blocked by INC-25.
- **Object pooling** — raw `Instantiate` is the project standard; no pool.
- **Ore tier assignment at spawn** — INC-14's wave system will set `oreTier` per-asteroid at spawn time. This plan only adds the placeholder field.
- **PlayerWallet persistence** — handled by INC-25, which is compatible with `AddMoney` usage here.
- **Max stat cap** — no GDD max defined for `CurrencyYieldMultiplier`; use `float.MaxValue` in Specs until GDD is updated.
- **Multiple pickup spawns** — one pickup per kill, value encodes the full yield amount.

---

## Implementation Approach

Work in small, independently compilable slices. Phases 1–3 are pure code changes with no scene impact; Phase 4 fills in the E2Manager body; Phase 5 wires the scene and validates end-to-end.

---

## Phase 1: Stat Pipeline — Add CurrencyYieldMultiplier

### Overview
Extend the three-file stat pipeline to add a `CurrencyYieldMultiplier` stat (float, baseline 1.0, min 1.0).

### Changes Required

#### 1. StatKeys.cs
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/StatKeys.cs`
**Change**: Add constant alongside the existing 11.

```csharp
public const string CurrencyYieldMultiplier = "currency_yield_multiplier";
```

#### 2. GameStatsContext.cs
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/GameStatsContext.cs`
**Change**: Add field and extend constructor parameter list (match existing field count + 1).

```csharp
public readonly float CurrencyYieldMultiplier;
// In constructor: add float currencyYieldMultiplier parameter and assignment
//   this.CurrencyYieldMultiplier = currencyYieldMultiplier;
```

#### 3. C6StatResolver.cs
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/C6StatResolver.cs`
**Change**: Add entry to `Specs` dictionary and resolution line in `BuildContext()`.

```csharp
// In Specs dict:
{ StatKeys.CurrencyYieldMultiplier, new FieldSpec(baseline: 1.0f, min: 1.0f, max: float.MaxValue, integerTier: false) },

// In BuildContext() resolution block:
float currencyYieldMultiplier = Resolve(StatKeys.CurrencyYieldMultiplier);
// Pass as new argument to GameStatsContext constructor (last position)
```

### Success Criteria

#### Automated Verification
- [ ] Unity compiles with no errors: `mcp__coplay-mcp__check_compile_errors`

#### Manual Verification
- [ ] No new console errors in Unity

---

## Phase 2: DollarPickup — Add SetValue

### Overview
Add a public setter so E2Manager can inject the computed yield value at spawn time.

### Changes Required

#### 1. DollarPickup.cs
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/DollarPickup.cs`
**Change**: Add one method below the `value` field.

```csharp
public void SetValue(int v) => value = v;
```

### Success Criteria

#### Automated Verification
- [ ] Unity compiles with no errors: `mcp__coplay-mcp__check_compile_errors`

---

## Phase 3: BoomerangTarget — Add oreTier

### Overview
Add an inspector-settable `oreTier` field (default `1`) to `BoomerangTarget`. All enemies currently are tier 1. Future wave tables (INC-14) will vary this at spawn time.

### Changes Required

#### 1. BoomerangTarget.cs
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/BoomerangTarget.cs`
**Change**: Add public field. Also update the `E2Manager.Instance.OnEnemyDied` call to pass `oreTier` instead of `"Asteroid"` (changes the signature, see Phase 4).

```csharp
public int oreTier = 1;
```

Updated call in `TakeDamage`:
```csharp
E2Manager.Instance.OnEnemyDied(contactPoint, oreTier);
```

### Success Criteria

#### Automated Verification
- [ ] Unity compiles with no errors (Phase 4 must also be complete before compile check passes since `OnEnemyDied` signature changes)

---

## Phase 4: E2Manager — Implement OnEnemyDied

### Overview
Update `E2Manager.OnEnemyDied` signature from `string enemyType` to `int oreTier`, add the `_currencyPickupPrefab` field, and implement yield calculation + pickup spawn.

### Changes Required

#### 1. E2Manager.cs
**File**: `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/E2Manager.cs`

```csharp
using UnityEngine;

public class E2Manager : SingletonMonoBehaviour<E2Manager>
{
    [SerializeField] private GameObject _currencyPickupPrefab;

    private bool _initialized;
    private bool _runEnded;
    private GameStatsContext _context;

    public void Initialize(GameStatsContext context)
    {
        _context = context;
        _initialized = true;
        _runEnded = false;
    }

    public void EndRun()
    {
        _runEnded = true;
    }

    public void OnEnemyDied(Vector2 position, int oreTier)
    {
        if (!_initialized || _runEnded) return;

        if (_currencyPickupPrefab == null)
        {
            Debug.LogWarning("E2Manager: _currencyPickupPrefab is not assigned.");
            return;
        }

        int yield = Mathf.RoundToInt(oreTier * _context.CurrencyYieldMultiplier);
        var go = Instantiate(_currencyPickupPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);
        var pickup = go.GetComponent<DollarPickup>();
        if (pickup == null)
        {
            Debug.LogWarning("E2Manager: spawned prefab has no DollarPickup component.");
            Destroy(go);
            return;
        }
        pickup.SetValue(yield);
    }
}
```

### Success Criteria

#### Automated Verification
- [ ] Unity compiles with no errors: `mcp__coplay-mcp__check_compile_errors`

---

## Phase 5: Scene Wiring and End-to-End Validation

### Overview
Assign `Money.prefab` to `_currencyPickupPrefab` on the E2Manager scene component, then run the full pickup flow.

### Changes Required

#### 1. Gameplay scene — E2Manager prefab reference
**Action**: In the gameplay scene, select the GameObject that holds the `E2Manager` component. Assign `Assets/Prefabs/Money.prefab` to the `_currencyPickupPrefab` field in the Inspector.

### Success Criteria

#### Automated Verification
- [ ] Unity compiles with no errors: `mcp__coplay-mcp__check_compile_errors`

#### Manual Verification
- [ ] Enter Play Mode in the gameplay scene — no console errors on start.
- [ ] Throw boomerang at an asteroid and destroy it — a `Money` pickup spawns at the impact position.
- [ ] Walk player into the pickup — `PlayerWallet.Money` increments by 1 (default `oreTier=1` × `CurrencyYieldMultiplier=1.0`).
- [ ] No NullReferenceException or "is not assigned" warnings in the console.
- [ ] Killing multiple enemies spawns multiple pickups with no errors.
- [ ] Existing fuel behavior (`FuelManager`) still works correctly after the kill.

---

## Testing Strategy

### Manual Testing Steps
1. Open the gameplay scene.
2. Enter Play Mode.
3. Check console — no errors on start.
4. Destroy one asteroid — confirm `Money` prefab spawns at impact point.
5. Collect the pickup — confirm `PlayerWallet.Money` = 1 in the Inspector.
6. Destroy several asteroids without collecting — confirm multiple pickups exist simultaneously.
7. Confirm fuel bar still decrements on kill (FuelManager regression check).

---

## Performance Considerations

Raw `Instantiate` is the project standard. One pickup per kill; no pooling needed at MVP scale.

---

## Migration Notes

No data migration. `oreTier = 1` default matches existing asteroid behavior exactly. `CurrencyYieldMultiplier` baseline of `1.0f` means unupgraded currency yield = `oreTier` credits per kill, which is the expected baseline.

---

## References

- Ticket: `thoughts/shared/tickets/INC-46.md`
- Research: `thoughts/shared/research/2026-04-25-INC-46-drop-currency-on-destruction.md`
- Prerequisite plan: `thoughts/shared/plans/2026-04-26-INC-42-e2manager-enemy-death-wiring.md`
- Pattern reference: `_Scripts/Gameplay/FuelManager.cs`
- Death entry point: `_Scripts/Gameplay/BoomerangTarget.cs:13–19`
- Pickup prefab: `Assets/Prefabs/Money.prefab`
- Pickup script: `_Scripts/Gameplay/DollarPickup.cs`
- Stat extension point: `_Scripts/Gameplay/Stats/C6StatResolver.cs:34–49`
- Singleton base: `_Scripts/Utility/Singletons.cs:44`
- Run init: `_Scripts/Gameplay/RunManager.cs:58`
