using System.IO;
using UnityEngine;
using MixedRealityArcade.CrossoverNetcode;

namespace MixedRealityArcade.ArcadeXR.Network
{

    [RequireComponent(typeof(NetworkPeerBehavior))]
    public class ConfigManager : MonoBehaviour
    {
        private NetworkConfig config;
        string filePath;

        private void Awake()
    {
        
        filePath = Path.Combine(Application.persistentDataPath, "config.json");
        LoadConfig();
    }

        public NetworkConfig LoadConfig()
        {
            Debug.Log(filePath);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Debug.Log(json);
                config = JsonUtility.FromJson<NetworkConfig>(json);
                Debug.Log($"Config geladen! IP: {config.targetAddress}, Lausch-Port: {config.listenPort}, Ziel-Port: {config.remotePort}");

            }
            else
            {
                Debug.Log("Config-Datei nicht gefunden. Erstelle eine neue mit Standardwerten.");
                config = new NetworkConfig();
                string json = JsonUtility.ToJson(config, true);
                File.WriteAllText(filePath, json);
                Debug.Log($"Neue Config-Datei erstellt unter: {filePath}");
            }
            return config;
        }
        public void SaveConfig(NetworkConfig config)
    {
        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Config gespeichert! IP: {config.targetAddress}, Lausch-Port: {config.listenPort}, Ziel-Port: {config.remotePort}");
    }
    }
}