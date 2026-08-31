using UnityEngine;
using System.IO.Ports;
    public enum LightMode 
    {
        Solid = 0, 
        Blink = 1,
        Pulse = 2 
    }

    public class ArduinoManager : MonoBehaviour
    {
        public string portName_Coin = "COM4"; // Muss mit Arduino Port übereinstimmen, config
        public string portName_Light = "COM3";
        public int baudRate = 9600; // Muss mit der Baudrate im Arduino Sketch übereinstimmen

        private SerialPort streamCoin;
        private SerialPort streamLight;

    
        void Start()
        {


            //Lese Configs ob Portnamen vorhanden 
            streamCoin = new SerialPort(portName_Coin, baudRate);
            streamCoin.ReadTimeout = 50;
            Debug.Log("Serial Manager für Ardunio (Coin) initialisiert.");
            //Init Arduinio 2
            streamLight = new SerialPort(portName_Light,baudRate);
            streamLight.ReadTimeout = 50;
            Debug.Log("Serial Manager für Ardunio (Light) initialisiert.");


            try
            {
                streamCoin.Open();
                Debug.Log("Serial Port (Coin) geöffnet: " + portName_Coin);
                streamLight.Open();
                Debug.Log("Serial Port (Light) geöffnet: " + portName_Coin);

            }
            catch (System.Exception ex)
            {
                Debug.LogError("Fehler beim Öffnen des seriellen Ports: " + ex.Message);
            }
        }
        public void Update()
        {
            //Eventuell anpassen, SerialPort Event oder so....
            if (streamCoin != null && streamCoin.IsOpen)
            {
                if (streamCoin.BytesToRead > 0)
                {
                    ReadFromArduino();
                }
            }
            if (streamLight != null && streamLight.IsOpen)
            {
                if (streamLight.BytesToRead > 0)
                {
                    
                }
            }
        }
        // Soll kontinuierlich in Update() aufgerufen werden und auf ein 'C' warten
        public string ReadFromArduino()
        {
            if (streamCoin != null && streamCoin.IsOpen)
            {
                try
                {
                    string res = streamCoin.ReadLine();
                    if (res == "C")
                    {
                        Debug.Log("Coin detected");
                    }
                }
                catch (System.TimeoutException)
                {
                    return null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Fehler beim Lesen vom seriellen Port: " + ex.Message);
                    return null;
                }
            }
            return null;
        }
        public void SendRotationCommand()
        {
            string command = "R"; // Befehl zum Rotieren
            if (streamCoin != null && streamCoin.IsOpen)
            {
                try
                {
                    streamCoin.WriteLine(command);
                    streamCoin.BaseStream.Flush();
                    Debug.Log("Rotation command 'R' sent to Arduino.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Fehler beim Senden des Befehls: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("Serielle Verbindung ist nicht offen oder null.");
            }
        }

        void OnDestroy()
        {
            if (streamCoin != null && streamCoin.IsOpen)
            {
                streamCoin.Close();
                Debug.Log("Serial Port (Coin) geschlossen.");
            }
            if (streamLight != null && streamLight.IsOpen)
            {
                streamLight.Close();
                Debug.Log("Serial Port (Light) geschlossen.");
            }
        }
        public void SetLightConfiguration(LightMode mode, Color32 color, int speedOrDuration)
        {
            // Format: "Mode,R,G,B,Speed" z.B. "2,255,0,0,500"
            int modeInt = (int)mode;
            string payload = $"{modeInt},{color.r},{color.g},{color.b},{speedOrDuration}";
            
            SendLightCommand(payload);
        }

        public void SendLightCommand(string payload)
        {
            if (streamLight != null && streamLight.IsOpen)
            {
                try
                {
                    streamLight.WriteLine(payload);
                    streamLight.BaseStream.Flush(); 
                    Debug.Log("Light Payload gesendet: " + payload);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Fehler beim Senden des Befehls (Light): " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("Serielle Verbindung (Light) ist nicht offen oder null.");
            }
        }
    }

