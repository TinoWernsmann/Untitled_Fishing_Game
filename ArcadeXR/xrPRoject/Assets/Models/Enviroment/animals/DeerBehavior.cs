using UnityEngine;
using Manager.Timer;

public class DeerBehavior : MonoBehaviour
{
    [SerializeField] private float _fleeSpeed = 7f;

    private Animator _animator;
    private TimeManager _timeManager;
    private bool _isFleeing;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _timeManager = FindObjectOfType<TimeManager>();

        if (_timeManager != null)
        {
            _timeManager.OnBossSpawnTime += OnBossSpawn;
        }
    }

    private void OnDestroy()
    {
        if (_timeManager != null)
        {
            _timeManager.OnBossSpawnTime -= OnBossSpawn;
        }
    }

    private void Update()
    {
        if (_isFleeing && _timeManager != null && _timeManager.RemainingTime > 0)
        {
            transform.Translate(Vector3.back * _fleeSpeed * Time.deltaTime);
        }
    }

    private void OnBossSpawn()
    {
        if (_animator != null)
        {
            transform.Rotate(0, 180, 0);
            _animator.SetTrigger("triggerFlee");
            Invoke(nameof(StartFleeing), 1f);
            Debug.Log("=== DEER FLEEING ===");
        }
    }

    private void StartFleeing()
    {
        _isFleeing = true;
    }
}
