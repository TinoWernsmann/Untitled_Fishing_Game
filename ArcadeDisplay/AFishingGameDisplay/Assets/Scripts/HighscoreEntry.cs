using UnityEngine;

[System.Serializable]
public class HighscoreEntry
{
    public string playerName;
    public int score;

    public HighscoreEntry(string playerName, int score)
    {
        this.playerName = playerName;
        this.score = score;
    }
}
