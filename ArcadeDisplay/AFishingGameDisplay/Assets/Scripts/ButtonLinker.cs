using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonLinker : MonoBehaviour
{
    private void Start()
    {
        Button myButton = GetComponent<Button>();
        myButton.onClick.AddListener(() =>
        {
            if (HighscoreManager.Instance != null)
            {
                HighscoreManager.Instance.DeleteSave();
            }
        });
    }
}
