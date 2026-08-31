using System;
using MixedRealityArcade.CrossoverNetcode;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;
using Manager.Networking;
using UnityEngine;

namespace MixedRealityArcade.ArcadeDisplay.Networking
{
    [RequireComponent(typeof(NetworkPeerBehavior))]
    public class NetworkManager : MonoBehaviour
    {
        private NetworkPeerBehavior peerComponent;
        private ConfigManager configManager;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> SimpleEventReceived;
        public event Action<ModeTransitionEvent> TransitionEventReceived;
        public event Action<int> ScoreEventReceived;

        public static NetworkManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return; 
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            peerComponent = GetComponent<NetworkPeerBehavior>();
            configManager = GetComponent<ConfigManager>();
        }

        private void Start()
        {
            if (configManager != null)
            {

                NetworkConfig config = configManager.LoadConfig();
                Debug.Log("Config geladen: " + config);
                
                peerComponent.listenPort = config.listenPort;
                peerComponent.remoteAddress = config.targetAddress;
                peerComponent.remotePort = config.remotePort;
                
                peerComponent.StartListenForConnections();
            }
            else
            {
                Debug.LogError("ConfigManager fehlt auf diesem GameObject!");
            }
        }

        private void OnEnable()
        {
            if (peerComponent != null)
            {
                peerComponent.Connected += OnConnected;
                peerComponent.Disconnected += OnDisconnected;
                peerComponent.CrossoverEventReceived += HandleCrossoverEvent;
            }
        }

        private void OnDisable()
        {
            if (peerComponent != null)
            {
                peerComponent.Connected -= OnConnected;
                peerComponent.Disconnected -= OnDisconnected;
                peerComponent.CrossoverEventReceived -= HandleCrossoverEvent;
            }
        }

        private void OnConnected()
        {
            Connected?.Invoke();
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke();
        }

        private void HandleCrossoverEvent(CrossoverEvent crossoverEvent)
        {
            switch (crossoverEvent)
            {
                case SimpleEvent simpleEvent:
                    SimpleEventReceived?.Invoke(simpleEvent.Payload);
                    break;
                case ModeTransitionEvent transitionEvent:
                    TransitionEventReceived?.Invoke(transitionEvent);
                    break;
                case ScoreEvent scoreEvent:
                    ScoreEventReceived?.Invoke(scoreEvent.Score);
                    break;            
            }
        }

        public void SendCrossoverEvent(CrossoverEvent crossoverEvent)
        {
            peerComponent.TrySendCrossoverEvent(crossoverEvent);
        }

        public void TestCE()
        {
            SimpleEvent coe = ScriptableObject.CreateInstance<SimpleEvent>();
            coe.Payload ="GameStart";
            SendCrossoverEvent(coe);
        }
    }
}