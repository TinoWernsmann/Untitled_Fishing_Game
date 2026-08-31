using TMPro;
using Manager.Input;
using UnityEngine;

public class HighscoreMenuInputUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _nameChars;
    [SerializeField] private GameObject[] _entryUI;
    [SerializeField] private TextMeshProUGUI _scoreNumber;
    [SerializeField] private GameObject _inputUI;

    private string _finalName;

    public void ChangeDisplayedChar(char toDisplayChar, int charPos)
    {
        _nameChars[charPos].text = string.Empty + toDisplayChar;
    }

    public void MarkInputEntry(int posIndex)
    {
        ClearMarkInputEntry();
        _entryUI[posIndex].SetActive(true);
    }

    private void ClearMarkInputEntry()
    {
        foreach (GameObject ui in _entryUI)
        {
            ui.SetActive(false);
        }  
    }

    public void InitInputUI(char defaultChar, int score)
    {
        foreach (TextMeshProUGUI chara in _nameChars)
        {
            chara.text = string.Empty + defaultChar;
        }
        _scoreNumber.text = string.Empty + score;
        _inputUI.SetActive(true);
    }

    public void FinalizeName(int score)
    {
        ClearMarkInputEntry();
        foreach (TextMeshProUGUI chara in _nameChars)
        {
            _finalName += chara.text;
        }
        HighscoreManager.Instance.AddHighscore(_finalName, score);
        _inputUI.SetActive(false);
    }
}
