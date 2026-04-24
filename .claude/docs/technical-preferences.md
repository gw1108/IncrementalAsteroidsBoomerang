# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 6.3 LTS (supported through December 2027)
- **Language**: C# (.NET — Mono + IL2CPP for WebGL builds)
- **Rendering**: URP 2D Renderer — feature-stripped for WebGL per TD-FEASIBILITY (no shadows, no SSAO, minimal renderer features, no post-processing stack unless required)
- **Physics**: Physics 2D (Box2D). Physics 3D stripped from build via Player Settings. Boomerang motion is **kinematic scripted** (not Rigidbody2D) per pillar P3 (Read the Arc)

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: Web (WebGL primary, desktop browsers only); PC (Steam native, post-MVP scope tier)
- **Input Methods**: Keyboard/Mouse, Gamepad
- **Primary Input**: Keyboard/Mouse (WebGL default; gamepad is co-supported)
- **Gamepad Support**: Partial on WebGL (browser Gamepad API); Full on Steam port
- **Touch Support**: None — mobile explicitly scoped out per `design/gdd/game-concept.md`
- **Platform Notes**: WebGL-first distribution. AudioContext must be user-input-unlocked (Unity WebGL quirk). Persistent save via IndexedDB + clipboard export/import (Safari ITP mitigation). Cold-load target <30s on mid-range hardware. Peak heap <512 MB for MVP as safety margin against 32-bit Safari. Brotli compression required on host.

## Naming Conventions

- **Classes**: PascalCase (e.g., `PlayerController`, `BoomerangController`)
- **Public fields/properties**: PascalCase (e.g., `MoveSpeed`, `DamageTier`)
- **Private fields**: `_camelCase` (e.g., `_currentFuel`, `_isReturning`)
- **Methods**: PascalCase (e.g., `ThrowBoomerang()`, `TakeDamage()`)
- **Signals/Events**: C# events in PascalCase with past-tense verb (e.g., `HealthChanged`, `BoomerangCaught`); `UnityEvent` where Inspector-exposure is required
- **Files**: PascalCase matching class (e.g., `BoomerangController.cs`)
- **Scenes/Prefabs**: PascalCase matching purpose (e.g., `MainMenu.unity`, `PlayerShip.prefab`)
- **Constants**: PascalCase (C# convention) — e.g., `MaxFuel`, `BaseThrowCooldown`

## Performance Budgets

- **Target Framerate**: 60 fps stable (144 fps uncapped on high-refresh displays via fixed-timestep decoupling)
- **Frame Budget**: 16.6 ms per frame
- **Draw Calls**: <100 typical, <200 peak during heavy encounters
- **Memory Ceiling**: <512 MB peak heap (Unity 6 WebGL has 2 GB theoretical; 1 GB floor on some 32-bit Safari builds — 512 MB target is safety margin)
- **GC Pauses**: <10 ms during a 6-enemy pierce event (TD-FEASIBILITY success criterion); enforce via object pooling from day 1

## Testing

- **Framework**: Unity Test Framework (NUnit-based) — PlayMode tests for integration, EditMode tests for pure logic units
- **Minimum Coverage**: 70% line coverage for Logic-type stories (formulas, state machines, economy math); Integration-type stories require either integration tests OR documented playtests per `.claude/docs/coding-standards.md`
- **Required Tests**: Balance formulas (fuel economy, damage scaling, tree node effects), boomerang trajectory math, save/load round-trip integrity, skill tree prereq validation

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- **`Resources.Load` / `Resources.LoadAsync`** in hot paths — use Addressables
- **`FindObjectsOfType` / `GameObject.Find`** in `Update`, `FixedUpdate`, or event callbacks — cache references
- **String allocation in hot paths** (`$"{damage}"`, `ToString()` on int inside Update) — use pre-allocated string pool for damage numbers / UI
- **LINQ in hot paths** (`.Where`, `.Select`, `.ToList` inside Update) — manual loops instead
- **`foreach` over `List<T>` in hot paths** if IL2CPP WebGL profiling shows allocation (verify case-by-case)
- **Rigidbody2D for boomerang** — boomerang must be kinematic scripted motion per pillar P3
- **UI Toolkit for the skill tree** — use UGUI with a custom mesh-based connector `Graphic` per TD-FEASIBILITY mandate
- **PlayerPrefs for shipping saves** — prototyping only; ship uses `ISaveStore` JSON-blob-in-IndexedDB
- **Per-run temporary power upgrades invisible to the tree** — violates anti-pillar AP2

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here. Only add when actively integrating — no speculative entries. -->
- [None configured yet — Addressables, New Input System, and Unity Test Framework will be added here when `/architecture-decision` formalizes them]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one. TD-FEASIBILITY identified 9 Required ADRs: object pooling, save-layer abstraction, kinematic boomerang motion, Addressables strategy, URP feature stripping, UGUI skill tree architecture, fixed-timestep tick, string-allocation discipline, hot-path prohibitions]

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (C# review — primary covers it)
- **Shader Specialist**: unity-shader-specialist (Shader Graph, HLSL, URP 2D shaders, sprite shader variants)
- **UI Specialist**: unity-ui-specialist (UGUI for MVP per TD mandate; UI Toolkit only if future ADR overrides)
- **Additional Specialists**: unity-addressables-specialist (asset management, memory, content catalogs — TD mandate)
- **Routing Notes**: Invoke primary for architecture and general C# code review. Invoke shader specialist for rendering, sprite shaders, and visual effects. Invoke UI specialist for all interface implementation including the skill-tree hex-grid. Invoke Addressables specialist for asset management systems and memory budgeting. **unity-dots-specialist is NOT applicable** — this project does not use DOTS/ECS at any tier (MonoBehaviour architecture only).

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->
<!-- If a row says [TO BE CONFIGURED], fall back to Primary for that file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (.cs files) | unity-specialist |
| Shader / material files (.shader, .shadergraph, .mat) | unity-shader-specialist |
| UI / screen files (.uxml, .uss, Canvas prefabs, skill-tree hex grid) | unity-ui-specialist |
| Scene / prefab / level files (.unity, .prefab) | unity-specialist |
| Addressables groups / content catalogs | unity-addressables-specialist |
| Native extension / plugin files (.dll, native plugins) | unity-specialist |
| General architecture review | unity-specialist |
