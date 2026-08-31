using UnityEngine;
using TMPro;

public class DebugHelper : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    private static DebugHelper instance;

    void Awake()
    {
        instance = this;
    }

    public static void SetMessage(string message)
    {
        if(instance != null)
        {
            instance.debugText.text = message;
        }
    }
}