using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BoomerangProjectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private BoomerangController _controller;
    private PlayerController _player;

    // Arc params (frozen at spawn)
    private Vector2 _p0, _p1, _p2;
    private Vector2 _perpRight;
    private float _arcFlightTime;
    private float _arcRadius;

    // Outbound
    private float _elapsedOutbound;

    // Inbound
    private bool _isInbound;
    private float _elapsedInbound;

    // Render interpolation
    private Vector2 _previousPosition;
    private Vector2 _currentPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.interpolation = RigidbodyInterpolation2D.None;
    }

    public void Initialize(
        BoomerangController controller,
        PlayerController player,
        Vector2 p0, Vector2 p1, Vector2 p2,
        Vector2 perpRight,
        float arcRadius,
        float arcFlightTime)
    {
        _controller    = controller;
        _player        = player;
        _p0            = p0;
        _p1            = p1;
        _p2            = p2;
        _perpRight     = perpRight;
        _arcRadius     = arcRadius;
        _arcFlightTime = arcFlightTime;
        _currentPosition  = p0;
        _previousPosition = p0;
    }

    public void SetInbound()
    {
        _isInbound      = true;
        _elapsedInbound = 0f;
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
        // Interpolate render position between the last two physics frames
        float alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
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
        Vector2 p1In = (_p2 + playerPos) / 2f + perpLeft * (_arcRadius * 2f);
        float u = 1f - s;
        return u * u * _p2 + 2f * u * s * p1In + s * s * playerPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<BoomerangTarget>(out var target) && target.IsAlive)
            _controller.OnProjectileContact(target, other.ClosestPoint(transform.position));
    }

    /// <summary>Normalized outbound progress (0–1). Used by controller to detect PeakTurn.</summary>
    public float OutboundProgress => Mathf.Clamp01(_elapsedOutbound / _arcFlightTime);

    /// <summary>Normalized inbound progress (0–1). Used by controller for catch check.</summary>
    public float InboundProgress => Mathf.Clamp01(_elapsedInbound / _arcFlightTime);

    public Vector2 CurrentPosition => _currentPosition;
}
