using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Linq;

namespace MixedRealityArcade.ArcadeXR.Network
{
    public class FormularController : MonoBehaviour
    {

        [Header("Input Fields (Alle einzeln zuweisen!)")]
        [SerializeField] private TMP_InputField ip0Field;
        [SerializeField] private TMP_InputField ip1Field;
        [SerializeField] private TMP_InputField ip2Field;
        [SerializeField] private TMP_InputField ip3Field;
        [SerializeField] private TMP_InputField listenPortField;
        [SerializeField] private TMP_InputField remotePortField;

        [Header("Numpad Buttons (Alle einzeln zuweisen!)")]
        [SerializeField] private Button btn0;
        [SerializeField] private Button btn1;
        [SerializeField] private Button btn2;
        [SerializeField] private Button btn3;
        [SerializeField] private Button btn4;
        [SerializeField] private Button btn5;
        [SerializeField] private Button btn6;
        [SerializeField] private Button btn7;
        [SerializeField] private Button btn8;
        [SerializeField] private Button btn9;
        [SerializeField] private Button btnDelete;
        [SerializeField] private Button btnEnter;

        [Header("Sonstige UI Elemente")]
        [SerializeField] private Button connectButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI ipText;

        private int activeFieldIndex = 0;
        private TMP_InputField[] inputFields;

        void Start()
        {
            NetworkManager.Instance.Connected += OnConnect;
        }


        void OnEnable()
        {
            // Array manuell aus den Brute-Force-Variablen befüllen
            inputFields = new TMP_InputField[] {
                ip0Field, ip1Field, ip2Field, ip3Field, listenPortField, remotePortField
            };
            for (int i = 0; i < inputFields.Length; i++)
            {
                if (inputFields[i] == null)
                {
                    Debug.LogError($"FormularController: InputField an Index {i} wurde im Inspektor nicht zugewiesen!");
                    return;
                }
            }

            ipText.text = GetLocalIPv4();

            // Klick-Events für die Buttons registrieren (Direkte Übergabe des Wertes)
            SetupButton(btn0, "0");
            SetupButton(btn1, "1");
            SetupButton(btn2, "2");
            SetupButton(btn3, "3");
            SetupButton(btn4, "4");
            SetupButton(btn5, "5");
            SetupButton(btn6, "6");
            SetupButton(btn7, "7");
            SetupButton(btn8, "8");
            SetupButton(btn9, "9");
            SetupButton(btnDelete, "Delete");
            SetupButton(btnEnter, "Enter");

            // InputField-Auswahl-Listener registrieren
            for (int i = 0; i < inputFields.Length; i++)
            {
                int index = i;
                inputFields[i].onSelect.AddListener((string text) => SetActiveField(index));
            }

            if (connectButton != null)
            {
                connectButton.onClick.AddListener(HandleConnectButtonClick);
            }

            SetActiveField(0);
        }
        private void OnDisable()
        {
            for (int i = 0; i < inputFields.Length; i++)
            {
                inputFields[i].onSelect.RemoveAllListeners();
            }

            if (connectButton != null)
            {
                connectButton.onClick.RemoveAllListeners();
            }
        }
        private void OnConnect()
        {
            statusText.color = Color.green;
            statusText.text = "Verbunden!";

            // Formular ausblenden und Parent ausblen
            gameObject.SetActive(false);
            if (transform.parent != null)
            {
                transform.parent.gameObject.SetActive(false);
            }

        }

        private void SetupButton(Button btn, string value)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners(); 
                btn.onClick.AddListener(() => HandleButtonClick(value));
            }
            else
            {
                Debug.LogError($"FormularController: Button für '{value}' wurde im Inspektor nicht zugewiesen!");
            }
        }
        public string GetLocalIPv4()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
            f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
        }

        public void SetActiveField(int newIndex)
        {
            activeFieldIndex = newIndex;
            UpdateActiveFieldStyles();
        }

        void HandleButtonClick(string input)
        {
            if (input == "Enter")
            {
                activeFieldIndex = (activeFieldIndex + 1) % inputFields.Length;
                UpdateActiveFieldStyles();
                return;
            }

            if (input == "Delete")
            {
                string currentText = inputFields[activeFieldIndex].text;
                if (currentText.Length > 0)
                {
                    inputFields[activeFieldIndex].text = currentText.Substring(0, currentText.Length - 1);
                }
                if (string.IsNullOrEmpty(inputFields[activeFieldIndex].text))
                {
                    inputFields[activeFieldIndex].text = "0";
                }
                return;
            }

            if (int.TryParse(input, out int number))
            {
                if (inputFields[activeFieldIndex].text == "0")
                {
                    inputFields[activeFieldIndex].text = "";
                }

                int maxLength = (activeFieldIndex < 4) ? 3 : 4;

                if (inputFields[activeFieldIndex].text.Length < maxLength)
                {
                    inputFields[activeFieldIndex].text += input;
                }

                if (inputFields[activeFieldIndex].text.Length >= maxLength)
                {
                    if (activeFieldIndex != inputFields.Length - 1)
                    {
                        activeFieldIndex++;
                        UpdateActiveFieldStyles();
                    }
                    else
                    {
                        activeFieldIndex = 0;
                        UpdateActiveFieldStyles();
                    }
                }
            }
        }

        void HandleConnectButtonClick()
        {
            foreach (var field in inputFields)
            {
                if (string.IsNullOrEmpty(field.text))
                {
                    if (statusText != null) statusText.text = "Bitte alle Felder ausfüllen!";
                    return;
                }
            }

            NetworkConfig config = new NetworkConfig
            {
                targetAddress = $"{inputFields[0].text}.{inputFields[1].text}.{inputFields[2].text}.{inputFields[3].text}",
                listenPort = (ushort)int.Parse(inputFields[4].text),
                remotePort = (ushort)int.Parse(inputFields[5].text)
            };

            if (statusText != null) statusText.text = "Verbinde...";

            var res = NetworkManager.Instance.TryConnect(config);
            if (res)
            {
                if (statusText != null)
                {
                    statusText.text = "Suche nach Arcade...";


                    if (NetworkManager.Instance != null)
                    {
                        NetworkManager.Instance.SaveConfig(config);
                    }
                }
            }
            else
            {
                if (statusText != null) statusText.text = "Verbindung fehlgeschlagen!";
            }
        }

        private void UpdateActiveFieldStyles()
        {
            for (int i = 0; i < inputFields.Length; i++)
            {
                if (inputFields[i] == null || inputFields[i].image == null) continue;

                if (i == activeFieldIndex)
                {
                    inputFields[i].image.color = Color.yellow;
                    inputFields[i].ActivateInputField();
                }
                else
                {
                    inputFields[i].image.color = Color.white;
                }
            }
        }
    }
}