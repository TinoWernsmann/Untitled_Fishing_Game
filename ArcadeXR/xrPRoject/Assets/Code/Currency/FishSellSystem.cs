using UnityEngine;

public class FishSellSystem : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private CurrencyWallet wallet;

    public int SellAllFishUntilWalletIsFull()
    {
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

                if (!wallet.HasEnoughSpaceFor(fishValue))
                {
                    Debug.Log("Nicht genug Coin-Platz für diesen Fisch.");
                    return earnedCoins;
                }

                wallet.AddCoins(fishValue);
                playerInventory.RemoveOneFish(item);

                earnedCoins += fishValue;

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
}