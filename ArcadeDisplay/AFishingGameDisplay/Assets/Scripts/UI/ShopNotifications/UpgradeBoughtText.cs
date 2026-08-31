using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradeBoughtText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text messageText;

    [SerializeField]
    private float displayDuration = 2f;

    private Coroutine hideTextCoroutine;

    private void Awake()
    {
        HideText();
    }

    public void ShowUpgradePurchased(string upgradeName, int upgradeLevel)
    {
        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        messageText.text =
            $"You purchased {upgradeName} Upgrade Level {upgradeLevel}!";

        gameObject.SetActive(true);

        hideTextCoroutine = StartCoroutine(HideTextAfterDelay());
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        HideText();
        hideTextCoroutine = null;
    }

    public void HideText()
    {
        gameObject.SetActive(false);
    }
}