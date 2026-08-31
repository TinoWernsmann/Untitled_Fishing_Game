using System;
using UnityEngine;
using MixedRealityArcade.CrossoverNetcode;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;
using UnityEngine.SocialPlatforms.Impl;

namespace MixedRealityArcade.ArcadeXR.Network
{
    [RequireComponent(typeof(NetworkPeerBehavior))]
    public class NetworkManager : MonoBehaviour
    {

        private NetworkPeerBehavior peerBehavior;
        private ConfigManager cm;
        public TMPro.TextMeshProUGUI t;

        public event Action<string> SimpleEventReceived;
        public event Action Connected;

        public event Action<int> ScoreEventReceived;
        public event Action OnLayerUpgraded;
        public event Action<SimpleEvent> OnAnyEvent;

        public event Action OnStart;

        public static NetworkManager Instance { get; private set; }

        public static bool HasInstance()
        {
            return NetworkManager.Instance != null;
        }



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            peerBehavior = GetComponent<NetworkPeerBehavior>();
            cm = GetComponent<ConfigManager>();
            peerBehavior.Connected += OnConnect;

            peerBehavior.CrossoverEventReceived += HandleCrossoverEvent;
        }

        private void Start()
        {
             Debug.Log("Trying to Connect to Host...");
             NetworkConfig config = cm.LoadConfig();
             //peerBehavior.listenPort = config.listenPort;
             //peerBehavior.remoteAddress = config.targetAddress;
             //peerBehavior.remotePort = config.remotePort;
             TryConnect(config);
            var res =  peerBehavior.TryConnect();
                Debug.Log($"Connection result: {res}");
                if (!res)
                {
                    Debug.LogError("Failed to connect to host. Please check the network configuration.");
                }
                else
                {
                    Debug.Log("Successfully connected to host.");
                }
        }
        public bool TryConnect(NetworkConfig config)
        {
            peerBehavior.Disconnect();
            Debug.Log($"Trying to Connect to Host...{config.targetAddress}:{config.remotePort} from Port {config.listenPort}");
            peerBehavior.listenPort = config.listenPort;
            peerBehavior.remoteAddress = config.targetAddress;
            peerBehavior.remotePort = config.remotePort;
            bool res = peerBehavior.TryConnect();
            return res;
        }
        private void HandleCrossoverEvent(CrossoverEvent crossoverEvent)
        {
            if (crossoverEvent is SimpleEvent)
            {
                HandleSimpleEvent(crossoverEvent as SimpleEvent);
            }
            if (crossoverEvent is ModeTransitionEvent)
            {

            }
            if (crossoverEvent is DispenserScreenEvent)
            {
                HandleDispenserEvent(crossoverEvent as DispenserScreenEvent);
            }
            if (crossoverEvent is ScoreEvent)
            {
                HandleScoreEvent(crossoverEvent as ScoreEvent);
            }
        }

        public void HandleSimpleEvent(SimpleEvent simpleEvent)
        {
            Debug.Log($"Simple Event Received: {simpleEvent.Payload}");
            if (simpleEvent.Payload == "introComplete")
            {
                return;
            }
            if (simpleEvent.Payload == "calibrationDialogueComplete")
            {

            }
            if (simpleEvent.Payload == "arcadeComplete")
            {
                //PrototypeManager.AllItemsDispensed?.Invoke();
            }
            if (simpleEvent.Payload == "fromDisplayTest")
            {
                t.text = "Display OK";
                Debug.Log("Display OK");
            }
            if (simpleEvent.Payload == "LayerUpgrade")
            {
                OnLayerUpgraded?.Invoke();
            }
            if (simpleEvent.Payload == "GameStart")
            {
                OnStart?.Invoke();
            }
            //dispatch all to game manager -> game handle -> parsing handled inside fishing rod params
            OnAnyEvent?.Invoke(simpleEvent);
        }

        private void HandleScoreEvent(ScoreEvent scoreEvent)
        {
            ScoreEventReceived?.Invoke(scoreEvent.Score);
        }

        private void HandleDispenserEvent(DispenserScreenEvent dispenserEvent){
            //Debug.Log($"Dispenser screen event recieved\nItem name: {dispenserEvent.ItemName}\nPosition: {dispenserEvent.RelativeScreenSpacePosition}");
            //dispenser.DispenseGunPart(dispenserEvent.ItemName, dispenserEvent.RelativeScreenSpacePosition);
        }


        private void OnCalibrationComplete()
        {
            //peerBehavior.TrySendCrossoverEvent(calibrationComplete);
        }

        private void OnMRComplete()
        {
            SimpleEvent transitionEvent = ScriptableObject.CreateInstance<SimpleEvent>();
            transitionEvent.Payload = "transitionVR";
            //peerBehavior.TrySendCrossoverEvent(mrComplete);
        }

        private void OnHandgunPlaced()
        {
            SimpleEvent transitionEvent = ScriptableObject.CreateInstance<SimpleEvent>();
            transitionEvent.Payload = "transitionReturn";
            peerBehavior.TrySendCrossoverEvent(transitionEvent);
        }
        private void OnConnect()
        {
            Debug.Log("Connected!");
            Connected?.Invoke();
        }


        private void OnDestroy()
        {
            // PrototypeManager.QRCodeDetected -= OnCalibrationComplete;
            // PrototypeManager.VRTransitionCompleted -= OnMRComplete;
            // PrototypeManager.AllItemsCollected -= OnGunAssembled;
            // PrototypeManager.HandGunPlacedPodium -= OnHandgunPlaced;

            peerBehavior.CrossoverEventReceived -= HandleCrossoverEvent;
        }
        public void SendCrossoverEvent(CrossoverEvent se)
        {
            bool erfolg = peerBehavior.TrySendCrossoverEvent(se);
            Debug.Log(erfolg);
        }

        public void SendScoreEvent(int scoreToSent)
        {
            var scoreEvent = ScriptableObject.CreateInstance<ScoreEvent>();
            scoreEvent.SetScore(scoreToSent);
            SendCrossoverEvent(scoreEvent);
        }

        public void test()
        {

            var simpleEvent = ScriptableObject.CreateInstance<SimpleEvent>();
            simpleEvent.Payload = "testMe";
            SendCrossoverEvent(simpleEvent);

    }

        public void ScoreTest()
        {
            var scoreEvent = ScriptableObject.CreateInstance<ScoreEvent>();
            scoreEvent.SetScore(69);
            SendCrossoverEvent(scoreEvent);
        }

        public void SaveConfig(NetworkConfig nc)
        {
            cm.SaveConfig(nc);
        }
    }
}
