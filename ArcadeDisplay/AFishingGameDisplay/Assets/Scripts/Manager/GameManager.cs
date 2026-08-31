using MixedRealityArcade.ArcadeDisplay.Networking;
using MixedRealityArcade.CrossoverNetcode;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;
using UnityEngine;
using UnityEngine.SceneManagement;
using Manager.Score;
using TMPro;

namespace MixedRealityArcade.ArcadeDisplay
{
    /// <summary>
    /// The GameManager manages the overall state of the game in 2D and calls the necessary functions when transitioning between them.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private ScoreManager _score;
        public TextMeshProUGUI t;


        private void Awake()
        {
            DontDestroyOnLoad(this);          
            _score = GetComponent<ScoreManager>();
            //t.text = "yes";
        }
        private void Start()
        {
                // connect to networkManager
            NetworkManager.Instance.SimpleEventReceived += HandleSimpleEvent;
            NetworkManager.Instance.TransitionEventReceived += HandleTransitionEvent;
            NetworkManager.Instance.Connected += OnConnected;
            NetworkManager.Instance.ScoreEventReceived += HandleScoreEvent;
        }

        private void OnEnable(){
            // connect to scene management
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable(){
            // disconnect from scene management
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // disconnect from networkManager
            NetworkManager.Instance.SimpleEventReceived -= HandleSimpleEvent;
            NetworkManager.Instance.TransitionEventReceived -= HandleTransitionEvent;
            NetworkManager.Instance.Connected -= OnConnected;
            NetworkManager.Instance.ScoreEventReceived -= HandleScoreEvent;
        }

        // Callback Methods
        private void OnConnected(){
            Debug.Log("Connected!");
        }

        private void OnGameEnd()
        {
            Debug.Log("Game Ended!");
        }

        private void OnFishingEnd()
        {
            _score.EndGame();
        }

        // Crossover Event Handling
        private void HandleSimpleEvent(string eventPayload){
            switch (eventPayload){
                case "calibrationInitialize":
                   // menu.StartCalibration();
                    break;
                case "calibrationSuccess":
                    //menu.CompleteCalibration();
                   // arcadeCharacterController.PlayCalibrationCompleteDialogue();
                    break;
                case "gunAssemblyComplete":
                   // arcadeCharacterController.PlaySimpleDialogue(gunDialogue);
                    break;
                case "transitionVR":
                    ChangeScene("vr");
                    break;
                case "transitionReturn":
                    ChangeScene("return");
                    break;
                case "gameDone":
                    OnFishingEnd();
                    break;
                case "testMe":
                Debug.Log("Input Received");
                    t.text = "Input received";
                    break;
                    
            }
        }

        private void HandleTransitionEvent(ModeTransitionEvent transitionEvent){
            if (transitionEvent.fromMode == transitionEvent.toMode) return;
            switch (transitionEvent.toMode){
                case ModeTransitionEvent.Mode.VirtualReality:
                    ChangeScene("vr");
                    break;
                case ModeTransitionEvent.Mode.MixedReality:
                    ChangeScene("return");
                    break;
            }
        }

        private static void ChangeScene(string sceneName){
            switch(sceneName){
                case "game":
                    SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
                    break;
                case "vr":
                    SceneManager.LoadSceneAsync(2, LoadSceneMode.Single);
                    break;
                case "return":
                    SceneManager.LoadSceneAsync(3, LoadSceneMode.Single);
                    break;
                case "credits":
                    break;
            }
        }

        private void HandleScoreEvent(int score)
        {
            ScoreManager.Instance.SetCurrentScore(score);
        }

        // SceneManagement functions
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode){
            if(scene.buildIndex == 1){

                SubscribeToGameEvents();
                return;
            }
            if(scene.buildIndex == 2){
               
                OnGameEnd();
            }
        }

        private void SubscribeToGameEvents(){

        }

        public void ChangeToGameOverManually() 
        {
            SceneManager.LoadScene("GameOver");
        }

        public void test()
        {
            var simpleEvent = ScriptableObject.CreateInstance<SimpleEvent>();
            simpleEvent.Payload = "fromDisplayTest";
            NetworkManager.Instance.SendCrossoverEvent(simpleEvent);
        }

        public void TestUpgrade()
        {
            var simpleEvent = ScriptableObject.CreateInstance<SimpleEvent>();
            simpleEvent.Payload = "LayerUpgrade";
            NetworkManager.Instance.SendCrossoverEvent(simpleEvent);
        }
    }
}
