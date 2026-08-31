using System.Linq;
using System.Net;
using Manager.Networking;
using TMPro;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public ConfigManager conf;

    [Header("IP-Adresses")]
    public TMP_Text[] HMD_IP = new TMP_Text[4];
    public TMP_Text[] Arcade_IP = new TMP_Text[4];

    [Header("Ports")]
    public TMP_Text[] HMD_Port = new TMP_Text[4];
    public TMP_Text[] Arcade_Port = new TMP_Text[4];

    [Header("COM Ports")]
    public TMP_Text Light_Port; 
    public TMP_Text Coin_Port; 

    void Start()
    {
        if (conf == null)
        {
            conf = Object.FindFirstObjectByType<ConfigManager>();
        }

        if (conf)
        {
            string arcadeAddress = GetLocalIPv4();
            var res_net = conf.LoadConfig();
            var res_arduino = conf.LoadArduinoConfig();

            AssignIP(HMD_IP, res_net.targetAddress);
            AssignIP(Arcade_IP, arcadeAddress);

            // Zurück auf ushort
            AssignPort4Digits(HMD_Port, res_net.remotePort);
            AssignPort4Digits(Arcade_Port, res_net.listenPort);

            AssignComPort(Coin_Port, res_arduino.portCoin);
            AssignComPort(Light_Port, res_arduino.portLight);
        }
    }

    void AssignIP(TMP_Text[] blocks, string ip)
    {
        if (string.IsNullOrEmpty(ip) || blocks.Length != 4) return;
        
        string[] ip_cut = ip.Split('.');
        for (int i = 0; i < 4; i++)
        {
            blocks[i].text = (i < ip_cut.Length) ? ip_cut[i] : "0";
        }
    }

    void AssignPort4Digits(TMP_Text[] blocks, ushort port)
    {
        if (blocks.Length != 4) return;
        
        string portStr = port.ToString("D4"); 
        
        for (int i = 0; i < 4; i++)
        {
            blocks[i].text = (i < portStr.Length) ? portStr[i].ToString() : "0";
        }
    }

    void AssignComPort(TMP_Text block, string comStr)
    {
        if (string.IsNullOrEmpty(comStr)) return;
        block.text = comStr.Replace("COM", "").Replace("com", "").Trim();
    }

    public void SaveSettings()
    {
        if (!conf) return;

        conf.networkConfig.targetAddress = BuildIPString(HMD_IP);
        
        conf.networkConfig.remotePort = BuildPortUshort(HMD_Port);
        conf.networkConfig.listenPort = BuildPortUshort(Arcade_Port);

        conf.arduinoConfig.portCoin = "COM" + Coin_Port.text;
        conf.arduinoConfig.portLight = "COM" + Light_Port.text;

        conf.SaveNetworkConfig();
        conf.SaveArduinoConfig();

        Debug.Log("Einstellungen wurden gespeichert!");
    }

    private string BuildIPString(TMP_Text[] blocks)
    {
        return $"{blocks[0].text}.{blocks[1].text}.{blocks[2].text}.{blocks[3].text}";
    }
    private ushort BuildPortUshort(TMP_Text[] blocks)
    {
        string portStr = $"{blocks[0].text}{blocks[1].text}{blocks[2].text}{blocks[3].text}";
        ushort.TryParse(portStr, out ushort port);
        return port;
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}