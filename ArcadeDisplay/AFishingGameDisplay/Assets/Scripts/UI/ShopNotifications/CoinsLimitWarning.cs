using UnityEngine;

public class CoinsLimitWarning : MonoBehaviour
{
    [SerializeField] private CanvasGroup warningPanel;

    private FishSellSystem currentFishSellSystem;

    private void Awake()
    {
        TryInitializeWarningPanel();
        HideWarning();
    }

    public void ShowWarning(FishSellSystem fishSellSystem)
    {
        currentFishSellSystem = fishSellSystem;
        TryInitializeWarningPanel();

        if (warningPanel == null)
        {
            if (currentFishSellSystem != null)
            {
                currentFishSellSystem.ContinueSaleAfterWarning();
            }

            currentFishSellSystem = null;
            return;
        }

        warningPanel.alpha = 1f;
        warningPanel.interactable = true;
        warningPanel.blocksRaycasts = true;
    }

    public void ConfirmSale()
    {
        HideWarning();

        if (currentFishSellSystem == null)
        {
            return;
        }

        currentFishSellSystem.ContinueSaleAfterWarning();
        currentFishSellSystem = null;
    }

    public void CancelSale()
    {
        HideWarning();
        currentFishSellSystem = null;
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