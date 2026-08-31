using Manager.Score;
using Manager.Input;
using UnityEngine;
using Manager.Navigation;

public class HighscoreInput : MonoBehaviour
{
    private static readonly char[] INPUT_CHARS = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private int _nameIndexCount = 0;
    private int _receivedScore;
    private int _currentSelectedCharIndex;
    private HighscoreMenuInputUI _inputUI;
    public HighscoreDisplay display;

    private void Awake()
    {
        _inputUI = GetComponent<HighscoreMenuInputUI>(); 
    }

    private void OnEnable()
    {
        InputManager.Instance.OnUp += HandleUpInput;
        InputManager.Instance.OnDown += HandleDownInput;
        InputManager.Instance.OnConfirm += HandleConfirmInput;
        InputManager.Instance.ToggleNameInput(true);

        if (ScoreManager.Instance != null)
        {
            SubmitScore(ScoreManager.Instance.CurrentScore);
        }
        else
        {
            Debug.Log("No Score manager!");
        }
    }

    private void HandleConfirmInput()
    {
        EnableNextNameInput();
    }

    private void HandleDownInput()
    {
        // Check if last char in alphabet is selected
        if (_currentSelectedCharIndex >= INPUT_CHARS.Length - 1)
        {
            _currentSelectedCharIndex = 0;
        }
        else
        {
            _currentSelectedCharIndex++;
        }
        _inputUI.ChangeDisplayedChar(INPUT_CHARS[_currentSelectedCharIndex], _nameIndexCount);
    }

    private void HandleUpInput()
    {
        // Check if first char in alphabet is selected
        if (_currentSelectedCharIndex == 0)
        {
            _currentSelectedCharIndex = INPUT_CHARS.Length - 1;
        }
        else
        {
            _currentSelectedCharIndex--;
        }
        _inputUI.ChangeDisplayedChar(INPUT_CHARS[_currentSelectedCharIndex], _nameIndexCount);
    }

    public void SubmitScore(int achievedScore)
    {
        _receivedScore = achievedScore;
        _nameIndexCount = 0;
        _inputUI.InitInputUI(INPUT_CHARS[0], _receivedScore);
        _inputUI.MarkInputEntry(_nameIndexCount);
    }

    private void EnableNextNameInput()
    {
        _currentSelectedCharIndex = 0;
        _nameIndexCount++;

        if (_nameIndexCount > 2)
        {
            _inputUI.FinalizeName(_receivedScore);
            display.Refresh();
            InputManager.Instance.ToggleNameInput(false);
            HighscoreManager.Instance.Save();
            NavigationManager.Instance.SelectFirstNavElement();
            return;
        }
        _inputUI.MarkInputEntry(_nameIndexCount);
    }

    private void OnDisable()
    {
        InputManager.Instance.OnUp -= HandleUpInput;
        InputManager.Instance.OnDown -= HandleDownInput;
        InputManager.Instance.OnConfirm -= HandleConfirmInput;
    }
}