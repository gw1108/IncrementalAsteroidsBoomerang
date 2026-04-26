# INC-25 Research: Persist Credits and XP Across Runs

**Date:** 2026-04-25  
**Ticket:** INC-25 — Persist credits and XP across runs  
**Parent:** INC-8 — E2 Currency & XP Economy

---

## Summary

The ticket asks for "both ledgers" (credits + XP) to be serialized to PlayerPrefs using FBPP so they survive run boundaries. Research reveals:
- Credits exist in a minimal in-memory singleton with no persistence
- The XP ledger does not exist yet (scope of sibling ticket under INC-8)
- FBPP is not installed anywhere in the project
- No PlayerPrefs usage exists anywhere in the codebase
- The run-end hook is a stub with a TODO comment

---

## Current State: What Exists

### Credits / Currency

**`_Scripts/Player/PlayerWallet.cs`**
- `PlayerWallet : SingletonMonoBehaviour<PlayerWallet>`
- `public int Money` — in-memory only, no save/load
- `public void AddMoney(int amount)` — only mutation method; no subtract, no query, no persistence

**`_Scripts/Gameplay/DollarPickup.cs`**
- Calls `PlayerWallet.Instance.AddMoney(value)` on player contact
- The only write path to credits; no run-end tally

### XP Ledger

Does not exist. Zero references to `XP`, `xp`, `experience`, or `Experience` in any `.cs` file. This is a **separate deliverable** within INC-8 that must be built before INC-25 persistence can cover it.

### Persistence Layer

None. No `PlayerPrefs`, no `File.Write`, no `JsonUtility`, no FBPP anywhere in the codebase.

---

## FBPP: Not Installed

The ticket description says "Serialize both ledgers to PlayerPrefs (FBPP)." FBPP (typically "File-Based PlayerPrefs" — a Unity Asset Store wrapper that stores PlayerPrefs as an encrypted file rather than the registry) is **not installed** and has no references anywhere in the project.

**Decision needed:** use vanilla `PlayerPrefs` or install FBPP.

- Vanilla `PlayerPrefs` on Windows stores in the registry; on WebGL stores in IndexedDB. It is fragile (users can edit it; no encryption; no easy backup).
- FBPP (or the community equivalent `EncryptedPrefs`) writes a hidden file and encrypts values. More robust for player-facing data.

For the current stage of development (no shipping concern, no cheating concern), **vanilla `PlayerPrefs` is the safe default** — it avoids a new dependency and can be swapped for FBPP later behind a thin wrapper class.

---

## Run Lifecycle (Where to Save)

### Start of run
`RunManager.StartRun()` (called from `Start()`) → `C6StatResolver.TriggerAggregation()` → `FuelManager.Initialize(context)`

### End of run (current state — stub)
`FuelManager.TriggerRunEnd()`:
```csharp
private void TriggerRunEnd()
{
    _runEnded = true;
    Debug.LogWarning("Run ended – TODO C5: trigger scene/run-end flow here");
    // Nothing else happens. No persistence, no tally, no scene transition.
}
```

**This is the hook point.** `TriggerRunEnd()` is the natural place to flush both ledgers to `PlayerPrefs` before a scene reload.

### Save-on-mutation vs save-at-run-end
Since credits are additive during a run and the run can be interrupted (game closed mid-run), there are two strategies:
1. **Save-on-mutation**: write to PlayerPrefs every `AddMoney`/`AddXP` call. Safest — no data loss on crash.
2. **Save-at-run-end**: accumulate in memory, flush at `TriggerRunEnd()`. Cheaper, but credits are lost if the app is force-closed mid-run.

Recommendation: **save-on-mutation** since `PlayerPrefs.SetInt` is cheap and this is a 2D casual game without tight per-frame budget concerns.

---

## Singleton Lifecycle Risk

`SingletonMonoBehaviour<T>` does **not** call `DontDestroyOnLoad`. This means `PlayerWallet` is destroyed when scenes reload. Since `Money` is in-memory only, all credits are lost on scene transition today.

For cross-run persistence, the save strategy (save-on-mutation to PlayerPrefs + load in `Awake`) makes scene reloads safe — money is always reloaded from PlayerPrefs on the new scene's first `Awake`. No `DontDestroyOnLoad` needed.

---

## Recommended Implementation Approach

### Step 1 — Thin persistence wrapper (optional but clean)
Create `_Scripts/Utility/PersistenceKeys.cs` with `const string` keys:
```csharp
public static class PersistenceKeys
{
    public const string Credits = "player_credits";
    public const string XP      = "player_xp";
}
```

### Step 2 — Persist credits in `PlayerWallet`
- Add `public void SubtractMoney(int amount)` (needed by skill tree later)
- Load from `PlayerPrefs.GetInt(PersistenceKeys.Credits, 0)` in `Awake`
- Write `PlayerPrefs.SetInt(PersistenceKeys.Credits, Money)` after every mutation

### Step 3 — XP ledger (prerequisite for full INC-25 scope)
Build `XPLedger : SingletonMonoBehaviour<XPLedger>` (or extend `PlayerWallet`) with:
- `public int XP`
- `public void AddXP(int amount)`
- `public void SpendXP(int amount)`
- Load/save mirroring `PlayerWallet`

**Note:** The XP ledger is technically a sibling sub-task of INC-8. INC-25 (persistence) cannot be fully validated until the XP ledger exists.

### Step 4 — Reset on new game
Add `PlayerPrefs.DeleteKey` (or `PlayerPrefs.DeleteAll`) on new-game path. This path doesn't exist yet but must be designed for.

---

## Risks and Open Questions

| Risk | Notes |
|------|-------|
| FBPP not installed | Vanilla PlayerPrefs is fine for now; suggest confirming with owner before adding dependency |
| XP ledger doesn't exist | INC-25 persistence can't be complete until XP ledger is built (sibling task in INC-8) |
| No run-end scene transition | `TriggerRunEnd()` is a stub; persistence hook should go here but the full C5 flow is blocked |
| No subtract on PlayerWallet | Skill tree spending will require it; add alongside AddMoney |
| No new-game/reset path | Needs to be designed — deleting all PlayerPrefs keys is naive if other systems write to them |

---

## Files Relevant to Implementation

| File | Relevance |
|------|-----------|
| `_Scripts/Player/PlayerWallet.cs` | Add persistence; add SubtractMoney |
| `_Scripts/Gameplay/FuelManager.cs` | `TriggerRunEnd()` is the run-end hook |
| `_Scripts/Gameplay/RunManager.cs` | `StartRun()` is the run-start hook |
| `_Scripts/Utility/Singletons.cs` | Singleton base used by PlayerWallet |
| `_Scripts/Utility/DoNotDestroyOnLoad.cs` | Available but not needed if save-on-mutation strategy is used |
| *(new)* `_Scripts/Utility/PersistenceKeys.cs` | Constants for PlayerPrefs keys |
| *(new)* `_Scripts/Economy/XPLedger.cs` | XP persistence (prerequisite) |
