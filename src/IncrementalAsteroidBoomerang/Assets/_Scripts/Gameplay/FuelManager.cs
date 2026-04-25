using _Scripts.Utility;
using UnityEngine;

/// <summary>
/// Tracks run fuel. Initialised by RunManager at run-start — does not self-initialise.
/// All tuning values come from GameStatsContext via C6StatResolver; no inspector-settable
/// tuning here. All player-facing stats live in C6 so P1a/P1b have a single authoritative
/// place to define upgrade effects.
/// </summary>
public class FuelManager : SingletonMonoBehaviour<FuelManager>
{
    private GameStatsContext _context;
    private float _fuel;
    private int _killCount;
    private bool _hasWarnedLowFuel;
    private bool _runEnded;
    private bool _initialized;

    private const float LowFuelThreshold = 3f;

    /// <summary>
    /// Called by RunManager at run-start. Resets all fuel state from the resolved context.
    /// </summary>
    public void Initialize(GameStatsContext context)
    {
        _context = context;
        _fuel = context.FuelStartingAmount;
        _killCount = 0;
        _hasWarnedLowFuel = false;
        _runEnded = false;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized || _runEnded) return;

        _fuel -= _context.FuelDecayRate * Time.deltaTime;
        CheckThresholds();
    }

    private void CheckThresholds()
    {
        if (!_hasWarnedLowFuel && _fuel <= LowFuelThreshold)
        {
            _hasWarnedLowFuel = true;
            // TODO A1: play low-fuel warning sound here
        }

        if (_fuel <= 0f)
            TriggerRunEnd();
    }

    /// <summary>
    /// Called directly by BoomerangTarget on death.
    /// Extension is only applied when FuelPerKill > 0 (i.e., a skill node has upgraded it).
    /// At baseline (FuelPerKill = 0) kills give 0s; FuelMinExtension is only the floor
    /// once FuelPerKill is above zero.
    /// </summary>
    public void OnTargetKilled()
    {
        if (!_initialized || _runEnded) return;
        // TODO (GitHub issue): C6 context is permanently frozen at run-start, so _killCount
        // increments even when FuelPerKill == 0, accumulating diminishing-return depth before
        // a skill node is ever unlocked. Simplify C6 to support mid-run context refreshes so
        // _killCount only needs to track fuel-granting kills.
        if (_context.FuelPerKill > 0f)
        {
            float extension = _context.FuelPerKill
                * Mathf.Pow(_context.FuelDiminishingReturnsFactor, _killCount);
            _fuel += Mathf.Max(_context.FuelMinExtension, extension);
        }
        _killCount++;
    }

    private void TriggerRunEnd()
    {
        _runEnded = true;
        // TODO U1 (#1): update HUD fuel bar to empty here
        // TODO C5: trigger scene/run-end flow here
        Debug.LogWarning("[FuelManager] Run ended: fuel depleted.");
    }
}
