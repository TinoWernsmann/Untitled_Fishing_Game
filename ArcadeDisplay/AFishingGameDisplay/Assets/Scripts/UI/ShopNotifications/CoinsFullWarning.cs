using System.Collections;
using UnityEngine;

public class CoinsFullWarning : MonoBehaviour
{
    [SerializeField] private CanvasGroup warningPanel;
    [SerializeField] private float displayDuration = 3f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        TryInitializeWarningPanel();
        HideWarning();
    }

    public void ShowWarning()
    {
        TryInitializeWarningPanel();

        if (warningPanel == null)
        {
            Debug.LogWarning("CoinsFullWarning: Kein CanvasGroup gefunden.");
            return;
        }

        // Bereits laufenden Timer stoppen.
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        warningPanel.alpha = 1f;
        warningPanel.interactable = false;
        warningPanel.blocksRaycasts = false;

        hideCoroutine = StartCoroutine(HideWarningAfterDelay());
    }

    private IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        HideWarning();
        hideCoroutine = null;
    }

    private void HideWarning()
    {
        if (warningPanel == null)
        {
            return;
        }

        warningPanel.alpha = 0f;
        warningPanel.interactable = false;
        warningPanel.blocksRaycasts = false;
    }

    private void TryInitializeWarningPanel()
    {
        if (warningPanel != null)
        {
            return;
        }

        warningPanel = GetComponentInChildren<CanvasGroup>(true);
    }
}