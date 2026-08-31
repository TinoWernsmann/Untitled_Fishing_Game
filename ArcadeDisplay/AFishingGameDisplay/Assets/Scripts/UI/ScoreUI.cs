using TMPro;
using UnityEngine;

namespace UI.Score
{
    /// <summary>
    /// Handles the Score UI of the game.
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _currentScoreText;

        public void UpdateCurrentScoreText(int currentScore)
        {
            _currentScoreText.text = string.Empty + currentScore;
        }
    }
}

