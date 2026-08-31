using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;

    private const string DEFAULT_SCORE_TEXT = "Score: ";
    private const string MAIN_SCENE = "MainVRScene";

    public void LoadMainGame()
    {
        SceneManager.LoadScene(MAIN_SCENE);
    }
}
