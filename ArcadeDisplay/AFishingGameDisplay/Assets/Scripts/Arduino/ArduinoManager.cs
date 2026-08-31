using UnityEngine;
using System.IO.Ports;
using Manager.Networking;
using System;

namespace Arduino
{
    public class ArduinoManager : MonoBehaviour
    {
        public static ArduinoManager Instance { get; private set; }

        public bool IsConnected { get; private set; }

        [Header("Coin Arduino")]
        public string coinPortName;
        private SerialPort streamCoin;
        public Action OnCoinDataReceived;

        [Header("Light Arduino")]
        public string lightPortName;
        private SerialPort streamLight;

        [Header("Settings")]
        public int baudRate;
        public ConfigManager configManager;

        public float coinCooldown = 0.8f;
        private float lastCoinTriggerTime = -100f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (configManager == null)
            {
                configManager = FindFirstObjectByType<ConfigManager>();
            }
        }

        void Start()
        {
            if (Instance != this) return;

            if (configManager == null)
            {
                Debug.LogError("ArduinoManager: Kein ConfigManager gefunden!");
                return;
            }

            ArduinoConfig arduinoConfig = configManager.LoadArduinoConfig();

            coinPortName = arduinoConfig.portCoin;
            lightPortName = arduinoConfig.portLight;
            baudRate = arduinoConfig.baudRate;


            InitializePort(ref streamCoin, coinPortName, "Coin");
            InitializePort(ref streamLight, lightPortName, "Light");
        }

        private void InitializePort(ref SerialPort port, string portName, string deviceName)
        {
            if (!string.IsNullOrEmpty(portName) && baudRate != 0)
            {
#if UNITY_EDITOR_OSX
                Debug.LogWarning($"Mac-Modus: Serielle Verbindung für {deviceName} wird übersprungen.");
                IsConnected = false;
#else
                try
                {
                    portName = portName.Trim();

                    port = new SerialPort(portName, baudRate);
                    port.ReadTimeout = 50;
                    port.Open();

                    Debug.Log($"Serial Port für {deviceName} erfolgreich geöffnet: {portName}");
                    IsConnected = true;
                }
                catch (System.PlatformNotSupportedException)
                {
                    Debug.LogError($"SerialPort für {deviceName} wird nicht unterstützt. (Prüfe die .NET Einstellungen)");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Zugriff verweigert oder Fehler bei {deviceName} (Port '{portName}'): {ex.Message}");
                }
#endif
            }
            else
            {
                Debug.LogWarning($"ArduinoManager: Portname für {deviceName} fehlt in der Config oder Baudrate ist 0.");
            }
        }

        public void Update()
        {

            if (streamCoin != null && streamCoin.IsOpen && streamCoin.BytesToRead > 0)
            {
                ReadFromCoinArduino();
            }
        }

        private void ReadFromCoinArduino()
        {
            try
            {
                string res = streamCoin.ReadLine();
                if (res != null && res.Trim() == "C")
                {
                    if (Time.time >= lastCoinTriggerTime + coinCooldown)
                    {
                        Debug.Log("Coin detected - Event ausgelöst!");
                        lastCoinTriggerTime = Time.time;
                        OnCoinDataReceived?.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning("Coin ignoriert (Cooldown aktiv)");
                    }
                }
            }
            catch (System.TimeoutException) {  }
            catch (System.Exception ex)
            {
                Debug.LogError("Fehler beim Lesen vom Coin Arduino: " + ex.Message);
            }
        }

        public void SendRotationCommand()
        {
            SendCommandToPort(streamCoin, "R", "Coin Arduino");
        }
        public void SendCommandToPort(SerialPort port, string command, string deviceName)
        {
            if (port != null && port.IsOpen)
            {
                try
                {
                    port.WriteLine(command);
                    port.BaseStream.Flush();
                    Debug.Log($"Command '{command}' sent to {deviceName}.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Fehler beim Senden an {deviceName}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Senden an {deviceName} fehlgeschlagen: Port nicht offen.");
            }
        }

        void OnDestroy()
        {
            ClosePort(streamCoin, "Coin");
            ClosePort(streamLight, "Light");
        }

        private void ClosePort(SerialPort port, string deviceName)
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
                Debug.Log($"Serial Port für {deviceName} geschlossen.");
            }
        }
    }
}