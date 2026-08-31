using UnityEngine;
using MixedRealityArcade.ArcadeDisplay.Networking;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;

public class NetworkButtonSender : MonoBehaviour
{
    // Du kannst hier im Inspector den Text eintragen, der gesendet werden soll (z.B. "testMe")
    public string eventPayload = "GameStart"; 

    public void SendEventToNetwork()
    {
        if (NetworkManager.Instance != null)
        {
            var simpleEvent = ScriptableObject.CreateInstance<SimpleEvent>();
            simpleEvent.Payload = eventPayload;
            
            NetworkManager.Instance.SendCrossoverEvent(simpleEvent);
            Debug.Log($"Button-Klick gesendet: {eventPayload}");
        }
        else
        {
            Debug.LogError("Fehler: NetworkManager fehlt! Das Event konnte nicht gesendet werden.");
        }
    }
}