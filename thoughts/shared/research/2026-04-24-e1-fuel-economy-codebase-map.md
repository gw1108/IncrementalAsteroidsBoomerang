---
date: 2026-04-24
researcher: George (gw1108)
git_commit: ae0559fb636a59a159d41e7e3ba2e7cc067774f5
branch: main
repository: gw1108/IncrementalAsteroidsBoomerang
topic: "E1 Fuel Economy — codebase touchpoints for implementation"
tags: [research, codebase, e1, fuel-economy, c6, stat-resolver, event-router]
status: complete
last_updated: 2026-04-24
last_updated_by: George (gw1108)
---

# Research: E1 Fuel Economy — Codebase Touchpoints

**Date**: 2026-04-24
**Researcher**: George (gw1108)
**Git Commit**: ae0559fb636a59a159d41e7e3ba2e7cc067774f5
**Branch**: main
**Repository**: gw1108/IncrementalAsteroidsBoomerang

## Research Question

> "Read `design/gdd/systems-index.md`. Mainly for the Fuel Economy. Gather research of the codebase on what is needed to implement this fuel game mechanic."

This document is purely descriptive — it maps the systems E1 will plug into, the patterns those systems use, and the gaps that exist between the current codebase and what `systems-index.md` describes for E1. It does not propose an implementation.

## Summary

E1 is described in `design/gdd/systems-index.md:36-44` as a system that:
- tracks a continuous fuel value that decays each tick
- triggers the run-end flow on expiry
- reads fuel rate multipliers from `GameStatsContext`
- shows a HUD bar/gauge
- emits low-fuel warning + fuel-depleted audio cues

There is **no E1 GDD yet** (no file under `design/gdd/` whose name contains `e1`/`fuel`); the only E1 references in `design/` are inline mentions inside `systems-index.md`, `game-concept.md`, `c6-stat-resolver_V2.md`, `c6-stat-resolver.md`, `g3-boomerang-weapon.md`, and `art/art-bible.md`. Open question **OQ-C6-3** (`design/gdd/c6-stat-resolver_V2.md:258`) explicitly defers "does E1 need any `GameStatsContext` fields (e.g., fuel-per-kill modifier)?" to the still-unwritten E1 GDD.

In the codebase, the **stat plumbing E1 reads from is fully built and functional** (C6 resolver, `GameStatsContext`, `StatKeys`, `StatDelta`, `UpgradeSource`). The **systems E1 is supposed to feed into and listen to (HUD, audio, run-end/scene flow, enemy-death events, asteroid-mining events) do not exist yet** — there is no `_Scripts/UI/`, no `_Scripts/Audio/`, no `_Scripts/Scheduler/` (those folders are in `CLAUDE.md`'s allowlist but currently empty), and no run-lifecycle/scene-flow controller. The shared `EventRouter` (`_Scripts/Utility/EventRouter.cs`) is currently unused by any gameplay system — its `Broadcast<TEvent>` API is in place but no system calls it.

## Detailed Findings

### 1. What E1 specifies (from systems-index.md)

`design/gdd/systems-index.md:36-44` (E1 — Fuel Economy):

> **Coding:** Tracks a continuous fuel value that decays each tick. Expiry triggers the run-end flow. Fuel rate multipliers are read from `GameStatsContext`.
>
> **Art:** Fuel is represented by a bar or gauge on the HUD; no standalone world-space art is required.
>
> **Audio:** A low-fuel warning tone and a fuel-depleted sound cue communicate critical state without requiring a HUD read.

Other index entries reference E1 as a consumer/producer:
- `systems-index.md:96-102` — G6 Enemy System: "Enemy death broadcasts an event consumed by E1 and E2."
- `systems-index.md:126-132` — G7 Wave & Spawn Director: "Reads current fuel level and kill count to modulate pacing."
- `systems-index.md:136-142` — U1 In-Run HUD: "Displays fuel gauge, credit counter, active mod indicators, run timer, and boss health bar as UGUI elements. All data arrives through events from G1, E1, E2, and G8 — no per-frame polling. Updates must not allocate per frame."
- `systems-index.md:166-172` — A1 Audio System: "Provides an SFX pool ... per-system SFX defined in each system above (... UI feedback)."

The core-loop description at `design/gdd/game-concept.md:181-188` adds context: "A run begins with fuel starting to tick down. Player engages asteroids and enemies; mining and kills grant small fuel extensions (with diminishing returns to prevent infinite-run exploits) ... fuel running out ends the run." `Risks and Open Questions` section at `game-concept.md:326` calls out "Fuel economy tuning subtlety — 20s first run to ~3min max requires tight pacing curves with diminishing-returns on kill/mine extension."

### 2. C6 Stat Resolver — already implemented; E1's source of truth for fuel multipliers

Files (all under `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/`):
- `C6StatResolver.cs:10-241` — MonoBehaviour, holds the resolved frozen `GameStatsContext`. Auto-aggregates in `Awake()` (`C6StatResolver.cs:46-50`) — the GDD's S0/Bootstrap step is collapsed into Awake here. State machine is `Ready → RunActive → RunEnd` (`C6StatResolver.cs:12`).
- `GameStatsContext.cs:1-25` — `readonly struct` with confirmed-only fields: `BaseDamage` (int), `ThrowCooldown`, `ArcRadius`, `ArcFlightTime`, `PierceFalloff` (floats), `ChainCount` (int). **No fuel field exists today.**
- `StatKeys.cs:1-9` — `public const string` keys: `BaseDamage`, `ThrowCooldown`, `ArcRadius`, `ArcFlightTime`, `PierceFalloff`, `ChainCount`. **No fuel key today.**
- `C6StatResolver.cs:31-39` — `Specs` dictionary keyed by `StatKeys` constants holds baseline + min/max + integer-tier flag for every field the resolver knows about. Adding a new field requires (a) a `StatKeys` const, (b) a `Specs` entry, (c) a `GameStatsContext` field + constructor param, (d) a line in `BuildContext`.
- `StatDelta.cs:1-26` — `readonly struct` with int and float constructors; carries `FieldKey`, `Mode`, `Type`.
- `DeltaEnums.cs:1-2`:
  ```csharp
  public enum DeltaMode { Additive }
  public enum DeltaValueType { Int, Float }
  ```
  The enum currently contains **only `Additive`**. `C6StatResolver.cs:167-170` writes `if (delta.Mode == DeltaMode.Additive) addSums[...] += value; else mulProds[...] *= value;` — the `else` branch is unreachable given the present enum. The C6 GDD (`c6-stat-resolver_V2.md`) describes a `Multiplicative` mode and additive-first resolution; the implementation has the math wired but no enum value to dispatch it.
- `UpgradeSource.cs:1-7` — abstract `MonoBehaviour`. Producers extend it and override `StatDelta[] GetDeltas()`. Wired into `C6StatResolver._producers` via `[SerializeField]` (`C6StatResolver.cs:41`).
- `TestUpgradeSource.cs:1-13` — UNITY_EDITOR-only stub returning the worked-example deltas.

**Resolver lifecycle hooks E1 may sit on:**
- `TriggerAggregation()` (`C6StatResolver.cs:59-74`) — produces the frozen context. Currently auto-called in `Awake()`.
- `NotifyRunEnd()` (`C6StatResolver.cs:77-82`) — caller transitions resolver to `RunEnd`. **No system currently calls this** (verified by `Grep`).
- `Reset()` (`C6StatResolver.cs:85-94`) — `RunEnd → Ready`. **Also currently uncalled.**
- `GetContext()` (`C6StatResolver.cs:97-107`) — read accessor for consumers.

E1's "fuel rate multipliers are read from `GameStatsContext`" requirement therefore corresponds to the `StatKeys` + `Specs` + `GameStatsContext` extension pattern shown above. The C6 GDD's open question OQ-C6-3 is the formal owner of "what fields, exactly."

### 3. EventRouter — present but unused

`src/IncrementalAsteroidBoomerang/Assets/_Scripts/Utility/EventRouter.cs:14-67`:
- Singleton accessor `EventRouter.Instance` (`EventRouter.cs:19-23`, lazy-initialized via `_instance ??= new EventRouter()`).
- Generic API: `Register<TEvent>(Action<TEvent> callback)` (`:27-39`), `Unregister<TEvent>` (`:41-54`), `Broadcast<TEvent>(TEvent eventData)` (`:56-65`). Constraint: `where TEvent : struct`.
- Also defines `IEventRouterHolder` (`:9-12`) for objects that want to expose a localized event router.

Usage today: `Grep` for `EventRouter\.Instance` and `Broadcast<` returns no matches across the project — no system currently broadcasts or listens to `EventRouter` events. The systems-index entries that say things like "Enemy death broadcasts an event consumed by E1 and E2" (`:96-102`) and "All data arrives through events from G1, E1, E2, and G8 — no per-frame polling" (`:136-142`) describe a pattern this router's API supports, but no event types or wiring exist yet.

### 4. PlayerController — movement only, no health/death/run-end

`src/IncrementalAsteroidBoomerang/Assets/_Scripts/Player/PlayerController.cs:10-115`:
- `[RequireComponent(typeof(CircleCollider2D))]` (`:9`).
- WASD/arrow movement clamped to camera bounds (`HandleMovement`, `:53-71`; `ClampToCameraBounds`, `:77-92`).
- `SetupInputActions` builds Unity Input System composite bindings inline (`:97-114`) — no `.inputactions` asset is wired through this path; the InputSystem actions asset exists at `Assets/InputSystem_Actions.inputactions` (per `CLAUDE.md`) but `PlayerController` allocates its own action.
- **No HP, no damage intake, no death.** Despite `systems-index.md:26-32` (G1) describing "damage intake for the player ship ... death triggers the run-end flow," the controller does not implement health or call any run-end signal.

There is therefore no place that currently signals "the run is over." E1's "expiry triggers the run-end flow" depends on a run-end signal that no system both produces and consumes today.

### 5. Other gameplay scripts — no event broadcasts, no kill/mine signals

- `BoomerangTarget.cs:1-19`:
  - Static `List<BoomerangTarget> All` registry (`:6`), maintained by `OnEnable`/`OnDisable` (`:10-11`).
  - `TakeDamage(int, Vector2)` (`:13-18`) sets `IsAlive = false` and `gameObject.SetActive(false)`. **No event is broadcast on death** — relevant to G6's promised "enemy death broadcasts an event consumed by E1 and E2."
- `BoomerangProjectile.cs` and `BoomerangController.cs` — exist (per Glob), implement the G3 boomerang per `tasks/g3-boomerang-plan.md`. Not directly part of E1.
- `AsteroidSpawner.cs:1-102` — wave-based spawner with internal coroutine (`SpawnLoop`, `:45-55`); no fuel awareness, no events broadcast.
- `AsteroidMover.cs:1-35` — straight-line motion + offscreen self-destruct (`:30-34`); no mining/destruction signal toward E1.

### 6. UI / Audio / Scheduler — no scripts present

- `Glob` for `_Scripts/UI/**/*.cs`, `_Scripts/Audio/**/*.cs`, `_Scripts/Scheduler/**/*.cs` returns **no files** despite those folder names being listed as canonical destinations in `CLAUDE.md` ("Project Structure" section).
- The U1 HUD described in `systems-index.md:136-142` (UGUI fuel gauge driven by events, no per-frame allocation) is therefore unimplemented; E1 has no current display target.
- The A1 audio system described in `systems-index.md:166-172` (SFX pool, music manager, WebGL AudioContext unlock) is also unimplemented; E1's "low-fuel warning tone" and "fuel-depleted sound cue" have no current audio service to call.
- `tasks/lessons.md` — referenced by `CLAUDE.md` as the place to record lessons learned; **does not exist** at the time of this research.

### 7. Singletons utility — alternative to EventRouter for service handles

`src/IncrementalAsteroidBoomerang/Assets/_Scripts/Utility/Singletons.cs`:
- `Singleton<T>` plain-C# (`:6-43`) and `SingletonMonoBehaviour<T>` (`:44-104`) generic bases.
- `SingletonMonoBehaviour<T>` does lazy `FindFirstObjectByType<T>()` and creates a new GameObject if absent (`:62-69`); standard duplicate-destroy guard in `Awake` (`:79-90`).
- Inherits from `SerializedMonoBehaviour` (Odin Inspector) — Odin is in the project (`using Sirenix.OdinInspector`).

This is an option the codebase already exposes for a service-style component (e.g., a future audio manager, fuel manager, or run-lifecycle controller), distinct from the typed-event `EventRouter` pattern.

### 8. Helpers — UI/physics utilities

`src/IncrementalAsteroidBoomerang/Assets/_Scripts/Utility/Helpers.cs:1-123`:
- `UIHelpers.IsMouseOverClickableUI` (`:15-39`), `IsMouseOverBlockingUI` (`:47-67`), `GetItemUnderMouse<T>` (`:73-100`).
- `PhysicsHelpers.SetLayer` (`:103-122`).

No fuel-relevant helpers; included for completeness so future HUD work knows what's already in `Utility/`.

### 9. Scene composition

`Glob` results:
- One scene: `Assets/Scenes/Game.unity` (only `.unity` file in the project).
- Prefabs present: `Player.prefab`, `Boomerang.prefab`, `EnemyBox.prefab`, `Asteroid.prefab` (`Assets/Prefabs/`).

There is a single play scene; no menu, run-end, or shop scene. The C5 "Scene & Mode Flow" hard-dependency referenced in `c6-stat-resolver_V2.md:215` ("C5 calls `Bootstrap()`, `TriggerAggregation()`, `NotifyRunEnd()`") has no corresponding script in the repo.

### 10. State of `tasks/` and `thoughts/`

- `tasks/todo.md` — currently scoped to C6 stat resolver implementation (Phase 1 + Phase 2 marked done; verification checkboxes unchecked).
- `tasks/g3-boomerang-plan.md` — boomerang-weapon plan; the matching code (`BoomerangController.cs`, `BoomerangProjectile.cs`, `BoomerangTarget.cs`) is on disk per Glob.
- `thoughts/shared/research/session-2026-04-23-video-to-gdd-research.md` — pre-existing research doc.
- `thoughts/shared/plans/2026-04-24-c6-stat-resolver.md` — pre-existing C6 plan.
- No fuel-related plan or research exists prior to this document.

## Code References

- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/C6StatResolver.cs:31-39` — `Specs` table; the place where a new fuel field's baseline/min/max/integer-tier metadata is registered.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/C6StatResolver.cs:46-50` — `Awake` auto-calls `TriggerAggregation()`.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/C6StatResolver.cs:77-82` — `NotifyRunEnd()` accessor, currently unused.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/GameStatsContext.cs:1-25` — frozen-context struct, fields enumerated.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/StatKeys.cs:1-9` — string keys, additive list.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/DeltaEnums.cs:1-2` — `DeltaMode` currently has only `Additive`.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Utility/EventRouter.cs:14-67` — typed pub/sub singleton, currently unused by gameplay.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Utility/Singletons.cs:6-104` — `Singleton<T>` and `SingletonMonoBehaviour<T>` patterns available.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Player/PlayerController.cs:10-115` — movement-only player; no HP/damage/death.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/BoomerangTarget.cs:13-18` — `TakeDamage` deactivates the GameObject without broadcasting.
- `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/AsteroidSpawner.cs:45-55` — coroutine `SpawnLoop`; no fuel-coupling.
- `design/gdd/systems-index.md:36-44` — E1 specification.
- `design/gdd/c6-stat-resolver_V2.md:258` — OQ-C6-3, the open design question on whether E1 needs `GameStatsContext` fields.
- `design/gdd/game-concept.md:181-188` — core-loop description with fuel mechanics.

## Architecture Documentation

Patterns the codebase already uses, in approximate order of E1 relevance:

1. **MonoBehaviour service with `[SerializeField]` wiring** — `C6StatResolver` is the canonical example. Producers inject by inspector reference, not via `FindObjectOfType` or singletons.
2. **`readonly struct` immutable snapshot** — `GameStatsContext` and `StatDelta` are both `readonly struct`. This is the chosen shape for "frozen value object passed across system boundaries."
3. **`StatKeys` + `Specs` + `GameStatsContext` triple** — three coordinated edits add a new resolver-managed field; the data-driven extension point for any system that wants its tuning surface routed through C6.
4. **Static instance registry** — `BoomerangTarget.All` (`BoomerangTarget.cs:6`) is the `OnEnable`/`OnDisable` add/remove registry. Useful precedent if E1 needs to enumerate live entities (it does not, per the systems-index spec).
5. **Typed pub/sub via `EventRouter`** — `Register<T>` / `Broadcast<T>` of struct event types. Unused in gameplay code today; would be the natural carrier for the systems-index lines about "broadcasts an event consumed by E1 and E2."
6. **Singleton MonoBehaviour with lazy auto-creation** — `SingletonMonoBehaviour<T>` (Odin-derived) for service-style components.
7. **Update()-driven tick** — per project `CLAUDE.md` and `systems-index.md:10`: "All gameplay systems use Unity's built-in `Update()` function directly — no custom fixed-timestep abstraction." Examples: `PlayerController.Update`, `AsteroidMover.Update`. The boomerang projectile uses `FixedUpdate` for kinematic motion (`BoomerangProjectile.FixedUpdate` per the plan), but that is the Bezier-motion exception, not the tick abstraction.
8. **Coroutine-driven spawner loop** — `AsteroidSpawner.SpawnLoop` is the local precedent for time-based long-running loops; not necessarily relevant to a continuously-decaying fuel value, but worth noting as an existing pattern.

## Historical Context (from thoughts/)

- `thoughts/shared/research/session-2026-04-23-video-to-gdd-research.md` — pre-existing research note (not E1-specific).
- `thoughts/shared/plans/2026-04-24-c6-stat-resolver.md` — the plan that produced the C6 implementation read above; predates this E1 research and informs the `StatKeys`/`Specs`/`GameStatsContext` triple pattern.

No prior memory or thought-doc references E1 / Fuel Economy.

## Related Research

- `thoughts/shared/research/session-2026-04-23-video-to-gdd-research.md`
- `thoughts/shared/plans/2026-04-24-c6-stat-resolver.md`
- Memory entry: `C:\Users\George\.claude\projects\C--GameDev-IncrementalAsteroidsBoomerang\memory\project_c6_complete.md` (referenced in `MEMORY.md`).

## Open Questions (carried forward, not raised by this research)

- **OQ-C6-3** (`design/gdd/c6-stat-resolver_V2.md:258`) — does E1 need any `GameStatsContext` fields (e.g., fuel-per-kill modifier, fuel-decay-rate multiplier)? Owner: future E1 GDD.
- The systems-index entries for G6 and U1 describe events ("enemy death broadcasts an event," "data arrives through events from G1, E1, E2, and G8") whose concrete event-struct types are undefined in code.
- C5 Scene & Mode Flow — referenced as the caller of `Bootstrap()` / `TriggerAggregation()` / `NotifyRunEnd()` — has no GDD listed in the systems-index and no script in the repo.
