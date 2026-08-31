using Game.World.Timer;
using System;
using Horror.Manager;
using UnityEngine;
using MixedRealityArcade.ArcadeXR.Network;

namespace Manager.Timer
{
    /// <summary>
    /// Manages the Time limit of the fishing game.
    /// Starts timer once fishing scene loads, but can also be started manually.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private float _defaultTimer;
        [Tooltip("Debug Option to disable the Timer if necessary")]
        [SerializeField] private bool _isEnabled;

        private TimerUI _timerUI;
        private float _remainingTime;
        private bool _isRunning;
        private float _horrorRampDuration;
        private float MaxHorrorValue => HorrorManager.Instance != null ? HorrorManager.Instance.MaxHorrorValue : 33f;
        private bool _bossSpawned;

        public event Action OnTimerDone;
        public event Action OnBossSpawnTime;
        public float RemainingTime => _remainingTime;

        private void Start()
        {
            _timerUI = GetComponentInChildren<TimerUI>();

            if (_isEnabled) 
            {
                StartTimer(_defaultTimer); 
            }
        }
        void OnEnable()
        {
            NetworkManager.Instance.OnStart += OnStart;
        }

        void OnDisable()
        {
            NetworkManager.Instance.OnStart -= OnStart;
        }

        private void OnStart()
        {
            StartTimer(_defaultTimer);
        }

        private void Update()
        {
            if (_isRunning)
            {
                _remainingTime -= Time.deltaTime;
                _timerUI.UpdateTimerText(_remainingTime);

                float elapsedTime = _defaultTimer - _remainingTime;

                if (elapsedTime <= _horrorRampDuration)
                {
                    float horrorToAdd = (100.0f / _horrorRampDuration) * Time.deltaTime;

                    HorrorManager manager = HorrorManager.Instance;
                    if(manager != null)
                    {
                        HorrorManager.Instance.AddValue(horrorToAdd);
                    }
                }

                if (_remainingTime <= 15f && !_bossSpawned && HorrorManager.Instance.CurrentValue >= 95f)
                {
                    _bossSpawned = true;
                    OnBossSpawnTime?.Invoke();
                    Debug.Log($"=== BOSS SPAWN SIGNAL: {_remainingTime}s left, Horror {HorrorManager.Instance.CurrentValue}% ===");
                }
            }

            if (_remainingTime <= 0f && _isRunning)
            {
                _remainingTime = 0.0f;
                _timerUI.UpdateTimerText(_remainingTime);
                _isRunning = false;
                OnTimerDone?.Invoke();
                return;
            }
        }

        /// <summary>
        /// Starts the in game timer with a given time.
        /// </summary>
        /// <param name="time">Time of the timer</param>
        public void StartTimer(float time)
        {
            _remainingTime = time;
            _horrorRampDuration = time * 0.75f;
            _isRunning = true;
            _bossSpawned = false;
        }

        public void StopTimerManually()
        {
            _isRunning = false;
        }

        public void ExtendHorrorRampDuration(float additionalTime)
        {
            _horrorRampDuration += additionalTime;
            Debug.Log($"Horror ramp duration extended to {_horrorRampDuration}s");
        }
    } 
}

