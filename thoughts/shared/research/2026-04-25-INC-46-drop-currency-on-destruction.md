---
date: 2026-04-25T03:50:00Z
researcher: Claude (ralph_research)
git_commit: dfd4ae1d0105e238466410299200fe5a012b3b7d
branch: main
repository: IncrementalAsteroidsBoomerang
topic: "INC-46 — Drop currency on full destruction (baseNumber × yield multiplier)"
tags: [research, codebase, currency, enemy, death, DollarPickup, E2Manager, GameStatsContext, BoomerangTarget]
status: complete
last_updated: 2026-04-25
last_updated_by: Claude
---

# Research: INC-46 — Drop currency on full destruction (baseNumber × yield multiplier)

**Date**: 2026-04-25
**Git Commit**: dfd4ae1d0105e238466410299200fe5a012b3b7d
**Branch**: main
**Repository**: IncrementalAsteroidsBoomerang

## Research Question

INC-46 says: "Yield = baseNumber × GameStatsContext yield multiplier. Spawn a currency pickup prefab." What does the codebase have today, and what is missing before this can be implemented?

---

## Summary

None of the three key primitives in INC-46 — `baseNumber` (ore tier), a yield-multiplier stat in `GameStatsContext`, or an E2Manager — exist in the codebase yet. The `DollarPickup` prefab/MonoBehaviour exists but cannot receive a runtime value without a code change. The death entry point (`BoomerangTarget.TakeDamage`) already receives `contactPoint` (world position) and is where the spawn call must be added. The parent ticket (INC-14) clarifies that `baseNumber` refers to **ore tier** — a per-asteroid property that is also not yet implemented. Implementing INC-46 fully requires: one new field on `BoomerangTarget` (ore tier placeholder), three new entries in the `StatKeys → GameStatsContext → C6StatResolver` pipeline, a small change to `DollarPickup` for runtime value injection, and a new `E2Manager` singleton wired by a direct call from `BoomerangTarget.TakeDamage`.

---

## Detailed Findings

### 1. Death Entry Point — `BoomerangTarget.TakeDamage`

**File**: `_Scripts/Gameplay/BoomerangTarget.cs:13–19`

```csharp
public void TakeDamage(int damage, Vector2 contactPoint)
{
    if (!IsAlive) return;
    IsAlive = false;
    gameObject.SetActive(false);
    FuelManager.Instance.OnTargetKilled(); // only on-kill hook today
}
```

- `contactPoint` (world position) is already present but **never used** inside the method — it is available for passing to a currency drop spawn.
- `SetActive(false)` is used, not `Destroy` — the GameObject persists in memory, deactivated.
- No `baseNumber` / ore-tier field exists on `BoomerangTarget`. All enemies are identical.
- Pattern: direct singleton call. EventRouter is **not** to be used; direct calls are required.

**Call chain**:
```
BoomerangProjectile.OnCollision()
  → BoomerangController.OnProjectileContact(target, point)
    → BoomerangTarget.TakeDamage(damage, contactPoint)
      → FuelManager.Instance.OnTargetKilled()   // only on-kill side-effect today
```

### 2. `baseNumber` — Does NOT Exist

- Neither `baseNumber`, `BaseNumber`, nor any ore-tier concept appears anywhere in `_Scripts`.
- The parent ticket INC-14 (G5 — Asteroid Mining) clarifies: "Ore tier assignment at spawn: tier determines base currency yield." So `baseNumber` = ore tier, a per-asteroid `int` that must be added to `BoomerangTarget`.
- `AsteroidMover` has only one serialized field: `_speed = 3f`. No resource-value field.
- `BoomerangTarget` is a generic component (also used by `EnemyBox.prefab`). It has no type-specific value field.
- For the minimal INC-46 implementation, a `public int oreTier = 1` field on `BoomerangTarget` is sufficient (inspector-settable; future wave tables will vary it per INC-14).

### 3. Yield Multiplier Stat — Does NOT Exist

**Files**: `_Scripts/Gameplay/Stats/StatKeys.cs`, `GameStatsContext.cs`, `C6StatResolver.cs`

- `StatKeys.cs` defines exactly **11 constants**: 6 core combat + 5 E1 fuel. No E2 keys.
- `GameStatsContext.cs` is a `readonly struct` with exactly **11 fields** mirroring the keys:
  - Boomerang: `BaseDamage` (int), `ThrowCooldown`, `ArcRadius`, `ArcFlightTime`, `PierceFalloff` (float), `ChainCount` (int)
  - Fuel/E1: `FuelStartingAmount`, `FuelDecayRate`, `FuelPerKill`, `FuelDiminishingReturnsFactor`, `FuelMinExtension` (all float)
  - No yield multiplier, currency drop rate, or ore-tier-related field exists.
- `C6StatResolver.cs` aggregates via a `Specs` static dictionary. Adding a new stat requires an entry here **and** a new field in `GameStatsContext` and a new constant in `StatKeys`.
- Runtime read pattern: `C6StatResolver.GetContext().FieldName` — no string lookup at runtime.

Three files must be extended to add a `CurrencyYieldMultiplier` stat (default 1.0, min 1.0, no hard max defined in GDD yet).

### 4. `DollarPickup` — Exists, but Value is Baked In

**File**: `_Scripts/Gameplay/DollarPickup.cs`

```csharp
[RequireComponent(typeof(Collider2D))]
public class DollarPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerWallet.Instance.AddMoney(value);
            Destroy(gameObject);
        }
    }
}
```

- `value` is `[SerializeField] private int` — there is no public setter or init method.
- **Not spawned anywhere** — no script references the class or its prefab via code. It is only an asset.
- `value` is not set in `Awake`, so a post-`Instantiate` field write is safe before any frame ticks.
- To inject a runtime currency amount at spawn, a `public void SetValue(int v)` method must be added. Without this, the only workaround is spawning multiple single-credit pickups, which degrades at large yields.

**Money prefab**: `Assets/Prefabs/Money.prefab`
- Components: `SpriteRenderer`, `CircleCollider2D` (radius 0.5, **IsTrigger: true**), `DollarPickup` (value: 1)
- Scale: `(0.25, 0.25, 0.25)`

**PlayerWallet**: `Assets/_Scripts/Player/PlayerWallet.cs`

```csharp
public class PlayerWallet : SingletonMonoBehaviour<PlayerWallet>
{
    public int Money;
    public void AddMoney(int amount) => Money += amount;
}
```

- In-memory only. No event fired. Persistence is being added separately (INC-25).

### 5. E2Manager — Does NOT Exist

- No `E2Manager.cs` file exists anywhere in `_Scripts`.
- INC-42 (call E2 manager on enemy death) was researched on 2026-04-25 and documents the intended structure, but has **not been implemented**.
- `E2Manager` must be created as a `SingletonMonoBehaviour<E2Manager>` following the `FuelManager` pattern.

### 6. `FuelManager` — Pattern Reference for E2Manager

**File**: `_Scripts/Gameplay/FuelManager.cs`

- Extends `SingletonMonoBehaviour<FuelManager>` (`_Scripts/Utility/Singletons.cs:44`).
- Initialized externally by `RunManager.StartRun():58` via `Initialize(GameStatsContext context)` — caches the context, resets all state.
- Guards all logic with `if (!_initialized || _runEnded) return`.
- `OnTargetKilled()` is the direct-call receiver from `BoomerangTarget`.

E2Manager should follow this structure identically: external `Initialize(GameStatsContext)`, guard flags, and a `OnEnemyDied(Vector2 position, int oreTier)` receiver.

### 7. Spawn Pattern — Raw Instantiate, No Pooling

**File**: `_Scripts/Gameplay/AsteroidSpawner.cs:69`

```csharp
var go = Instantiate(_asteroidPrefab, spawnPos, Quaternion.identity);
var component = go.GetComponent<ComponentType>();
if (component == null) { Debug.LogWarning(...); Destroy(go); return; }
component.Initialize(args);
```

- All runtime spawning in the project uses raw `Instantiate` with a `[SerializeField] GameObject` prefab field.
- No object pooling system exists anywhere in `_Scripts`.
- The same pattern should be used for spawning `DollarPickup`: `[SerializeField] GameObject _currencyPickupPrefab` on E2Manager.

### 8. Stat Reading Patterns

Two established patterns for reading stats:

```csharp
// Pull at run-start (BoomerangController approach):
_stats = _statResolver.GetContext();

// Receive via Initialize at run-start (FuelManager approach):
public void Initialize(GameStatsContext context) { _context = context; }
```

The yield multiplier (once added to `GameStatsContext`) would be read from the frozen context via the `Initialize` pattern, matching FuelManager.

### 9. INC-25 (Persist Credits) — Plan Exists, Not Yet Implemented

**File**: `thoughts/shared/plans/2026-04-26-INC-25-persist-credits-xp-across-runs.md`

- A complete implementation plan for `PlayerWallet` persistence via FBPP has been written but not executed.
- INC-25 will add `SubtractMoney` and a private setter to `Money`. INC-46 only calls `AddMoney` (via `DollarPickup`) — no conflict with INC-25.
- `XPLedger` (planned in INC-25) is a separate system from currency drop — INC-46 does not depend on it.

---

## Code References

| File | Key Detail |
|------|-----------|
| `_Scripts/Gameplay/BoomerangTarget.cs:13–19` | `TakeDamage(int, Vector2)` — sole death entry; `contactPoint` available, unused |
| `_Scripts/Gameplay/DollarPickup.cs` | `[SerializeField] private int value = 1` — no runtime setter exists; not set in Awake |
| `_Scripts/Player/PlayerWallet.cs` | `AddMoney(int)` — sole currency API; in-memory only |
| `Assets/Prefabs/Money.prefab` | Trigger collider + DollarPickup (value=1); exists but never spawned |
| `Assets/Prefabs/Asteroid.prefab` | BoomerangTarget + AsteroidMover; no currency value field |
| `_Scripts/Gameplay/AsteroidMover.cs` | Only field: `_speed`; no baseNumber |
| `_Scripts/Gameplay/FuelManager.cs:24–32,60–74` | Pattern reference: `Initialize`, guard flags, `OnTargetKilled` receiver |
| `_Scripts/Gameplay/AsteroidSpawner.cs:69` | `Instantiate` pattern — `[SerializeField] GameObject` prefab field |
| `_Scripts/Gameplay/Stats/StatKeys.cs` | 11 keys, no E2 entries |
| `_Scripts/Gameplay/Stats/GameStatsContext.cs` | 11 fields, no E2 fields; immutable struct |
| `_Scripts/Gameplay/Stats/C6StatResolver.cs:34–49` | `Specs` dict — extension point for new stats |
| `_Scripts/Gameplay/Stats/UpgradeSource.cs` | Abstract base for stat producers (`GetDeltas()`) |
| `_Scripts/Gameplay/RunManager.cs:58` | Calls `FuelManager.Initialize` — E2Manager must be initialized here too |
| `_Scripts/Utility/Singletons.cs:44` | `SingletonMonoBehaviour<T>` base (uses Odin `SerializedMonoBehaviour`) |

---

## Architecture Documentation

### What Exists Today

```
[Asteroid death]
BoomerangTarget.TakeDamage(damage, contactPoint)
  → SetActive(false)
  → FuelManager.Instance.OnTargetKilled()
  [NOTHING ELSE — no currency drop]

[Currency pickup]
Money.prefab (never Instantiated)
  → DollarPickup.OnTriggerEnter2D → PlayerWallet.Instance.AddMoney(value)
```

### Intended Flow After INC-46

```
BoomerangTarget.TakeDamage(damage, contactPoint)
  → FuelManager.Instance.OnTargetKilled()
  → E2Manager.Instance.OnEnemyDied(contactPoint, oreTier)
       yield = Mathf.RoundToInt(oreTier × context.CurrencyYieldMultiplier)
       Instantiate(_currencyPickupPrefab, contactPoint, ...)
       pickup.SetValue(yield)

[Stat Pipeline Extension]
StatKeys.cs          → add: const string CurrencyYieldMultiplier = "currency_yield_multiplier"
GameStatsContext.cs  → add: readonly float CurrencyYieldMultiplier (baseline: 1.0)
C6StatResolver.cs    → add: entry in Specs dict (baseline=1.0, min=1.0) + line in BuildContext()
```

---

## Historical Context

- `thoughts/shared/research/2026-04-25-INC-42-enemy-death-event-e2.md` — documents the E2 integration approach in detail. Confirmed direct-call pattern, `E2Manager` as singleton, `contactPoint` available. E2Manager not yet built.
- `thoughts/shared/research/2026-04-24-e1-fuel-economy-codebase-map.md` — Documents that `GameStatsContext` has no E2 drop-rate fields; describes the triple-extension pattern (StatKeys + Specs + GameStatsContext) for adding new stat keys.
- `thoughts/shared/plans/2026-04-24-c6-stat-resolver.md` — The plan that produced the C6 implementation.
- `thoughts/shared/plans/2026-04-26-INC-25-persist-credits-xp-across-runs.md` — INC-25 implementation plan; `PlayerWallet` changes are compatible with INC-46's `AddMoney` usage.

---

## Risks and Concerns

1. **INC-42 not yet implemented**: E2Manager must be created as part of INC-46 (or INC-42 must ship first). The two tickets share the same new singleton.
2. **`DollarPickup.value` is private**: A small change is required to allow runtime value injection. Without it, only fixed-value pickups are possible.
3. **`C6StatResolver.GetContext()` requires `RunActive` state**: E2Manager must not call `GetContext()` before `RunManager.StartRun()` completes aggregation — the `Initialize(context)` pattern safely avoids this.
4. **Ore tier is a placeholder**: When INC-14's ore tier assignment ships, the `BoomerangTarget.oreTier` field will need to be set at spawn time by `AsteroidSpawner`.

---

## Open Questions

1. **Should `E2Manager` be created first (INC-42), or can INC-46 create it?** INC-42 is not implemented yet. They share the same singleton — INC-46 should create E2Manager to avoid duplicate work.
2. **Where does `baseNumber` live?** For MVP: `public int oreTier = 1` on `BoomerangTarget` (inspector-settable). Future wave tables (INC-14) will vary this at spawn time via `AsteroidSpawner`.
3. **Should `DollarPickup` have an explicit `SetValue(int)` method?** Yes — cleaner than exposing the field directly; consistent with the `Initialize` pattern used elsewhere.
4. **What is the `CurrencyYieldMultiplier` stat range?** No GDD max is defined — use `float.MaxValue` in `C6StatResolver.Specs` until the GDD is updated.
5. **Spawn at `contactPoint` or asteroid `transform.position`?** `contactPoint` — asteroid's `transform.position` is unavailable after `SetActive(false)`.
