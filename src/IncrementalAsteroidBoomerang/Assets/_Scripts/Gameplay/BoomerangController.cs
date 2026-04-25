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
            case State.Idle:          UpdateIdle();      break;
            case State.ArmedForThrow: UpdateArmed();     break;
            case State.Outbound:      UpdateOutbound();  break;
            case State.PeakTurn:      UpdatePeakTurn();  break;
            case State.Inbound:       UpdateInbound();   break;
            case State.CaughtLate:    UpdateCaughtLate(); break;
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

        Vector2 forward   = (targetPos - shipPos).normalized;
        Vector2 perpRight = new Vector2(forward.y, -forward.x);
        Vector2 p0        = shipPos;
        Vector2 p2        = targetPos;
        Vector2 p1        = (p0 + p2) / 2f + perpRight * (_stats.ArcRadius * 2f);

        float flightTime = Mathf.Max(0.1f, _stats.ArcFlightTime);
        if (_stats.ArcFlightTime < 0.1f)
            Debug.LogWarning("[BoomerangController] EC-5: ArcFlightTime below floor; clamped to 0.1s.", this);

        var go = Instantiate(_boomerangPrefab, shipPos, Quaternion.identity);
        _projectile = go.GetComponent<BoomerangProjectile>();
        if (_projectile == null)
        {
            Debug.LogWarning("[BoomerangController] Boomerang prefab is missing BoomerangProjectile component.", this);
            Destroy(go);
            _state = State.Idle;
            return;
        }

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
        if (_projectile == null) { _state = State.Idle; return; }
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

    /// <summary>Called by BoomerangProjectile on OnTriggerEnter2D. Controller owns all damage logic.</summary>
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
                (Mathf.Abs(sqDist - minSqDist) <= 0.01f && nearest != null &&
                 t.transform.position.x < nearest.transform.position.x))
            {
                minSqDist = sqDist;
                nearest = t;
            }
        }
        return nearest;
    }
}
