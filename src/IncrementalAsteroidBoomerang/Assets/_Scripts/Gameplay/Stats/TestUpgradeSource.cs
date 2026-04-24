#if UNITY_EDITOR
// Simulates combined P1a + G4 deltas from the GDD worked example (c6-stat-resolver_V2.md).
// Expected results: BaseDamage=2, ArcRadius=6.0, ThrowCooldown=0.595, ArcFlightTime=0.8
public class TestUpgradeSource : UpgradeSource
{
    public override StatDelta[] GetDeltas() => new[]
    {
        new StatDelta(StatKeys.BaseDamage,    DeltaMode.Additive,       1),
        new StatDelta(StatKeys.ArcRadius,     DeltaMode.Additive,       1.0f),
        new StatDelta(StatKeys.ThrowCooldown, DeltaMode.Additive,       -0.1f),
        new StatDelta(StatKeys.ThrowCooldown, DeltaMode.Multiplicative, 0.85f), // G4 mod: ×0.85
    };
}
#endif
