using System;
using TMPro;
using UnityEngine;

namespace Game.World.Timer
{
    /// <summary>
    /// Manages the displaying of the Timer UI in the game World
    /// </summary>
    public class TimerUI : MonoBehaviour
    {
        private TextMeshProUGUI _timerText;

        private void Start()
        {
            _timerText = GetComponentInChildren<TextMeshProUGUI>();
        }

        /// <summary>
        /// Updates the displayed Timer text with a given time.
        /// The given Time is formatted correctly.
        /// </summary>
        /// <param name="time">Time to display</param>
        public void UpdateTimerText(float time)
        {
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);

                _timerText.SetText("{0}:{1:00}", minutes, seconds);
            }
        }
    }
}
