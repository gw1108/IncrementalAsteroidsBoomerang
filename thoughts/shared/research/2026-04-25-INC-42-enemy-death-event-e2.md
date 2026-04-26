---
date: 2026-04-25T03:30:00Z
researcher: Claude (ralph_research)
git_commit: dfd4ae1
branch: main
repository: IncrementalAsteroidsBoomerang
topic: "INC-42 — Call E2 manager on enemy death (currency/XP drop)"
tags: [research, codebase, enemy, death-event, e2, currency, xp, BoomerangTarget, FuelManager]
status: complete
last_updated: 2026-04-25
last_updated_by: Claude
last_updated_note: "Clarified: direct call pattern confirmed by dev; EventRouter not to be used"
---

# Research: INC-42 — Call E2 manager on enemy death (currency/XP drop)

**Date**: 2026-04-25  
**Git Commit**: dfd4ae1  
**Branch**: main  
**Repository**: IncrementalAsteroidsBoomerang  

## Research Question

How should we wire enemy death in `BoomerangTarget` to notify E2 (currency/XP drop), passing enemy type and world position?

**Clarification (2026-04-25)**: Dev confirmed the implementation must use a **direct singleton call** (matching the `FuelManager.OnTargetKilled()` pattern), not `EventRouter` or any pub/sub mechanism.

## Summary

Enemy death currently happens in a single 20-line class with one direct call to `FuelManager`. The established on-kill pattern is a direct singleton call. The confirmed approach for E2 is to add a second direct call to an E2 manager in `BoomerangTarget.TakeDamage()`, mirroring `FuelManager`. XP does not exist yet (blocked by INC-25). Currency pickup (`DollarPickup`) exists as a prefab/MonoBehaviour but is not yet spawned on death. Enemy type differentiation does not exist — all enemies are `BoomerangTarget`.

---

## Detailed Findings

### 1. Enemy Death Entry Point

**File**: `_Scripts/Gameplay/BoomerangTarget.cs`

```csharp
public void TakeDamage(int damage, Vector2 contactPoint)
{
    if (!IsAlive) return;
    IsAlive = false;
    gameObject.SetActive(false);
    FuelManager.Instance.OnTargetKilled(); // only on-kill call
}
```

- This is the **sole death entry point** in the codebase
- `contactPoint` (a `Vector2`) is already passed in — world position is available here
- Enemy type does not exist — all enemies share this class with no differentiating field
- The `gameObject.SetActive(false)` pattern means `OnDisable` fires and removes the target from `BoomerangTarget.All`

### 2. Established On-Kill Pattern (FuelManager)

**File**: `_Scripts/Gameplay/FuelManager.cs:60-74`

`FuelManager.OnTargetKilled()` is called **directly** from `BoomerangTarget.TakeDamage()`. This is the only existing "on enemy death" integration and establishes the direct-call coupling style.

```csharp
// Called directly by BoomerangTarget on death.
public void OnTargetKilled()
{
    if (!_initialized || _runEnded) return;
    if (_context.FuelPerKill > 0f) { ... }
    _killCount++;
}
```

### 3. Coupling Style — Confirmed Direct Call

**File**: `src/CLAUDE.md`

> "Prefer direct function calls over pub/sub or event-broadcast systems (EventRouter, UnityEvent, Action callbacks). Tight coupling between systems is fine — favor simplicity and stack-trace traceability over decoupling abstractions."

Dev confirmed this applies to INC-42. `EventRouter` is **not** to be used. The implementation must add a direct call to an E2 manager singleton from `BoomerangTarget.TakeDamage()`, identical to how `FuelManager.OnTargetKilled()` is called today.

### 5. Currency System (E2 — Partial)

**File**: `_Scripts/Player/PlayerWallet.cs`

```csharp
public class PlayerWallet : SingletonMonoBehaviour<PlayerWallet>
{
    public int Money;
    public void AddMoney(int amount) => Money += amount;
}
```

- The currency accumulation API is simple and ready: `PlayerWallet.Instance.AddMoney(int)`
- Persistence is being added in INC-25 (in progress)

**File**: `_Scripts/Gameplay/DollarPickup.cs`

- A pickup prefab/MonoBehaviour that calls `PlayerWallet.Instance.AddMoney(value)` on player collision
- Currently **not spawned on enemy death** — only exists as an asset
- This is the existing "currency pick-up" delivery mechanism

### 6. XP System — Does Not Exist

- No `XPLedger`, no XP tracking, no stat keys for XP multipliers
- INC-25 (`XPLedger`) is planned but not yet implemented
- The "XP drop" component of the ticket cannot be completed without INC-25

### 7. GameStatsContext — No E2 Stats

**File**: `_Scripts/Gameplay/Stats/GameStatsContext.cs`

Current stats: `BaseDamage`, `ThrowCooldown`, `ArcRadius`, `ArcFlightTime`, `PierceFalloff`, `ChainCount`, and E1 fuel fields. **No drop-rate or XP multiplier fields exist.** Adding E2 stats (e.g., `CurrencyDropAmount`, `XPDropAmount`, yield multiplier) would require:

1. New keys in `StatKeys.cs`
2. New fields in `GameStatsContext.cs`
3. New resolution logic in `C6StatResolver.cs`

### 8. Enemy Type Differentiation — Does Not Exist

`BoomerangTarget` has no `EnemyType` field. All enemies are identical. If the death event needs to carry enemy type (per the ticket description), a type enum or string would need to be added to `BoomerangTarget`.

---

## Code References

| File | Key Detail |
|------|-----------|
| `_Scripts/Gameplay/BoomerangTarget.cs:13-19` | `TakeDamage()` — sole death entry point; `contactPoint` available |
| `_Scripts/Gameplay/FuelManager.cs:60-74` | `OnTargetKilled()` — direct-call pattern for on-kill behavior |
| `_Scripts/Player/PlayerWallet.cs:1-8` | `AddMoney(int)` — currency API |
| `_Scripts/Gameplay/DollarPickup.cs` | Pickup prefab; adds money on player collision; not yet spawned on death |
| `_Scripts/Gameplay/Stats/GameStatsContext.cs` | No E2 drop-rate fields |
| `_Scripts/Gameplay/Stats/StatKeys.cs` | No E2 stat keys |
| `_Scripts/Gameplay/Stats/C6StatResolver.cs` | Aggregates upgrade deltas into GameStatsContext |
| `src/CLAUDE.md` | Coupling style: prefer direct calls over event systems |

---

## Architecture Documentation

### Current On-Kill Flow

```
BoomerangProjectile.OnCollision()
  → BoomerangController.OnProjectileContact(target, point)
    → BoomerangTarget.TakeDamage(damage, contactPoint)
      → FuelManager.Instance.OnTargetKilled()
```

No event is broadcast. Everything is direct calls.

## Implementation Approach (Confirmed)

Add a second direct call in `BoomerangTarget.TakeDamage()`, mirroring `FuelManager.OnTargetKilled()`:

```csharp
public void TakeDamage(int damage, Vector2 contactPoint)
{
    if (!IsAlive) return;
    IsAlive = false;
    gameObject.SetActive(false);
    FuelManager.Instance.OnTargetKilled();
    E2Manager.Instance.OnEnemyDied(transform.position, "Asteroid");
}
```

`E2Manager` is a new `SingletonMonoBehaviour<E2Manager>` to be created. The method signature carries world position and enemy type string. "Asteroid" is the only enemy type today; when new types are added, a type field on `BoomerangTarget` will be needed.

---

## Risks and Concerns

1. **XP half of the ticket is blocked**: INC-25 (`XPLedger`) must exist before XP drop can be wired in. This ticket should be scoped to currency-only until INC-25 ships.
2. **No enemy type field**: `BoomerangTarget` has no type field. Hardcode `"Asteroid"` for now; add a type field when new enemy types are introduced.
3. **GameStatsContext has no drop-rate fields**: Drop quantity/yield can't read from GameStatsContext yet. Either hardcode a baseline (e.g., 1 credit per kill) or extend the stat system as part of this ticket.

---

## Open Questions

1. Is XP drop in scope for INC-42, or should it be split until INC-25 is done?
2. Should drop amount be hardcoded (e.g., `1 credit per kill`) or read from `GameStatsContext`? (impacts C6 scope)
3. Should `DollarPickup` (pickup prefab) be spawned on death, or should credits be added directly to `PlayerWallet`?
