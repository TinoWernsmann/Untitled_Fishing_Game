using System.Security.Cryptography;
using Core.Game.Entity;
using Debugging.Keyboard;
using Game;
using Game.Fishing;
using Game.FishingRod.transfer;
using Gameworld.Pond;
using Manager.Timer;
using MixedRealityArcade.ArcadeXR.Network;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;
using Player.Points;
using UnityEngine;

namespace Core.Game
{
    //will manage the game loop, instancing of components and hold all (main?) references 
    // (player controllers, rod, pond prefab etc)
    //in on space
    public class GameHandle : MonoBehaviour
    {
        public bool isVr = false;

        // --- prefabs ---
        public GameObject playerPrefabPC;
        public GameObject playerPrefabVR;

        public GameObject fishingRodPrefab = null;

        public GameObject pondPrefab = null;

        public GameObject AmbientPlanePrefab = null;
        // --- prefabs ---


        private GameObject playerReference = null;
        private GameObject pondReference = null;
        private GameObject rodReference = null;



        public GameObject automatPointReference = null;
        public GameObject playerSpawnPointReference = null;


        //setup from game manager
        [SerializeField] private DebugKeyboard _debugInput;
        [SerializeField] private bool _disableNetwork;
        [SerializeField] private UpgradeManager _upgrader;
        [SerializeField] private TimeManager _timer;

        private PlayerPoints _playerPoints;

        //setup from game manager

        void FindPlayerPoints()
        {
            _playerPoints = GetComponent<PlayerPoints>();
        }

        void FindUpgrader()
        {
            _upgrader = GetComponent<UpgradeManager>();
        }



        public void Start()
        {

            //start the game

            //create player (vr / xr)

            CreatePlayer();
            PlayerLookAt(automatPointReference);
            CreateFishingRod();
            CreatePond();
            CreatePlayerCircleDecal();
            MakeRodParameters();
            MakeAmbientObjects();

            UpdateUiCanvasCamera();


            //game manager setup
            FindPlayerPoints();
            FindUpgrader();
            SetupPondConnectionToDebugger();
            SetupNetworkEvents();
        }

        private void SetupPondConnectionToDebugger()
        {
            Pond pond = GetPond();
            _debugInput.ConnectPond(pond);
        }

        private void SetupNetworkEvents()
        {
            _debugInput.OnFished += HandleFishing;
            if (_timer != null)
                _timer.OnTimerDone += HandleTimerDone;

            if(NetworkManager.HasInstance())
                NetworkManager.Instance.OnLayerUpgraded += HandleLayerUpgrade;

            RegisterToNetWorkEventsRaw(true);

            Pond pond = GetPond();
            if (pond != null)
            {
                pond.OnFished += HandleFishing;
            }

            DebugNetworkRodParamsOnce();
        }

        private void HandleLayerUpgrade()
        {
            _upgrader.UpgradeDepth();
        }

        /// <summary>
        /// Reaction to the OnFished event.
        /// Scores the Fish and sends the score to the Display to show it.
        /// </summary>
        /// <param name="caughtData">Data of the caught object</param>
        private void HandleFishing(CatchData caughtData)
        {
            Debug.Log("INVOKE HANDLE FISHING " + caughtData.GetScoreValue());
            Debug.Log(caughtData.name + caughtData.Layer);
            _playerPoints.HandleScoring(caughtData.GetScoreValue());
            try
            {
                bool wasCaught = caughtData.GetCatchedStatus();

                if (!_disableNetwork)
                {
                    var fishRarity = ScriptableObject.CreateInstance<SimpleEvent>();
                    fishRarity.Payload = "fish_" + caughtData.Layer.ToString();


                    if (NetworkManager.HasInstance())
                    {
                        NetworkManager.Instance.SendScoreEvent(_playerPoints.CurrentScore);
                        if(wasCaught)
                            NetworkManager.Instance.SendCrossoverEvent(fishRarity);
                    }

                    

                }
            }
            catch (System.Exception e)
            {
                Debug.Log("INVOKE HANDLE FISHING - NETWORK ISSUE");
            }

        }

        /// <summary>
        /// Reaction on the OnTimerDone event.
        /// Currently sends a simpleEvent to the Display to signal game is done.
        /// TODO: Implement VR timer done scene or visuals and make them appear here.
        /// </summary>
        private void HandleTimerDone()
        {
            // TODO: Make an End Screen for when the run is over.
           // GetPond().ClearPond();
            var doneEvent = ScriptableObject.CreateInstance<SimpleEvent>();
            doneEvent.Payload = "gameDone";
            if (NetworkManager.HasInstance())
            {
                NetworkManager.Instance.SendCrossoverEvent(doneEvent);
            }
                
        }

        private void OnDisable()
        {
            _debugInput.OnFished -= HandleFishing;
            if (_timer != null)
                _timer.OnTimerDone -= HandleTimerDone;

            Pond p = GetPond();
            if (p != null)
            {
                p.OnFished -= HandleFishing;
            }
            RegisterToNetWorkEventsRaw(false);
        }



        private void RegisterToNetWorkEventsRaw(bool flag)
        {
            if (NetworkManager.HasInstance())
            {
                if (flag)
                {
                    NetworkManager.Instance.OnAnyEvent += DispatchMessageToGameHandle;
                }
                else
                {
                    NetworkManager.Instance.OnAnyEvent -= DispatchMessageToGameHandle;
                }
            }
            
        }

        //dispatch der network events an game handle, für angel params
        public void DispatchMessageToGameHandle(SimpleEvent eventIn)
        {
            GameHandle handle = GameHandle.FindGameHandle();
            if (handle != null)
            {
                handle.DispatchMessage(eventIn);
            }
        }




        private void DebugNetworkRodParamsOnce()
        {
            FishingRodParameterDebugger debugger = new FishingRodParameterDebugger();
            debugger.Setup();
        }

        // game manager merge

        private void UpdateUiCanvasCamera()
        {
            CameraSetter[] cameraSetters = Object.FindObjectsByType<CameraSetter>();
            UpdateUiCanvasCamera(cameraSetters);
        }
        
        private void UpdateUiCanvasCamera(CameraSetter[] array)
        {
            for(int i = 0; i < array.Length; i++){
                UpdateUiCanvasCamera(array[i]);
            }
        }
        
        private void UpdateUiCanvasCamera(CameraSetter inSetter)
        {
            if(inSetter != null)
            {
                PlayerControllerBaseShared player = GetPlayer();
                if (player != null)
                {
                    Camera playerCam = player.FindCamera();
                    inSetter.SetCamera(playerCam);
                }
                
            }
        }


        

        public static GameHandle FindGameHandle()
        {
            GameHandle handler = Object.FindAnyObjectByType<GameHandle>();
            if (handler != null)
            {
                Debug.Log("GameHandle Found");
                return handler;
            }
            return null;
        }

        private void MakeRodParameters()
        {
            FishingRodParameters.MakeInstance();
        }
        
        private void DispatchPlayerCircleDecal()
        {
            PlayerControllerBaseShared player = GetPlayer();
            if (player)
            {
                FishingRod rod = GetPlayerFishingRod();
                if (rod != null)
                {
                    rod.DispatchPlayerCircleDecal(player.GetPlayerTargetCircle());
                }
            }
        }



        private void CreatePlayer()
        {
            GameObject spawned = Instantiate(PlayerToSpawn());
            playerReference = spawned;
            if (playerReference)
            {
                playerReference.transform.position = playerSpawnLocation();
            }
        }

        private void PlayerLookAt(GameObject objectIn)
        {
            PlayerControllerBaseShared player = GetPlayer();
            if (objectIn != null && player != null)
            {
                player.LookAt2D(objectIn);
            }
        }

        private Vector3 playerSpawnLocation()
        {
            if(playerSpawnPointReference != null)
            {
                Transform t = playerSpawnPointReference.transform;
                if (t)
                {
                    return t.position;
                }
            }
            return new Vector3(0, 0, 0);
        }



        //returns the selected player based on the given public boolean.
        private GameObject PlayerToSpawn()
        {
            #if !UNITY_EDITOR
                // Code, der NUR im fertigen Build läuft (z.B. Analytics, Savegame-Pfade, Performance-Logs)
                //Debug.Log("Ich laufe im fertigen Spiel!");
                return playerPrefabVR;
            #endif




            if (isVr)
            {
                //create vr player 
                return playerPrefabVR;
            }
            else
            {
                //create player
                return playerPrefabPC;
            }
        }

        private void CreateFishingRod()
        {
            ReCreateFishingRod(fishingRodPrefab);
        }
        
        private void ReCreateFishingRod(GameObject newRod)
        {
            if (newRod != null)
            {
                PlayerControllerBaseShared playerScript = GetPlayer();
                if(playerScript != null){
                    playerScript.RemoveRodReference();

                    if(rodReference != null){
                        Destroy(rodReference);
                    }
                    rodReference = Instantiate(newRod);
                    if (rodReference != null)
                    {
                        playerScript.ReplaceFishingRod(rodReference);
                    }
                }
            }
        }

        private void CreatePond()
        {
            if (pondReference != null)
            {
                Destroy(pondReference);
            }

            if (pondPrefab != null)
            {
                pondReference = Instantiate(pondPrefab);
                pondReference.transform.position = new Vector3(0, 0, 2);
            }
            else
            {
                Pond existing = Object.FindAnyObjectByType<Pond>();
                if (existing != null)
                {
                    pondReference = existing.gameObject;
                }
            }
        }

        private void CreatePlayerCircleDecal()
        {
            PlayerControllerBaseShared player = GetPlayer();
            Pond pond = GetPond();
            if (player && pond)
            {
                player.SetupPlayerCircleDecal(pond);
            }
        }

        private Pond GetPond()
        {
            if (pondReference)
            {
                return pondReference.GetComponent<Pond>();
            }
            return null;
        }

        public PlayerControllerBaseShared GetPlayer()
        {
            if (playerReference != null)
            {
                return playerReference.GetComponent<PlayerControllerBaseShared>(); //get base script.
            }
            return null;
        }

        private FishingRod GetPlayerFishingRod()
        {
            PlayerControllerBaseShared player = GetPlayer();
            if (player != null)
            {
                return player.GetFishingRod();
            }
            return null;
        }

        //game loop
        void Update()
        {
            UpdateBuoy();
            //debug
            DispatchPlayerCircleDecal();
        }


        void UpdateBuoy()
        {
            FishingRod rod = GetPlayerFishingRod();
            if (rod != null)
            {
                FBuoyFlags flags = rod.GetCurrentBuoyState();
                Pond pond = GetPond();
                if (flags != null && pond != null)
                {
                    pond.AttemptCatch(flags);
                }
            }
        }


        public void DispatchMessage(SimpleEvent eventIn)
        {
            if (eventIn != null)
            {
                MakeRodParameters();
                FishingRodParameters.Instance.ParseEventString(eventIn.Payload);
            }
        }
        

        private void MakeAmbientObjects()
        {
            if (pondReference)
            {
                Vector3 pos = pondReference.transform.position;
                float height = 30; //keep like this
                AmbientPlane.Make(AmbientPlanePrefab, pos, height);
            }
        }
    };   
}