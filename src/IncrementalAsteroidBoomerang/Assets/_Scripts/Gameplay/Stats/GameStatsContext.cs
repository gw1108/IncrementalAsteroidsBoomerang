public readonly struct GameStatsContext
{
    public readonly int BaseDamage;           // baseline 1
    public readonly float ThrowCooldown;        // baseline 0.8 s
    public readonly float ArcRadius;            // baseline 5.0 wu
    public readonly float ArcFlightTime;        // baseline 0.8 s; hard floor 0.1 s
    public readonly float PierceFalloff;        // baseline 0.35
    public readonly int ChainCount;           // baseline 0; MVP max 1

    // E1 — Fuel Economy
    public readonly float FuelStartingAmount;           // baseline 20 s
    public readonly float FuelDecayRate;                // baseline 1.0 s/s
    public readonly float FuelPerKill;                  // baseline 0 s (no extension until skill node)
    public readonly float FuelDiminishingReturnsFactor; // baseline 0.8
    public readonly float FuelMinExtension;             // baseline 0.1 s; floor only when FuelPerKill > 0

    public GameStatsContext(
        int baseDamage,
        float throwCooldown,
        float arcRadius,
        float arcFlightTime,
        float pierceFalloff,
        int chainCount,
        float fuelStartingAmount,
        float fuelDecayRate,
        float fuelPerKill,
        float fuelDiminishingReturnsFactor,
        float fuelMinExtension)
    {
        BaseDamage = baseDamage;
        ThrowCooldown = throwCooldown;
        ArcRadius = arcRadius;
        ArcFlightTime = arcFlightTime;
        PierceFalloff = pierceFalloff;
        ChainCount = chainCount;
        FuelStartingAmount = fuelStartingAmount;
        FuelDecayRate = fuelDecayRate;
        FuelPerKill = fuelPerKill;
        FuelDiminishingReturnsFactor = fuelDiminishingReturnsFactor;
        FuelMinExtension = fuelMinExtension;
    }
}
