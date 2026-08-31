using System;
using UnityEngine;

public class CurrencyWallet : MonoBehaviour
{
    public event Action<int> OnCoinsChanged;

    [SerializeField] private int maxCoins = 25;

    public int Coins { get; private set; }

    public int MaxCoins => maxCoins;

    public bool HasReachedMaxCoins()
    {
        return Coins >= maxCoins;
    }

    public bool HasEnoughSpaceFor(int amount)
    {
        return Coins + amount <= maxCoins;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        Coins = Mathf.Clamp(Coins + amount, 0, maxCoins);

        OnCoinsChanged?.Invoke(Coins);
    }

    // Das könntest du für den Upgradeshop benutzten, um Coins auszugeben.
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
            return false;

        if (Coins < amount)
            return false;

        Coins -= amount;

        OnCoinsChanged?.Invoke(Coins);

        return true;
    }
    
    public void ResetWallet()
    {
        Coins = 0;

        OnCoinsChanged?.Invoke(Coins);
    }
}