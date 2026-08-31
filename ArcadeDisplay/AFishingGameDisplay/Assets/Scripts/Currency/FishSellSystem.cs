using UnityEngine;

public class FishSellSystem : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private CurrencyWallet wallet;
    [SerializeField] private CoinsLimitWarning coinsLimitWarning;
    [SerializeField] private CoinsFullWarning coinsFullWarning;

    public void TrySellAllFish()
    {
        if (wallet == null || playerInventory == null)
        {
            Debug.LogWarning("FishSellSystem is missing references.");
            return;
        }

        if (wallet.HasReachedMaxCoins())
        {
            Debug.Log("Coin-Limit erreicht. Verkauf nicht möglich.");

            if (coinsFullWarning != null)
            {
                coinsFullWarning.ShowWarning();
            }
            
            return;
        }

        if (coinsLimitWarning != null && WouldNeedConfirmationForNextSale())
        {
            coinsLimitWarning.ShowWarning(this);
            return;
        }

        SellAllFishUntilWalletIsFull();
    }

    public void ContinueSaleAfterWarning()
    {
        SellAllFishUntilWalletIsFull(allowPartialSale: true);
    }

    public int SellAllFishUntilWalletIsFull(bool allowPartialSale = false)
    {
        if (wallet == null || playerInventory == null)
        {
            Debug.LogWarning("FishSellSystem is missing references.");
            return 0;
        }

        if (wallet.HasReachedMaxCoins())
        {
            Debug.Log("Coin-Limit erreicht. Verkauf nicht möglich.");
            return 0;
        }

        int earnedCoins = 0;

        for (int i = playerInventory.StoredFish.Count - 1; i >= 0; i--)
        {
            InventoryItem item = playerInventory.StoredFish[i];

            while (item.Amount > 0)
            {
                int fishValue = item.Fish.CoinValue;

                if (!allowPartialSale && !wallet.HasEnoughSpaceFor(fishValue))
                {
                    if (wallet.Coins < wallet.MaxCoins && coinsLimitWarning != null)
                    {
                        coinsLimitWarning.ShowWarning(this);
                        return earnedCoins;
                    }

                    Debug.Log("Nicht genug Coin-Platz für diesen Fisch.");
                    return earnedCoins;
                }

                int coinsToAdd = allowPartialSale ? Mathf.Min(fishValue, wallet.MaxCoins - wallet.Coins) : fishValue;

                if (coinsToAdd <= 0)
                {
                    return earnedCoins;
                }

                //wallet.AddCoins(coinsToAdd);
                OutputCoins(coinsToAdd); // Output the coins to the Arduino
                playerInventory.RemoveOneFish(item);

                earnedCoins += coinsToAdd;

                if (wallet.HasReachedMaxCoins())
                {
                    Debug.Log("Coin-Limit erreicht.");
                    return earnedCoins;
                }
            }
        }

        Debug.Log($"Coins erhalten: {earnedCoins}");
        return earnedCoins;
    }

    private async void OutputCoins(int amount)
    {
        if(Arduino.ArduinoManager.Instance.IsConnected)
        if (Arduino.ArduinoManager.Instance != null)
        {
            for (int i = 0; i < amount; i++)
            {
                Arduino.ArduinoManager.Instance.SendRotationCommand();
                //Timeout 1s
                await System.Threading.Tasks.Task.Delay(1200);
            }
        }
        else
        {
            Debug.LogWarning("ArduinoManager instance not found. Cannot send coin output command.");
        }
        else
        {
            wallet.AddCoins(amount);
        }
    }

    private bool WouldNeedConfirmationForNextSale()
    {
        if (wallet == null || playerInventory == null || playerInventory.StoredFish.Count == 0)
        {
            return false;
        }

        foreach (InventoryItem item in playerInventory.StoredFish)
        {
            if (item == null || item.Fish == null || item.Amount <= 0)
            {
                continue;
            }

            int fishValue = item.Fish.CoinValue;

            if (!wallet.HasEnoughSpaceFor(fishValue) && wallet.Coins < wallet.MaxCoins)
            {
                return true;
            }
        }

        return false;
    }
}