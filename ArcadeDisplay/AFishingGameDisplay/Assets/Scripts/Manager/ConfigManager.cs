using System.IO;
using UnityEngine;
using MixedRealityArcade.CrossoverNetcode;

namespace Manager.Networking
{
    [RequireComponent(typeof(NetworkPeerBehavior))]
    public class ConfigManager : MonoBehaviour
    {
        [Header("Geladene Konfigurationen")]
        public NetworkConfig networkConfig;
        public ArduinoConfig arduinoConfig;

        [HideInInspector] public string networkFilePath;
        [HideInInspector] public string arduinoFilePath;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            networkFilePath = Path.Combine(Application.persistentDataPath, "config.json");
            arduinoFilePath = Path.Combine(Application.persistentDataPath, "arduino_config.json");
            
            Debug.Log($"Config-Pfade gesetzt! Speicherort: {Application.persistentDataPath}");
        }
        public NetworkConfig LoadConfig()
        {
            Debug.Log(networkFilePath);
            if (File.Exists(networkFilePath))
            {
                string json = File.ReadAllText(networkFilePath);
                Debug.Log("Network JSON: " + json);
                networkConfig = JsonUtility.FromJson<NetworkConfig>(json);
                Debug.Log($"Config geladen! IP: {networkConfig.targetAddress}, Lausch-Port: {networkConfig.listenPort}, Ziel-Port: {networkConfig.remotePort}");
            }
            else
            {
                Debug.Log("Config-Datei nicht gefunden. Erstelle eine neue mit Standardwerten.");
                Debug.Log(networkFilePath);
                networkConfig = new NetworkConfig();
                string json = JsonUtility.ToJson(networkConfig, true);
                File.WriteAllText(networkFilePath, json);
                Debug.Log($"Neue Config-Datei erstellt unter: {networkFilePath}");
            }
            return networkConfig;
        }

        public ArduinoConfig LoadArduinoConfig()
        {
            if (File.Exists(arduinoFilePath))
            {
                string json = File.ReadAllText(arduinoFilePath);
                Debug.Log("Arduino JSON: " + json);
                arduinoConfig = JsonUtility.FromJson<ArduinoConfig>(json);
                Debug.Log($"Arduino Config geladen! Coin: {arduinoConfig.portCoin}, Light: {arduinoConfig.portLight}");
            }
            else
            {
                Debug.Log("Arduino Config-Datei nicht gefunden. Erstelle eine neue mit Standardwerten.");
                arduinoConfig = new ArduinoConfig();
                string json = JsonUtility.ToJson(arduinoConfig, true);
                File.WriteAllText(arduinoFilePath, json);
                Debug.Log($"Neue Arduino Config-Datei erstellt unter: {arduinoFilePath}");
            }
            return arduinoConfig;
        }
        public void SaveArduinoConfig()
        {
            if (arduinoConfig != null)
            {
                string json = JsonUtility.ToJson(arduinoConfig, true);
                File.WriteAllText(arduinoFilePath, json);
                Debug.Log($"Arduino Config gespeichert unter: {arduinoFilePath}");
            }
        }
        public void SaveNetworkConfig()
{
    if (networkConfig != null)
    {
        try
        {
            string directory = Path.GetDirectoryName(networkFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(networkConfig, true);
            
            if (json == "{}")
            {
                Debug.LogWarning("Achtung: Das generierte JSON ist leer! Überprüfe, ob die Klasse [System.Serializable] hat und keine Properties ({ get; set; }) verwendet werden.");
            }

            File.WriteAllText(networkFilePath, json);
            Debug.Log($"Network Config erfolgreich gespeichert unter: {networkFilePath}\nInhalt: {json}");
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fehler beim Speichern der Network Config: {e.Message}");
        }
    }
    else
    {
        Debug.LogWarning("SaveNetworkConfig fehlgeschlagen: networkConfig ist null!");
    }
}
    }
}