using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private CurrencyWallet wallet;
    [SerializeField] private FishSellSystem sellSystem;

    [Header("Fish Types")]
    [SerializeField] private FishData commonFish;
    [SerializeField] private FishData rareFish;
    [SerializeField] private FishData epicFish;
    [SerializeField] private FishData legendaryFish;

    [Header("UI")]
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text coinsText;

    private void Start()
    {
        UpdateUI();

        inventory.OnInventoryChanged += UpdateUI;
        wallet.OnCoinsChanged += _ => UpdateUI();
    }

    private void OnDestroy()
    {
        inventory.OnInventoryChanged -= UpdateUI;
        wallet.OnCoinsChanged -= _ => UpdateUI();
    }

    public void AddCommonFish()
    {
        inventory.AddFish(commonFish, 1);
    }

    public void AddRareFish()
    {
        inventory.AddFish(rareFish, 1);
    }

    public void AddEpicFish()
    {
        inventory.AddFish(epicFish, 1);
    }

    public void AddLegendaryFish()
    {
        inventory.AddFish(legendaryFish, 1);
    }

    public void SellAllFishes()
    {
        sellSystem.TrySellAllFish();
    }

    private void UpdateUI()
    {
        inventoryText.text = BuildInventoryText();
        coinsText.text = $"Coins: {wallet.Coins}/{wallet.MaxCoins}";
    }

    private string BuildInventoryText()
    {
        if (inventory.StoredFish.Count == 0)
            return "You have no fishes in your inventory.";

        string text = "Inventory:\n";

        foreach (InventoryItem item in inventory.StoredFish)
        {
            int totalValue = item.Fish.CoinValue * item.Amount;

            text += $"{item.Fish.FishName} x{item.Amount} | Value: {totalValue} Coins\n";
        }

        return text;
    }
    
    public void ResetTestData()
    {
        wallet.ResetWallet();
        inventory.ClearInventory();

        Debug.Log("Inventar und Coins zurückgesetzt.");
    }
}