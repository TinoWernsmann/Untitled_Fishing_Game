using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class HighscoreData
{
    public List<HighscoreEntry> highscores = new List<HighscoreEntry>();
}


public class HighscoreManager : MonoBehaviour
{
    public static HighscoreManager Instance;

    private const string HIGHSCORE_KEY = "TopHighscores";
    private HighscoreData _highscoreData;

    public event Action OnHighscoreClear;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            _highscoreData = LoadHighscores();
            
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public List<HighscoreEntry> GetHighscores()
    {
        return _highscoreData.highscores;
    }

    public void AddHighscore(string name, int score)
    {
        name = name.ToUpper();

        if (name.Length > 3)
            name = name.Substring(0, 3);

        _highscoreData.highscores.Add(new HighscoreEntry(name, score));

        _highscoreData.highscores.Sort((a, b) => b.score.CompareTo(a.score));

        if (_highscoreData.highscores.Count > 9)
        {
            _highscoreData.highscores.RemoveRange(9, _highscoreData.highscores.Count - 9);
        }
    }

    public void Save()
    {
        SaveHighscores(_highscoreData);
    }

    private void SaveHighscores(HighscoreData data)
    {
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(HIGHSCORE_KEY, json);
        PlayerPrefs.Save();
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(HIGHSCORE_KEY);
        _highscoreData = new HighscoreData();
        OnHighscoreClear?.Invoke();
    }

    public HighscoreData LoadHighscores()
    {
        if (PlayerPrefs.HasKey(HIGHSCORE_KEY))
        {
            string json = PlayerPrefs.GetString(HIGHSCORE_KEY);
            return JsonUtility.FromJson<HighscoreData>(json);
        }

        return new HighscoreData();
    }
}