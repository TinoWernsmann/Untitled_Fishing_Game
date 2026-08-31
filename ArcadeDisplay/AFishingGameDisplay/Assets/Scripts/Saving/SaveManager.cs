using UnityEngine;

namespace Saving.Save
{
    /// <summary>
    /// Handles Highscore saving in the game.
    /// Also loads the saved Highscore.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string HighscoreKey = "Highscore";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            var go = new GameObject("SaveManager");
            Instance = go.AddComponent<SaveManager>();
            DontDestroyOnLoad(go);
        }

        public int GetSavedHighScore()
        {
            return PlayerPrefs.GetInt(HighscoreKey, 0);
        }

        public void SaveHighscore(int score)
        {
            PlayerPrefs.SetInt(HighscoreKey, score);
            PlayerPrefs.Save();
        }
    }  
}

