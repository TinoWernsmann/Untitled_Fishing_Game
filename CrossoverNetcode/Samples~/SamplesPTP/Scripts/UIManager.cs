using UnityEngine;
using UnityEngine.UI;

namespace MixedRealityArcade.CrossoverNetcode.Samples
{
    [RequireComponent(typeof(NetworkPeerBehavior))]
    public class UIManager : MonoBehaviour
    {
        private const string LoopbackIP = "127.0.0.1";
        
        [SerializeField] private GameObject testLocallyToggle;
        private Toggle _testLocallyToggle;

        [SerializeField] private GameObject remoteIPInput;
        private InputField _remoteIPInput;

        [SerializeField] private GameObject listenPortInput;
        private InputField _listenPortInput;
        
        [SerializeField] private GameObject remotePortInput;
        private InputField _remotePortInput;
        
        [SerializeField] private GameObject listenButton;
        private Button _listenButton;
        
        [SerializeField] private GameObject connectButton;
        private Button _connectButton;
        
        [SerializeField] private GameObject sendButton;
        private Button _sendButton;
        
        [SerializeField] private GameObject disconnectButton;
        private Button _disconnectButton;
        
        [SerializeField] private CrossoverEvent crossoverEvent;
        
        private NetworkPeerBehavior _networkPeerBehavior;
        
        private void Awake()
        {
            _testLocallyToggle = testLocallyToggle.GetComponent<Toggle>();
            _remoteIPInput = remoteIPInput.GetComponent<InputField>();
            _listenPortInput = listenPortInput.GetComponent<InputField>();
            _remotePortInput = remotePortInput.GetComponent<InputField>();
            _listenButton = listenButton.GetComponent<Button>();
            _connectButton = connectButton.GetComponent<Button>();
            _sendButton = sendButton.GetComponent<Button>();
            _disconnectButton = disconnectButton.GetComponent<Button>();
            
            _networkPeerBehavior = GetComponent<NetworkPeerBehavior>();

            _networkPeerBehavior.Disconnected += OnDisconnect;
            _networkPeerBehavior.Connected += OnConnect;
            _networkPeerBehavior.CrossoverEventReceived += OnEventReceived;
        }

        public void OnTestLocallyToggleChanged(bool value)
        {
            if (value) _remoteIPInput.text = LoopbackIP;
            _remoteIPInput.interactable = !value;
            _listenPortInput.interactable = !value;
            _remotePortInput.interactable = !value;
            _listenButton.interactable = !value;
            
            ushort.TryParse(_remotePortInput.text, out ushort remotePort);
            _networkPeerBehavior.remotePort = value ? (ushort)7777 : remotePort;
        }

        public void OnRemoteIPInputChanged(string value)
        {
            _networkPeerBehavior.remoteAddress = value;
        }

        public void OnListenPortInputChanged(string value)
        {
            ushort.TryParse(_listenPortInput.text, out ushort listenPort);
            _networkPeerBehavior.listenPort = listenPort;
        }

        public void OnRemotePortInputChanged(string value)
        {
            ushort.TryParse(_remotePortInput.text, out ushort remotePort);
            _networkPeerBehavior.remotePort = remotePort;
        }

        public void OnConnectButtonClicked()
        {
            _networkPeerBehavior.TryConnect();
        }

        public void OnDisconnectButtonClicked()
        {
            _networkPeerBehavior.Disconnect();
        }

        public void OnSendButtonClicked()
        {
            _networkPeerBehavior.TrySendCrossoverEvent(crossoverEvent);
        }

        private void OnDisconnect()
        {
            SetConnectedState(false);
        }

        private void OnConnect()
        {
            SetConnectedState(true);
        }

        private void OnEventReceived(CrossoverEvent crossoverEvent)
        {
            Debug.Log(crossoverEvent);
        }

        private void SetConnectedState(bool isConnected)
        {
            _testLocallyToggle.interactable = !isConnected;
            if (!isConnected)
            {
                OnTestLocallyToggleChanged(_testLocallyToggle.isOn);
            }
            else
            {
                _remoteIPInput.interactable = false;
                _listenPortInput.interactable = false;
                _remotePortInput.interactable = false;
                _listenButton.interactable = false;
            }
            
            _connectButton.interactable = !isConnected;
            _disconnectButton.interactable = isConnected;
            _sendButton.interactable = isConnected;
        }
    }
}
