using System.Collections;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _asteroidPrefab;

    [Header("Wave Size")]
    [SerializeField] private int _initialWaveSize = 1;
    [SerializeField] private int _maxWaveSize = 20;
    [SerializeField] private int _waveSizeIncrement = 2;

    [Header("Wave Interval (seconds)")]
    [SerializeField] private float _initialInterval = 10f;
    [SerializeField] private float _minInterval = 2f;
    [SerializeField] private float _intervalDecrement = 1f;

    [Header("Spawn")]
    [SerializeField] private float _spawnMargin = 1f;

    private int _currentWaveSize;
    private float _currentInterval;
    private Camera _cam;

    private void Awake()
    {
        if (_asteroidPrefab == null)
        {
            Debug.LogWarning($"[AsteroidSpawner] _asteroidPrefab is not set on {gameObject.name}. Spawner will not run.");
            enabled = false;
            return;
        }

        _cam = Camera.main;
        _currentWaveSize = Mathf.Max(1, _initialWaveSize);
        _currentInterval = Mathf.Max(_minInterval, _initialInterval);
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnWave(_currentWaveSize);
            yield return new WaitForSeconds(_currentInterval);

            _currentWaveSize = Mathf.Min(_currentWaveSize + _waveSizeIncrement, _maxWaveSize);
            _currentInterval = Mathf.Max(_currentInterval - _intervalDecrement, _minInterval);
        }
    }

    private void SpawnWave(int count)
    {
        float vExtent = _cam.orthographicSize;
        float hExtent = vExtent * _cam.aspect;
        Vector2 camPos = _cam.transform.position;

        for (int i = 0; i < count; i++)
        {
            int edge = Random.Range(0, 4);
            Vector2 spawnPos = camPos + GetEdgePoint(edge, hExtent, vExtent, _spawnMargin);
            Vector2 targetPos = camPos + GetEdgePoint(OppositeEdge(edge), hExtent, vExtent, 0f);

            var go = Instantiate(_asteroidPrefab, spawnPos, Quaternion.identity);
            var mover = go.GetComponent<AsteroidMover>();
            if (mover == null)
            {
                Debug.LogWarning($"[AsteroidSpawner] {_asteroidPrefab.name} is missing AsteroidMover. Add AsteroidMover to the prefab.");
                Destroy(go);
                continue;
            }
            mover.Initialize((targetPos - spawnPos).normalized);
        }
    }

    private static Vector2 GetEdgePoint(int edge, float hExtent, float vExtent, float margin)
    {
        switch (edge)
        {
            case 0: return new Vector2(Random.Range(-hExtent, hExtent),  vExtent + margin); // top
            case 1: return new Vector2(Random.Range(-hExtent, hExtent), -vExtent - margin); // bottom
            case 2: return new Vector2(-hExtent - margin, Random.Range(-vExtent, vExtent)); // left
            default: return new Vector2( hExtent + margin, Random.Range(-vExtent, vExtent)); // right
        }
    }

    private static int OppositeEdge(int edge)
    {
        switch (edge)
        {
            case 0: return 1;
            case 1: return 0;
            case 2: return 3;
            default: return 2;
        }
    }
}
