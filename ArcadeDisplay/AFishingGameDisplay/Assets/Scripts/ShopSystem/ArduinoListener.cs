using UnityEngine;
using Arduino;
using System;
using MixedRealityArcade.ArcadeDisplay.Networking;
using UnityEngine.SceneManagement;

public class ArduinoListener : MonoBehaviour
{
    public InventoryUI inventory;
    void Start()
    {
        inventory = FindObjectOfType<InventoryUI>();
        Arduino.ArduinoManager.Instance.OnCoinDataReceived += OnDataReceived;
        NetworkManager.Instance.ScoreEventReceived += ScoreEventReceived;
        NetworkManager.Instance.SimpleEventReceived += SimpleEventReceived;

    }
    //Bei erhaltener Münze
    private void OnDataReceived()
    {
        CurrencyWallet wallet = FindObjectOfType<CurrencyWallet>();
        if (wallet != null)
        {
            wallet.AddCoins(1);
        }
    }
    private void ScoreEventReceived(int score)
    {
        Debug.Log("ScoreEventReceived: " + score);
    }
    private void SimpleEventReceived(string payload)
    {
        //Wenn Fisch gefangen wurde, wird dieser in das Inventar hinzugefügt
        Debug.Log("ScoreEventReceived: " + payload);
        if (payload.StartsWith("fish_"))
        {
            var rarity = payload.Split("_")[1];
            switch (rarity)
            {
                case "1": inventory.AddCommonFish();break;
                case "2": inventory.AddRareFish();break;
                case "3": inventory.AddEpicFish();break;
                case "4": inventory.AddLegendaryFish();break;
                default: break;
            }

        }
        //Bei Spielende wird die Score Szene geladen
        if (payload.Equals( "gameDone"))
        {
            SceneManager.LoadScene("Score");
        }

    }
}
