using System.Collections;
using UnityEngine;

public class CoinsNotEnoughWarning : MonoBehaviour
{
    [SerializeField]
    private float displayDuration = 2f;

    private Coroutine hideWarningCoroutine;

    private void Awake()
    {
        HideWarning();
    }

    public void ShowWarning()
    {
        // Falls bereits ein Timer läuft, diesen zurücksetzen.
        if (hideWarningCoroutine != null)
        {
            StopCoroutine(hideWarningCoroutine);
        }

        gameObject.SetActive(true);

        hideWarningCoroutine = StartCoroutine(HideWarningAfterDelay());
    }

    private IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        HideWarning();
        hideWarningCoroutine = null;
    }

    public void HideWarning()
    {
        gameObject.SetActive(false);
    }
}