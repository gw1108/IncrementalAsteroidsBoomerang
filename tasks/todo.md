# C6 Stat Resolver — Implementation Todo

## Goal
Implement the C6 stat-aggregation layer (`C6StatResolver`) and all supporting types.

## Acceptance Criteria
- Zero errors or warnings in Unity console during normal flow
- All validation rules (VAL-1 through VAL-6) enforced
- S1→S2→S3 state machine maintained
- `GetContext()` returns frozen `GameStatsContext` to consumers

## Tasks

### Phase 1: Core Types
- [x] `DeltaEnums.cs` — DeltaMode, DeltaValueType enums
- [x] `StatKeys.cs` — confirmed field key constants
- [x] `StatDelta.cs` — readonly struct with int/float constructors
- [x] `GameStatsContext.cs` — readonly struct, confirmed fields only

### Phase 2: MonoBehaviour + Stub
- [x] `UpgradeSource.cs` — abstract base class
- [x] `C6StatResolver.cs` — full aggregation, state machine, validation
- [x] `TestUpgradeSource.cs` — UNITY_EDITOR stub for worked example verification

### Verification
- [ ] Project compiles with zero errors/warnings (check Unity console)
- [ ] Manual: Add C6StatResolver + TestUpgradeSource to test scene, wire _producers[0]
- [ ] Manual: Play mode → right-click → "Print Resolved Context" → matches GDD worked example
- [ ] Manual: Second "Print Resolved Context" call logs warning, values unchanged
- [ ] Manual: Remove TestUpgradeSource from array → config error logged, context resolves to baseline

## Working Notes
- TestUpgradeSource includes the ×0.85 ThrowCooldown multiplicative delta to reproduce the GDD worked example (0.595 s). The plan's stub code omitted it; without it ThrowCooldown resolves to 0.7 s, not 0.595 s.
- The plan removes S0 (Uninitialized) and Bootstrap() vs the GDD; producers are wired via [SerializeField].
- `float.MaxValue` used as BaseDamage ceiling (OQ-C6-1 — ceiling TBD in P1b GDD).
- Range bounds sourced from GDD field inventory table.
