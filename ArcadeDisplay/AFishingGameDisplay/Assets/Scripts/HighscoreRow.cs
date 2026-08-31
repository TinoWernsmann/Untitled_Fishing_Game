using TMPro;
using UnityEngine;

public class HighscoreRow : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text scoreText;

    public void SetEntry(string name, int score)
    {
        nameText.text = name;
        scoreText.text = score.ToString();
    }

    public void Clear()
    {
        nameText.text = "---";
        scoreText.text = "---";
    }
}