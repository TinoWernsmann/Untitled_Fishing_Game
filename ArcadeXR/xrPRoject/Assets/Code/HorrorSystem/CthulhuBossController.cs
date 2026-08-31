using UnityEngine;
using Manager.Timer;

public class CthulhuBossController : MonoBehaviour
{
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private Transform _spawnPoint;

    private TimeManager _timeManager;

    private void Start()
    {
        _timeManager = FindObjectOfType<TimeManager>();
        if (_timeManager != null)
        {
            _timeManager.OnBossSpawnTime += SpawnBoss;
        }
    }

    private void OnDestroy()
    {
        if (_timeManager != null)
        {
            _timeManager.OnBossSpawnTime -= SpawnBoss;
        }
    }

    private void SpawnBoss()
    {
        if (_bossPrefab == null)
        {
            Debug.LogError("Boss Prefab not assigned!");
            return;
        }

        Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
        Instantiate(_bossPrefab, spawnPos, Quaternion.identity);
        Debug.Log("=== CTHULHU EMERGED WITH 15s LEFT ===");
    }
}
