using UnityEngine;
using TMPro;

public class RandomMenuTitle : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;

    private readonly string[] titleOptions =
    {
        "titel",
        "lorem fishum",
        "catfishing",
        "get fish quick",
        "let's go fishing!"
    };

    private void Start()
    {
        titleText.text = titleOptions[Random.Range(0, titleOptions.Length)];
    }
}
