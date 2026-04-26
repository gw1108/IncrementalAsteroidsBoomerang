---
ticket: INC-42
title: Broadcast enemy death event for E2 (currency/XP drop)
status: plan in progress
priority: Medium
parent: INC-13
url: https://linear.app/incremental-asteroid-boomerang/issue/INC-42/broadcast-enemy-death-event-for-e2-currencyxp-drop
created: 2026-04-25
---

# INC-42 — Broadcast enemy death event for E2 (currency/XP drop)

## Description

Death includes enemy type and world position. On enemy death, call the appropriate E2 manager method directly (direct singleton call pattern, matching FuelManager.OnTargetKilled). Consumed by E2 to deposit currency and XP.

## Clarifications (from comments)

- **Direct call confirmed**: Implementation must use a direct `E2Manager.Instance.OnEnemyDied(position, "Asteroid")` call added to `BoomerangTarget.TakeDamage()`, identical to how `FuelManager.OnTargetKilled()` is wired. `EventRouter` is NOT to be used.
- **XP drop deferred**: `XPLedger` does not exist yet (blocked by INC-25). INC-42 is scoped to currency wiring only.

## Linked Documents

- Research: `thoughts/shared/research/2026-04-25-INC-42-enemy-death-event-e2.md`
- Related: INC-46 (Drop currency on full destruction — implements E2Manager business logic)
- Related: INC-25 (Persist credits/XP — needed before XP drop can be wired)
