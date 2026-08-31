using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 
using Manager.Input;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Einstellungen")]
    public float wartezeitInSekunden = 52f; 
    public string naechsteSzene = "MainMenu"; 

    void Start()
{
    if (InputManager.Instance != null)
    {
        InputManager.Instance.EnableNavInput();
        InputManager.Instance.OnNavConfirm += OnConfirm; 
    }
    
    StartCoroutine(WechsleSzeneNachZeit());
}

void OnDisable()
{
    if (InputManager.Instance != null)
    {
        InputManager.Instance.OnNavConfirm -= OnConfirm;
    }
}

    IEnumerator WechsleSzeneNachZeit()
    {
        yield return new WaitForSeconds(wartezeitInSekunden);
        SceneManager.LoadScene(naechsteSzene);
    }

    void OnConfirm()
    {
        Debug.Log("Skip Credits");
        SceneManager.LoadScene(naechsteSzene);
    }
}