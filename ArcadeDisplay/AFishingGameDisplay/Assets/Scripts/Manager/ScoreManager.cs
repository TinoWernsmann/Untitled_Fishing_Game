using Saving.Save;
using UnityEngine;
using UI.Score;
using MixedRealityArcade.ArcadeDisplay.Networking;

namespace Manager.Score
{
    /// <summary>
    /// Handles the saving of the current score and Highscore for a play session.
    /// Initializes saving of a new Highscore if there is a new one to save.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [SerializeField] private ScoreUI _scoreUI;

        private int _highscore = 0;
        private int _currentScore = 0;

        public int CurrentHighscore => _highscore;
        public int CurrentScore => _currentScore;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            //_highscore = SaveManager.Instance.GetSavedHighScore();
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            NetworkManager.Instance.ScoreEventReceived += SetCurrentScore;
            //DebugScore();
            BeginGame();
        }

        private void DebugScore()
        {
            _currentScore = 100;
        }

        public void SetCurrentScore(int score)
        {
            _currentScore = score;
            _scoreUI.UpdateCurrentScoreText(_currentScore);
        }

        /// <summary>
        /// Called when the game has ended (Timer is done in VR)
        /// Checks if a new Highscore is reached and calls for it to be saved.
        /// </summary>
        public void EndGame()
        {
            if (_currentScore > _highscore)
            {
                _highscore = _currentScore;
                SaveManager.Instance.SaveHighscore(_highscore);
            }

           // UpdateHighscoreUI();
        }

        /// <summary>
        /// Called when the game is started (Currently: Score Display Scene is loaded.)
        /// Loads the currently saved Highscore from SaveManager and shows it.
        /// </summary>
        private void BeginGame()
        {
           // _highscore = SaveManager.Instance.GetSavedHighScore();

           // _scoreUI.UpdateHighscoreText(_highscore);
            _scoreUI.UpdateCurrentScoreText(_currentScore);
        }

       
    }
}