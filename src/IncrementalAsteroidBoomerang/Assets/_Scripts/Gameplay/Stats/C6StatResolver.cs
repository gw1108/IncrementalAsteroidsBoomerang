using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Aggregates stat deltas from registered producers into a frozen GameStatsContext at run-start.
/// State machine: Ready (S1) → RunActive (S2) → RunEnd (S3). Reset() returns S3 → S1.
/// Wire producers into _producers via the inspector; call TriggerAggregation() to resolve.
/// </summary>
public class C6StatResolver : MonoBehaviour
{
    private enum ResolverState { Ready, RunActive, RunEnd }

    private readonly struct FieldSpec
    {
        public readonly float Baseline;
        public readonly float Min;
        public readonly float Max;
        public readonly bool IntegerTier; // true for base_damage, chain_count — additive-only

        public FieldSpec(float baseline, float min, float max, bool integerTier)
        {
            Baseline = baseline;
            Min = min;
            Max = max;
            IntegerTier = integerTier;
        }
    }

    // Ranges sourced from GDD field inventory table
    private static readonly Dictionary<string, FieldSpec> Specs = new Dictionary<string, FieldSpec>
    {
        { StatKeys.BaseDamage,           new FieldSpec(1f,    1f,   float.MaxValue, true)  },
        { StatKeys.ThrowCooldown,        new FieldSpec(0.8f,  0.1f, 10.0f,          false) },
        { StatKeys.ArcRadius,            new FieldSpec(5.0f,  0.0f, 20.0f,          false) },
        { StatKeys.ArcFlightTime,        new FieldSpec(0.8f,  0.1f, 10.0f,          false) },
        { StatKeys.PierceFalloff,        new FieldSpec(0.35f, 0.0f, 1.0f,           false) },
        { StatKeys.ChainCount,           new FieldSpec(0f,    0f,   1f,             true)  },
    };

    [SerializeField] private UpgradeSource[] _producers; // ordered: P1a first, G4 second

    private ResolverState _state = ResolverState.Ready;
    private GameStatsContext _context;

    private void Awake()
    {
        _state = ResolverState.Ready;
        TriggerAggregation();
    }

    private void OnDestroy()
    {
        _context = default;
        _state = ResolverState.Ready;
    }

    /// <summary>Resolves all producer deltas into a frozen GameStatsContext. Valid only in S1 (Ready).</summary>
    public void TriggerAggregation()
    {
        if (_state == ResolverState.RunActive)
        {
            Debug.LogWarning("[C6StatResolver] TriggerAggregation called while already RunActive; ignoring.", this);
            return;
        }
        if (_state == ResolverState.RunEnd)
        {
            Debug.LogError("[C6StatResolver] TriggerAggregation called in RunEnd state; call Reset() first.", this);
            return;
        }

        _context = Aggregate();
        _state = ResolverState.RunActive;
    }

    /// <summary>Marks the current run as ended. Valid from S2; also accepted from S1 with a warning.</summary>
    public void NotifyRunEnd()
    {
        if (_state == ResolverState.Ready)
            Debug.LogWarning("[C6StatResolver] NotifyRunEnd called in Ready state; transitioning to RunEnd anyway.", this);
        _state = ResolverState.RunEnd;
    }

    /// <summary>Clears the resolved context and returns to Ready. Valid only in S3 (RunEnd).</summary>
    public void Reset()
    {
        if (_state != ResolverState.RunEnd)
        {
            Debug.LogError("[C6StatResolver] Reset() called outside RunEnd state; no-op.", this);
            return;
        }
        _context = default;
        _state = ResolverState.Ready;
    }

    /// <summary>Returns the frozen context. Logs a warning if called before aggregation or after run-end.</summary>
    public GameStatsContext GetContext()
    {
        if (_state == ResolverState.Ready)
        {
            Debug.LogWarning("[C6StatResolver] GetContext called in Ready state; returning default context.", this);
            return default;
        }
        if (_state == ResolverState.RunEnd)
            Debug.LogWarning("[C6StatResolver] GetContext called in RunEnd state; returning last resolved context.", this);
        return _context;
    }

    private GameStatsContext Aggregate()
    {
        var addSums = new Dictionary<string, float>();
        var mulProds = new Dictionary<string, float>();
        foreach (var key in Specs.Keys)
        {
            addSums[key] = 0f;
            mulProds[key] = 1f;
        }

        if (_producers != null)
        {
            foreach (var producer in _producers)
            {
                if (producer == null)
                {
                    Debug.LogError("[C6StatResolver] Null entry in _producers array; skipping.", this);
                    continue;
                }

                StatDelta[] deltas;
                try { deltas = producer.GetDeltas(); }
                catch (Exception ex)
                {
                    Debug.LogError($"[C6StatResolver] Producer '{producer.name}' threw in GetDeltas(): {ex.Message}; skipping.", this);
                    continue;
                }

                if (deltas == null) continue;

                var seen = new HashSet<(string, DeltaMode)>();

                foreach (var delta in deltas)
                {
                    if (string.IsNullOrEmpty(delta.FieldKey))
                    {
                        Debug.LogError($"[C6StatResolver] Delta with null/empty FieldKey from '{producer.name}'; discarding.", this);
                        continue;
                    }

                    if (!Specs.TryGetValue(delta.FieldKey, out var spec))
                    {
                        Debug.LogWarning($"[C6StatResolver] Unknown FieldKey '{delta.FieldKey}' from '{producer.name}'; skipping.", this);
                        continue;
                    }

                    // VAL-3: integer-tier fields accept only Int-typed deltas
                    if (spec.IntegerTier && delta.Type != DeltaValueType.Int)
                    {
                        Debug.LogError($"[C6StatResolver] VAL-3: Non-integer delta on integer-tier field '{delta.FieldKey}' from '{producer.name}'; rejecting.", this);
                        continue;
                    }

                    if (!seen.Add((delta.FieldKey, delta.Mode)))
                        Debug.LogWarning($"[C6StatResolver] Duplicate ({delta.FieldKey}, {delta.Mode}) from '{producer.name}'; both applied.", this);

                    float value = delta.Type == DeltaValueType.Float ? delta.FloatValue : (float)delta.IntValue;

                    if (delta.Mode == DeltaMode.Additive)
                        addSums[delta.FieldKey] += value;
                    else
                        mulProds[delta.FieldKey] *= value;
                }
            }
        }

        return BuildContext(addSums, mulProds);
    }

    private GameStatsContext BuildContext(Dictionary<string, float> addSums, Dictionary<string, float> mulProds)
    {
        float ResolveFloat(string key)
        {
            var spec = Specs[key];
            float raw = (spec.Baseline + addSums[key]) * mulProds[key];

            // VAL-1: arc_flight_time hard floor — runs before all other clamps (G3 Bezier divides by this)
            if (key == StatKeys.ArcFlightTime && raw < 0.1f)
            {
                Debug.LogWarning($"[C6StatResolver] VAL-1: ArcFlightTime {raw:F3}s clamped to hard floor 0.1s.", this);
                raw = 0.1f;
            }

            // VAL-2: general float clamp
            if (raw < spec.Min || raw > spec.Max)
            {
                Debug.LogWarning($"[C6StatResolver] VAL-2: '{key}' value {raw} outside [{spec.Min}, {spec.Max}]; clamping.", this);
                raw = Mathf.Clamp(raw, spec.Min, spec.Max);
            }

            return raw;
        }

        // Integer-tier fields: PROD_mul = 1.0 always; sum additive deltas only
        int baseDamage = Mathf.RoundToInt(Specs[StatKeys.BaseDamage].Baseline + addSums[StatKeys.BaseDamage]);
        if (baseDamage < 1) baseDamage = 1; // VAL-4

        int chainCount = Mathf.RoundToInt(Specs[StatKeys.ChainCount].Baseline + addSums[StatKeys.ChainCount]);
        if (chainCount > 1)
        {
            Debug.LogWarning($"[C6StatResolver] VAL-5: ChainCount {chainCount} clamped to MVP ceiling of 1.", this);
            chainCount = 1;
        }
        if (chainCount < 0) chainCount = 0;

        return new GameStatsContext(
            baseDamage: baseDamage,
            throwCooldown: ResolveFloat(StatKeys.ThrowCooldown),
            arcRadius: ResolveFloat(StatKeys.ArcRadius),
            arcFlightTime: ResolveFloat(StatKeys.ArcFlightTime),
            pierceFalloff: ResolveFloat(StatKeys.PierceFalloff),
            chainCount: chainCount
        );
    }

#if UNITY_EDITOR
    [ContextMenu("Print Resolved Context")]
    private void PrintResolvedContext()
    {
        TriggerAggregation();
        var ctx = GetContext();
        Debug.Log(
            $"[C6StatResolver] Resolved Context:\n" +
            $"  BaseDamage:           {ctx.BaseDamage}\n" +
            $"  ThrowCooldown:        {ctx.ThrowCooldown:F3} s\n" +
            $"  ArcRadius:            {ctx.ArcRadius:F3} wu\n" +
            $"  ArcFlightTime:        {ctx.ArcFlightTime:F3} s\n" +
            $"  PierceFalloff:        {ctx.PierceFalloff:F3}\n" +
            $"  ChainCount:           {ctx.ChainCount}\n" +
            this);
    }
#endif
}
