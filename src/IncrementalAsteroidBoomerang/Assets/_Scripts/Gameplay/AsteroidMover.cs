using UnityEngine;

public class AsteroidMover : MonoBehaviour
{
    [SerializeField] private float _speed = 3f;

    private Vector2 _direction;
    private Camera _cam;
    private float _aliveTime;

    private void Awake()
    {
        _cam = Camera.main;
    }

    public void Initialize(Vector2 direction)
    {
        _direction = direction.normalized;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

        _aliveTime += Time.deltaTime;
        if (_aliveTime >= 60f && IsOffScreen())
            Destroy(gameObject);
    }

    private bool IsOffScreen()
    {
        Vector3 vp = _cam.WorldToViewportPoint(transform.position);
        return vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f;
    }
}
