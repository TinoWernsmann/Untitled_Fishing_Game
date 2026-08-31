using UnityEngine;
using System.Collections.Generic;

public class HighscoreDisplay : MonoBehaviour
{
    public HighscoreRow[] rows;

    void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        HighscoreManager.Instance.OnHighscoreClear += ClearHighscore;
    }

    private void ClearHighscore()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].Clear();
        }
    }

    public void Refresh()
    {
        List<HighscoreEntry> scores =
            HighscoreManager.Instance.GetHighscores();

        for (int i = 0; i < rows.Length; i++)
        {
            if (i < scores.Count)
            {
                rows[i].SetEntry(
                    scores[i].playerName,
                    scores[i].score);
            }
            else
            {
                rows[i].Clear();
            }
        }
    }

    private void OnDisable()
    {
        HighscoreManager.Instance.OnHighscoreClear -= ClearHighscore;
    }
}