using UnityEngine;

public enum FishRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    public string FishName;
    public FishRarity Rarity;

    public int CoinValue
    {
        get
        {
            switch (Rarity)
            {
                case FishRarity.Common:
                    return 1;

                case FishRarity.Rare:
                    return 2;

                case FishRarity.Epic:
                    return 4;

                case FishRarity.Legendary:
                    return 8;

                default:
                    return 1;
            }
        }
    }
}