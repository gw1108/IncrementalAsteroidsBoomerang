# Review Log: C6 Stat Resolver & Upgrade Aggregation

---

## Review — 2026-04-24 — Verdict: NEEDS REVISION (resolved in-session)

Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, creative-director
Blocking items: 6 | Recommended: 8
Summary: The core architecture (pull model, immutable context, IUpgradeSource contract, additive-first aggregation) is sound. Blocking items were: delta value type unspecified (discriminated union selected), string key allocation risk (StatKeys const class mandated), consumer access pattern unspecified (Inspector-serialized [SerializeField] selected), VAL rules guarding only technical floors not pillar floors (VAL-8 advisory warning added for P3-unsafe band), OQ-C6-7 injection method (resolved: Bootstrap(IUpgradeSource[]) method), and 3 missing ACs (AC-13/14/15 added). All 6 blocking items addressed in-session. CD verdict: "C6 delivers the mechanism but not the guarantee — VAL rules were technical floors, not pillar-aware floors." Fixed with VAL-8.
Prior verdict resolved: No — first review
