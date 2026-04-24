public readonly struct GameStatsContext
{
    public readonly int BaseDamage;           // baseline 1
    public readonly float ThrowCooldown;        // baseline 0.8 s
    public readonly float ArcRadius;            // baseline 5.0 wu
    public readonly float ArcFlightTime;        // baseline 0.8 s; hard floor 0.1 s
    public readonly float PierceFalloff;        // baseline 0.35
    public readonly int ChainCount;           // baseline 0; MVP max 1

    public GameStatsContext(
        int baseDamage,
        float throwCooldown,
        float arcRadius,
        float arcFlightTime,
        float pierceFalloff,
        int chainCount)
    {
        BaseDamage = baseDamage;
        ThrowCooldown = throwCooldown;
        ArcRadius = arcRadius;
        ArcFlightTime = arcFlightTime;
        PierceFalloff = pierceFalloff;
        ChainCount = chainCount;
    }
}
