using System;
using System.Collections.Generic;
using UnityEngine;

namespace Horror.Manager
{
    public class HorrorManager : MonoBehaviour
    {
        public static HorrorManager Instance { get; private set; }

        [SerializeField] private float _currentValue;
        [SerializeField] private List<HorrorMapChange> _changes;
        private float _maxHorrorValue = 33f;

        public event Action<HorrorMapChange> OnHorrorChange;
        public event Action<float> OnHorrorValueChanged;

        public HorrorMapChange CurrentChange { get; private set; }
        public float CurrentValue => _currentValue;
        public float MaxHorrorValue => _maxHorrorValue;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddValue(float amount)
        {
            SetValue(_currentValue + amount);
        }

        public void SetMaxHorrorValue(float newMax)
        {
            _maxHorrorValue = newMax;
            if (_currentValue > _maxHorrorValue) _currentValue = _maxHorrorValue;
            Debug.Log($"Max horror value updated to {_maxHorrorValue}");
            OnHorrorValueChanged?.Invoke(_currentValue);
        }

        private void SetValue(float value)
        {
            _currentValue = value;
            if (_currentValue > _maxHorrorValue) _currentValue = _maxHorrorValue;
            if (_currentValue < 0.0f) _currentValue = 0.0f;

            float percent = (_currentValue / _maxHorrorValue) * 100f;
            Debug.Log($"Horror: {_currentValue:F1}/{_maxHorrorValue} ({percent:F0}%)");

            if (percent >= 100f && _maxHorrorValue == 100f) Debug.Log("=== 100% HORROR REACHED ===");
            else if (percent >= 99f && _maxHorrorValue == 66f) Debug.Log("=== 66% HORROR REACHED ===");
            else if (percent >= 99f && _maxHorrorValue == 33f) Debug.Log("=== 33% HORROR REACHED ===");

            OnHorrorValueChanged?.Invoke(_currentValue);

            var newChange = GetHorrorChangeForValue(_currentValue);
            if (newChange != null && newChange != CurrentChange)
            {
                CurrentChange = newChange;
                _changes.Remove(newChange);
                OnHorrorChange?.Invoke(CurrentChange);
            }
        }

        public void IncreaseHorrorByDepth()
        {
            AddValue(33.3f);
        }

        private HorrorMapChange GetHorrorChangeForValue(float value)
        {
            HorrorMapChange result = null;
            if (_changes.Count == 0) return null;
            float bestThreshold = float.NegativeInfinity;

            foreach (var change in _changes)
            {
                if (value >= change.changeThreshold && change.changeThreshold > bestThreshold)
                {
                    bestThreshold = change.changeThreshold;
                    result = change;
                }
            }
            return result;
        }
    }
}

