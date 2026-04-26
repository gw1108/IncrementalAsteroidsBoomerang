# INC-25 — Persist credits and XP across runs

**Status:** research in review → In Progress  
**Priority:** Medium (3)  
**Parent:** INC-8 — E2 Currency & XP Economy  
**URL:** https://linear.app/incremental-asteroid-boomerang/issue/INC-25/persist-credits-and-xp-across-runs

## Description

Serialize both ledgers to PlayerPrefs (FBPP). Both values must survive run boundaries.

## Research Attachment

`thoughts/shared/research/2026-04-25-INC-25-persist-credits-xp-across-runs.md`

## Key Findings (from research)

- `PlayerWallet` holds credits in-memory only (`public int Money`) — no persistence
- XP ledger (`XPLedger`) does not exist yet — must be created as part of this ticket
- FBPP is NOT installed; vanilla `PlayerPrefs` is the safe default for now
- No `PlayerPrefs` usage anywhere in the codebase
- `FuelManager.TriggerRunEnd()` is a stub — natural hook for run-end flush
- `SingletonMonoBehaviour<T>` does NOT call `DontDestroyOnLoad` — save-on-mutation strategy makes this safe

## Recommended Approach

1. Create `PersistenceKeys.cs` (const string keys for PlayerPrefs)
2. Persist credits in `PlayerWallet` — load in `Awake`, save on every mutation; add `SubtractMoney`
3. Build `XPLedger` singleton with `AddXP`/`SpendXP` and same save/load pattern
4. New-game reset path: `PlayerPrefs.DeleteKey` calls (path doesn't exist yet — design for it)

## Files

| File | Action |
|------|--------|
| `_Scripts/Player/PlayerWallet.cs` | Add load/save + SubtractMoney |
| `_Scripts/Gameplay/FuelManager.cs` | `TriggerRunEnd()` hook reference |
| `_Scripts/Gameplay/RunManager.cs` | `StartRun()` hook reference |
| `_Scripts/Utility/PersistenceKeys.cs` | NEW — const string keys |
| `_Scripts/Economy/XPLedger.cs` | NEW — XP singleton with persistence |
