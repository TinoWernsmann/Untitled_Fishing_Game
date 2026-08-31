using System.Collections.Generic;
using UnityEngine;
using MixedRealityArcade.ArcadeDisplay.Networking;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;

public class BuyUpgrade : MonoBehaviour
{
    public enum UpgradeType
    {
        Rod,
        Bait
    }

    [Header("Upgrade")]
    [SerializeField]
    private UpgradeType upgradeType;

    [SerializeField]
    private UpgradeManager upgradeManager;

    [Header("Currency")]
    [SerializeField]
    private CurrencyWallet currencyWallet;

    [Tooltip("Kosten für die einzelnen Upgrade-Stufen.")]
    [SerializeField]
    private int[] upgradeCosts = { 5, 10, 15 };

    [Header("Warnings")]
    [SerializeField]
    private CoinsNotEnoughWarning coinsNotEnoughWarning;

    [SerializeField]
    private MaxUpgradeReachedWarning maxUpgradeReachedWarning;

    [Header("Upgrade UI")]
    [SerializeField]
    private UpgradeBoughtText upgradeBoughtText;

    [SerializeField]
    private CurrentUpgradesUI currentUpgradesUI;

    [Header("Networking")]
    [SerializeField]
    private NetworkManager networkManager;

    public void OnButtonClicked()
    {
        // Aktuelles Upgrade-Level des gewählten Upgrade-Typs ermitteln.
        int currentUpgradeLevel = GetCurrentUpgradeLevel();

        // Prüfen, ob das maximale Upgrade-Level erreicht wurde.
        if (currentUpgradeLevel >= GetMaximumUpgradeLevel())
        {
            maxUpgradeReachedWarning.ShowWarning();

            Debug.Log(
                $"{upgradeType} hat bereits das maximale Upgrade-Level " +
                $"({GetMaximumUpgradeLevel()}) erreicht."
            );

            return;
        }

        // Kosten für das nächste Upgrade bestimmen.
        int upgradeCost = upgradeCosts[currentUpgradeLevel];

        // Prüfen, ob der Spieler genügend Coins besitzt.
        if (currencyWallet.Coins < upgradeCost)
        {
            coinsNotEnoughWarning.ShowWarning();

            Debug.Log(
                $"Nicht genügend Coins für {upgradeType}. " +
                $"Benötigt: {upgradeCost}, " +
                $"Vorhanden: {currencyWallet.Coins}"
            );

            return;
        }

        // Upgrade durchführen.
        List<string> eventStrings = GetUpgradeEvents();

        // Wenn kein Upgrade durchgeführt werden konnte,
        // wird die Max-Level-Warnung angezeigt.
        if (eventStrings == null)
        {
            maxUpgradeReachedWarning.ShowWarning();

            Debug.Log(
                $"{upgradeType} konnte nicht weiter verbessert werden. " +
                "Maximales Upgrade-Level erreicht."
            );

            return;
        }

        // Coins erst nach erfolgreichem Upgrade ausgeben.
        if (!currencyWallet.TrySpendCoins(upgradeCost))
        {
            return;
        }

        // Events an das Netzwerk senden.
        SendUpgradeEvents(eventStrings);

        // Neues Upgrade-Level aus dem UpgradeManager holen.
        int newUpgradeLevel = GetCurrentUpgradeLevel();

        // Erfolgsmeldung anzeigen.
        upgradeBoughtText.ShowUpgradePurchased(
            upgradeType.ToString(),
            newUpgradeLevel
        );

        // Aktuelle Upgrade-Statistiken aktualisieren.
        currentUpgradesUI.UpdateUpgradeStats();

        Debug.Log(
            $"{upgradeType} wurde erfolgreich auf Level " +
            $"{newUpgradeLevel} verbessert. " +
            $"Kosten: {upgradeCost} Coins."
        );
    }

    private int GetCurrentUpgradeLevel()
    {
        switch (upgradeType)
        {
            case UpgradeType.Rod:
                return upgradeManager.RodUpgradeLevel;

            case UpgradeType.Bait:
                return upgradeManager.BaitUpgradeLevel;

            default:
                return 0;
        }
    }

    private int GetMaximumUpgradeLevel()
    {
        switch (upgradeType)
        {
            case UpgradeType.Rod:
                return upgradeManager.MaxRodUpgradeLevel;

            case UpgradeType.Bait:
                return upgradeManager.MaxBaitUpgradeLevel;

            default:
                return 0;
        }
    }

    private List<string> GetUpgradeEvents()
    {
        switch (upgradeType)
        {
            case UpgradeType.Rod:
                return upgradeManager.UpgradeRod();

            case UpgradeType.Bait:
                return upgradeManager.UpgradeBait();

            default:
                return null;
        }
    }

    private void SendUpgradeEvents(List<string> eventStrings)
    {
        foreach (string eventString in eventStrings)
        {
            SimpleEvent simpleEvent =
                ScriptableObject.CreateInstance<SimpleEvent>();

            simpleEvent.Payload = eventString;

            NetworkManager.Instance.SendCrossoverEvent(simpleEvent);

            Debug.Log($"Sent Event: {eventString}");
        }
        SimpleEvent simpleEventDepth =
                ScriptableObject.CreateInstance<SimpleEvent>();

                simpleEventDepth.Payload = "LayerUpgrade";
                NetworkManager.Instance.SendCrossoverEvent(simpleEventDepth);

    }
}