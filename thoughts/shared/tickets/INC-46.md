---
ticket: INC-46
title: Drop currency on full destruction (baseNumber × yield multiplier)
status: Research Needed
priority: Medium (3)
parent: INC-14
url: https://linear.app/incremental-asteroid-boomerang/issue/INC-46/drop-currency-on-full-destruction-basenumber-yield-multiplier
---

# INC-46 — Drop currency on full destruction (baseNumber × yield multiplier)

## Description

Yield = baseNumber × GameStatsContext yield multiplier. Spawn a currency pickup prefab.

## Context

Child of INC-14 (enemy destruction system).

## Research Needed

- What is `baseNumber` — is it a field on BoomerangTarget / enemy ScriptableObject, or a constant?
- Does a `yield multiplier` stat key exist in GameStatsContext / StatKeys?
- How does DollarPickup work — does it need a value injected, or does it read from somewhere?
- Where/how should the pickup prefab be instantiated (object pool vs Instantiate)?
- What is the current status of E2Manager (was INC-42 implemented)?
