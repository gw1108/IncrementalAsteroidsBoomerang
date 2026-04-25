# G3 Boomerang Weapon — Implementation Plan

## Goal
Implement the primary boomerang weapon as specified in `design/gdd/g3-boomerang-weapon-V2.md`.

## Acceptance Criteria
- Boomerang auto-fires when cooldown ≤ 0 and a valid target exists
- Arc sweeps right on outbound, left on inbound (quadratic Bezier, F1/F2)
- Inbound leg homes on live player position
- Catch fires within `catchRadius`; cooldown seeds; projectile destroyed
- Pierce: boomerang passes through all targets; damage follows F3
- No NullRefException if target dies mid-flight
- Zero errors/warnings in Unity console during normal flow

## Out of Scope
- `ChainBoomerangProjectile` — file a GitHub issue, implement later
- Enemy AI / movement
- VFX, audio, UI

---

## Phase 1 — Foundation

### 1a. Auto-trigger C6StatResolver

**File:** `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/Stats/C6StatResolver.cs`

Add `TriggerAggregation()` call to `Awake()` so consumers can call `GetContext()` without manual wiring:

```csharp
private void Awake()
{
    _state = ResolverState.Ready;
    TriggerAggregation();
}
```

### 1b. Create `BoomerangTarget.cs`

**File:** `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/BoomerangTarget.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

public class BoomerangTarget : MonoBehaviour
{
    public static readonly List<BoomerangTarget> All = new List<BoomerangTarget>();

    public bool IsAlive { get; private set; } = true;

    private void OnEnable()  => All.Add(this);
    private void OnDisable() => All.Remove(this);

    /// <summary>Called by BoomerangController on contact. Override or extend for HP system later.</summary>
    public void TakeDamage(int damage, Vector2 contactPoint)
    {
        if (!IsAlive) return;
        IsAlive = false;
        gameObject.SetActive(false);
    }
}
```

### 1c. Create enemy prefab in Unity (via MCP)

- Create GameObject "Enemy" with:
  - `SpriteRenderer` using `EnemyBox.png`
  - `CircleCollider2D` (IsTrigger = false, for layer targeting)
  - `BoomerangTarget` component
  - Layer: "Enemy" (create if needed — boomerang trigger will filter on this layer)
- Save as `Assets/Prefabs/Enemy.prefab`
- Place 2–3 instances in `Game.unity` scene for testing

### Success Criteria — Phase 1
- [ ] Automated: Project compiles; zero console errors on Play
- [ ] Automated: `BoomerangTarget.All` populates when enemies are active (verify via Debug.Log in Awake of a test enemy)
- [ ] Manual: `C6StatResolver` resolves context on Play without requiring "Print Resolved Context" right-click

---

## Phase 2 — Boomerang Projectile

### 2a. Create `BoomerangProjectile.cs`

**File:** `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/BoomerangProjectile.cs`

Key design:
- Receives frozen arc params from `BoomerangController` via `Initialize()` at spawn
- `FixedUpdate`: advances elapsed time, computes Bezier position, calls `rb.MovePosition()`
- `Update`: lerps render position between `_previousPosition` and `_currentPosition` for smoothness
- `SetInbound()`: called by controller at `PeakTurn` to switch legs
- `OnTriggerEnter2D`: calls `_controller.OnProjectileContact(target, contactPoint)` — controller owns all damage logic

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BoomerangProjectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private BoomerangController _controller;
    private PlayerController _player;

    // Arc params (frozen at spawn)
    private Vector2 _p0, _p1, _p2;   // outbound control points
    private Vector2 _perpRight;
    private float _arcFlightTime;

    // Inbound
    private bool _isInbound;
    private float _elapsedInbound;

    // Outbound
    private float _elapsedOutbound;

    // Render interpolation
    private Vector2 _previousPosition;
    private Vector2 _currentPosition;

    public void Initialize(
        BoomerangController controller,
        PlayerController player,
        Vector2 p0, Vector2 p1, Vector2 p2,
        Vector2 perpRight,
        float arcFlightTime)
    {
        _controller    = controller;
        _player        = player;
        _p0            = p0;
        _p1            = p1;
        _p2            = p2;
        _perpRight     = perpRight;
        _arcFlightTime = arcFlightTime;
        _currentPosition  = p0;
        _previousPosition = p0;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.interpolation = RigidbodyInterpolation2D.None; // we interpolate manually
    }

    public void SetInbound()
    {
        _isInbound       = true;
        _elapsedInbound  = 0f;
    }

    private void FixedUpdate()
    {
        _previousPosition = _currentPosition;

        if (!_isInbound)
        {
            _elapsedOutbound += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(_elapsedOutbound / _arcFlightTime);
            _currentPosition = EvaluateOutbound(t);
        }
        else
        {
            _elapsedInbound += Time.fixedDeltaTime;
            float s = Mathf.Clamp01(_elapsedInbound / _arcFlightTime);
            _currentPosition = EvaluateInbound(s);
        }

        _rb.MovePosition(_currentPosition);
    }

    private void Update()
    {
        float alpha = Time.deltaTime / Time.fixedDeltaTime; // 0..1 interpolation factor
        transform.position = Vector2.Lerp(_previousPosition, _currentPosition, alpha);
    }

    private Vector2 EvaluateOutbound(float t)
    {
        float u = 1f - t;
        return u * u * _p0 + 2f * u * t * _p1 + t * t * _p2;
    }

    private Vector2 EvaluateInbound(float s)
    {
        Vector2 perpLeft = -_perpRight;
        Vector2 playerPos = _player.transform.position;
        Vector2 p1In = (_p2 + playerPos) / 2f + perpLeft * (_arcFlightTime * 2f);
        // Note: arcRadius is baked into _arcFlightTime as ArcRadius*2 at controller side
        float u = 1f - s;
        return u * u * _p2 + 2f * u * s * p1In + s * s * playerPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<BoomerangTarget>(out var target) && target.IsAlive)
        {
            _controller.OnProjectileContact(target, other.ClosestPoint(transform.position));
        }
    }

    /// <summary>Returns normalized outbound progress (0..1). Used by controller for PeakTurn check.</summary>
    public float OutboundProgress => Mathf.Clamp01(_elapsedOutbound / _arcFlightTime);

    /// <summary>Returns normalized inbound progress (0..1). Used by controller for catch check.</summary>
    public float InboundProgress => Mathf.Clamp01(_elapsedInbound / _arcFlightTime);

    public Vector2 CurrentPosition => _currentPosition;
}
```

> **Note on inbound P1 formula:** The GDD multiplies `ArcRadius * 2` for the control point offset. In `EvaluateInbound` above, `_arcFlightTime` is the flight time, not ArcRadius. Fix this: `BoomerangController` should pass `ArcRadius` separately to the projectile, or store it as a field. Capture in Phase 3 implementation.

### 2b. Create boomerang prefab in Unity (via MCP)

- Create GameObject "Boomerang" with:
  - `SpriteRenderer` using a placeholder white circle sprite (create a 32×32 white circle texture or use Unity's built-in sprites)
  - `Rigidbody2D` (Kinematic)
  - `CircleCollider2D` (IsTrigger = true, radius ≈ 0.2)
  - `BoomerangProjectile` component
  - Layer: "Boomerang" (create if needed — exclude from player trigger)
- Save as `Assets/Prefabs/Boomerang.prefab`

### Success Criteria — Phase 2
- [ ] Automated: Project compiles; zero errors
- [ ] Manual: Instantiate boomerang prefab manually in scene — it sits at origin without moving (Initialize not called yet, which is expected)

---

## Phase 3 — Boomerang Controller

### 3a. Fix arc radius passing

Before implementing the controller, resolve the note from Phase 2: `BoomerangProjectile.Initialize()` needs `arcRadius` as a separate parameter so `EvaluateInbound` can use `ArcRadius * 2` correctly.

Update `Initialize` signature to include `float arcRadius` and store it as `_arcRadius`. Update `EvaluateInbound` to use `_arcRadius * 2f` instead of `_arcFlightTime * 2f`.

### 3b. Create `BoomerangController.cs`

**File:** `src/IncrementalAsteroidBoomerang/Assets/_Scripts/Gameplay/BoomerangController.cs`

```csharp
using UnityEngine;

public class BoomerangController : MonoBehaviour
{
    private enum State { Idle, ArmedForThrow, Outbound, PeakTurn, Inbound, CaughtLate }

    [SerializeField] private PlayerController _player;
    [SerializeField] private C6StatResolver _statResolver;
    [SerializeField] private GameObject _boomerangPrefab;

    [SerializeField] private float catchRadius = 0.8f;

    private State _state = State.Idle;
    private float _cooldownRemaining;
    private BoomerangProjectile _projectile;
    private GameStatsContext _stats;
    private int _contactIndex;

    private void Update()
    {
        switch (_state)
        {
            case State.Idle:       UpdateIdle();       break;
            case State.ArmedForThrow: UpdateArmed();   break;
            case State.Outbound:   UpdateOutbound();   break;
            case State.PeakTurn:   UpdatePeakTurn();   break;
            case State.Inbound:    UpdateInbound();    break;
            case State.CaughtLate: UpdateCaughtLate(); break;
        }
    }

    private void UpdateIdle()
    {
        _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - Time.deltaTime);
        if (_cooldownRemaining > 0f) return;
        if (FindNearestTarget() != null)
            _state = State.ArmedForThrow;
    }

    private void UpdateArmed()
    {
        var target = FindNearestTarget();
        if (target == null) { _state = State.Idle; return; }

        _stats = _statResolver.GetContext();
        _contactIndex = 0;

        Vector2 shipPos   = _player.transform.position;
        Vector2 targetPos = target.transform.position;

        Vector2 forward    = (targetPos - shipPos).normalized;
        Vector2 perpRight  = new Vector2(forward.y, -forward.x); // right of forward
        Vector2 p0         = shipPos;
        Vector2 p2         = targetPos;
        Vector2 p1         = (p0 + p2) / 2f + perpRight * (_stats.ArcRadius * 2f);

        float flightTime = Mathf.Max(0.1f, _stats.ArcFlightTime);
        if (_stats.ArcFlightTime < 0.1f)
            Debug.LogWarning("[BoomerangController] EC-5: ArcFlightTime below floor; clamped to 0.1s.", this);

        var go = Instantiate(_boomerangPrefab, shipPos, Quaternion.identity);
        _projectile = go.GetComponent<BoomerangProjectile>();
        _projectile.Initialize(this, _player, p0, p1, p2, perpRight, _stats.ArcRadius, flightTime);

        _state = State.Outbound;
    }

    private void UpdateOutbound()
    {
        if (_projectile == null) { _state = State.Idle; return; }
        if (_projectile.OutboundProgress >= 1f)
            _state = State.PeakTurn;
    }

    private void UpdatePeakTurn()
    {
        _projectile.SetInbound();
        _state = State.Inbound;
    }

    private void UpdateInbound()
    {
        if (_projectile == null) { _state = State.Idle; return; }

        float dist = Vector2.Distance(_projectile.CurrentPosition, _player.transform.position);
        if (dist <= catchRadius)
        {
            _cooldownRemaining = _stats.ThrowCooldown;
            Destroy(_projectile.gameObject);
            _projectile = null;
            _state = State.CaughtLate;
        }
    }

    private void UpdateCaughtLate()
    {
        _state = State.Idle;
    }

    /// <summary>Called by BoomerangProjectile on OnTriggerEnter2D.</summary>
    public void OnProjectileContact(BoomerangTarget target, Vector2 contactPoint)
    {
        int damage = ComputeDamage(_contactIndex);
        target.TakeDamage(damage, contactPoint);
        _contactIndex++;
    }

    private int ComputeDamage(int n)
    {
        float raw = _stats.BaseDamage * Mathf.Pow(1f - _stats.PierceFalloff, n);
        return Mathf.Max(1, Mathf.FloorToInt(raw));
    }

    private BoomerangTarget FindNearestTarget()
    {
        BoomerangTarget nearest = null;
        float minSqDist = float.MaxValue;
        Vector2 shipPos = _player.transform.position;

        foreach (var t in BoomerangTarget.All)
        {
            if (!t.IsAlive) continue;
            float sqDist = ((Vector2)t.transform.position - shipPos).sqrMagnitude;
            if (sqDist < minSqDist - 0.01f ||
                (Mathf.Abs(sqDist - minSqDist) <= 0.01f && t.transform.position.x < nearest.transform.position.x))
            {
                minSqDist = sqDist;
                nearest = t;
            }
        }
        return nearest;
    }
}
```

### 3c. Wire up scene in Unity (via MCP)

- Add `BoomerangController` component to the Player GameObject
- Set serialized refs in inspector:
  - `_player` → Player's `PlayerController`
  - `_statResolver` → Player's (or scene's) `C6StatResolver`
  - `_boomerangPrefab` → `Assets/Prefabs/Boomerang.prefab`
- Place 2–3 Enemy prefab instances in scene within ~10 units of player

### Success Criteria — Phase 3
- [ ] Automated: Project compiles; zero errors/warnings in console
- [ ] Manual: Hit Play — boomerang auto-fires toward nearest enemy within ~1 second
- [ ] Manual: Arc visibly curves right on outbound, left on inbound
- [ ] Manual: Enemy deactivates on contact (TakeDamage called)
- [ ] Manual: Boomerang returns toward player and disappears on catch
- [ ] Manual: Cooldown delays next throw (~0.8s gap between catches and next throw)
- [ ] Manual: With no enemies in scene — no throw fires

---

## GitHub Issue to File

**Title:** `[G3] Implement ChainBoomerangProjectile (chain_count > 0)`

**Body:**
```
ChainCount defaults to 0; chain is disabled in baseline play.
When ChainCount >= 1, first outbound contact should spawn a ChainBoomerangProjectile
at contact position targeting the nearest valid non-contact target.
Chain returns to its spawn position (not the player) and destroys itself.
See design/gdd/g3-boomerang-weapon-V2.md CR-7 and Acceptance Criteria — Chain section.
```

---

## Working Notes
- `perpRight` formula: `new Vector2(forward.y, -forward.x)` is the right-hand perpendicular (Unity's `Vector2.Perpendicular` returns left; negate for right).
- Inbound control point recomputes every `FixedUpdate` from live player position — this is intentional for the homing feel.
- `BoomerangProjectile.Update()` lerp uses `Time.deltaTime / Time.fixedDeltaTime` as alpha — this is an approximation; visual smoothness should be verified at different frame rates.
- `C6StatResolver` now auto-aggregates in `Awake()`. If a future system needs to add producers dynamically before aggregation, this will need revisiting.
