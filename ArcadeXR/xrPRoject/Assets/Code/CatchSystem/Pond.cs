using System.Collections.Generic;
using Core.Catching;
using Core.Game;
using Core.Game.Entity;
using Game.FishingRod.transfer;
using UnityEngine;

/// <summary>
/// Spawn Area of all the catchables in the game.
/// Spawns catchables and checks if pond is empty.
/// </summary>
namespace Gameworld.Pond
{
    public class Pond : MonoBehaviour
    {
        [Header("Factory")]
        [SerializeField] private CatchFactory _factory;
        private List<CatchBase> _currentCatchables;
        //private int _currentPondLayer = 1;
        [SerializeField] private int _fishToSpawn = 8;
        [SerializeField] private float biteDistance = 1.5f;

        [SerializeField] private int maxFishAtATime = 1;

        //fish spawn delay
        private BaseLerp timer = null;
        private float timeMaxDelay = 5.0f;
        //fish spawn delay


        [Header("Spawn Area")]
        public Collider WaterBounds = null;
        private bool _hasResolvedWaterBounds;
        private Bounds _resolvedWaterBounds;

        //event called once a fish is caught 
        public event System.Action<CatchData> OnFished;

        private void Awake()
        {
            _currentCatchables = new List<CatchBase>();

            InitializeFactory();
            InitializeWaterBounds();

            ValidateConfiguration();
            DebugComponentFind();

            timer = new BaseLerp();
            ResetSpawnDelayTimer();
        }

        private void InitializeFactory()
        {
            if (_factory != null)
            {
                return;
            }
            _factory = GetComponent<CatchFactory>() ?? GetComponentInChildren<CatchFactory>() ?? Object.FindAnyObjectByType<CatchFactory>();
        }

        private void InitializeWaterBounds()
        {
            if (WaterBounds == null)
            {
                WaterBounds = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
            }
            _hasResolvedWaterBounds = TryResolveWaterBounds();
        }

        private void ValidateConfiguration()
        {
            if (_factory == null)
            {
                Debug.LogWarning("Pond has no CatchFactory assigned or found. Catchables will not spawn until a CatchFactory is available.");
                enabled = false;
                return;
            }
            if (!_hasResolvedWaterBounds)
            {
                Debug.LogWarning("Pond has no WaterBounds collider or renderer assigned. Fish will spawn at the Pond position.");
            }
        }

        public void ClearPond()
        {
            // Destroying each catchable may cause a lag spike!
            // TODO: Find better way to get rid of catchables (Scene change? Catchable Object Pool?)
            foreach (CatchBase catchable in _currentCatchables)
            {
                Destroy(catchable.gameObject);
            }
            _currentCatchables.Clear();
        }

        private void OnEnable()
        {
            //UpgradeManager.OnDepthUpgraded += HandleDepthUpgrade;
            LoadFish();
        }

        private void LoadFish()
        {
            if (_factory != null)
            {
                _factory.OnDataLoaded += LoadFinished;
                if (_factory.DataLoaded)
                {
                    LoadFinished();
                }
            }
        }

        private void HandleDepthUpgrade(int depth)
        {
            //_currentPondLayer = depth;
            /*_factory.RefreshCatchPool(_currentPondLayer);
            ClearPond();
            SpawnNewCatchables(_fishToSpawn);*/
        }

        private void OnDisable()
        {
            if (_factory != null)
            {
                _factory.OnDataLoaded -= LoadFinished;
            }
            //UpgradeManager.OnDepthUpgraded -= HandleDepthUpgrade;
        }

        private void LoadFinished()
        {
            //_factory.RefreshCatchPool(_currentPondLayer);
            /* 
            Die Game instace übermittelt die Buoy flags.
            Wenn die Buoye in der lage ist einen fisch zu fangen,
            nur dann wird ein fisch gespawnt.

            Niemand darf fische ohne die erlaubnis der buoye spawnen.
            Es gibt immer nur EINEN fisch!
            
            **/
            //SpawnNewCatchables(_fishToSpawn);
        }

        //a fish will notify the pond if its caught -> notify to 
        //observer (Game Manager)
        public void AddCatchDataPendingToProcess(CatchData data)
        {
            //parentPond.AddCatchDataPendingToProcess(_catchData);
            if (data != null)
            {
                OnFished?.Invoke(data);
            }

        }




        /// <summary>
        /// Spawns on random catchable object.
        /// </summary>
        private CatchBase SpawnRandomCatchable(int layer)
        {
            Vector3 spawnPos = GetSpawnPosition();
            CatchBase spawned = SpawnCatchable(spawnPos, layer);
            _currentCatchables.Add(spawned); //kein grund dazu grade.
            return spawned;
        }

        private CatchBase SpawnCatchable(Vector3 spawnPos, int layer)
        {
            CatchBase newCatchable = _factory.CreateRandomFish(spawnPos, Quaternion.identity, layer);

            if (newCatchable != null)
            {
                newCatchable.SetPondReference(this);
                


                if (newCatchable is Fish fish && _hasResolvedWaterBounds)
                {
                    fish.SetWaterBounds(_resolvedWaterBounds);
                }
            }
            return newCatchable;
        }




        private Vector3 GetSpawnPosition()
        {
            if (!_hasResolvedWaterBounds)
            {
                return transform.position;
            }

            Bounds bounds = _resolvedWaterBounds;
            float y = bounds.center.y;// Mathf.Approximately(bounds.size.y, 0f) ? bounds.center.y : Random.Range(bounds.min.y, bounds.max.y);

            Vector3 randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            return randomPosition;
        }

        private bool TryResolveWaterBounds()
        {
            if(!_hasResolvedWaterBounds)
            {
                if (WaterBounds != null)
                {
                    _resolvedWaterBounds = WaterBounds.bounds;
                    return true;
                }
                
                _resolvedWaterBounds = EntityBase.GetGameObjectBounds(this.gameObject);
                Debug.Log("Pond::ResolveWaterBounds_GameObject");
                return true;

            }

            return false;
        }

        /// <summary>
        /// Returns a random catchable object in the pond.
        /// Removes the determined catchable from the pond.
        /// </summary>
        /// <returns>The random catchable form the pond</returns>
        public CatchData CaughtCatchable()
        {
            if (_currentCatchables.Count <= 0) return null;

            /*// ---- WARUM IST DAS RANDOM???? ----
            CatchBase caught = _currentCatchables[Random.Range(0, _currentCatchables.Count)];
            CatchData caughtData = caught.GetData();
            caught.Catch();
            _currentCatchables.Remove(caught);

            CheckPondRespawn();

            return caughtData;*/
            return null;
        }

        /*/// <summary>
        /// Checks if the pond is empty and spawns new catchables if so.
        /// </summary>
        private void CheckPondRespawn()
        {
            if (_currentCatchables.Count <= 0)
            {
                Debug.Log("Teich ist leer! Spawne neue Catchable!");
                SpawnNewCatchables(_fishToSpawn);
            }
        }

        /// <summary>
        /// Spawns a given number of random catchables.
        /// </summary>
        /// <param name="numberOfCatchables">Number of catchables to spawn</param>
        private void SpawnNewCatchables(int numberOfCatchables)
        {
            for (int i = 0; i < numberOfCatchables; i++)
            {
                SpawnRandomCatchable();
            }
        }*/

        // attempt to catch a fish with the current FBuoyFlags
        //ready to catch -> try catch -> notfiy
        public void AttemptCatch(FBuoyFlags flags)
        {
            if (flags == null)
            {
                return;
            }
            if (!flags.BuoyIsReadyToCatch())
            {
                return;
            }
            if (!SpawnAllowedByTimer())
            {
                return;
            }

            //spawn a single fish to catch

            //if failed to catch: despawn fish

            CatchBase spawned = SpawnRandomCatchable(flags.GetBuoyDepthLevel());
            Fish casted = (Fish)spawned;
            if (casted != null)
            {
                //Debug.Log("FISHSPAWN - ReverseBuildTransformForSingleMovementPattern");
                casted.SetBuoyReference(flags.GetBuoy());
                casted.ReverseBuildTransformForSingleMovementPattern(flags);
            }

        }

        private bool SpawnAllowedByTimer()
        {
            timer.BaseTick(Time.deltaTime);
            if (timer.ReachedFlagRead())
            {
                ResetSpawnDelayTimer();
                return true;
            }
            return false;
        }

        private void ResetSpawnDelayTimer()
        {
            timer.ResetWithRandomTime(1.0f, timeMaxDelay);
        }









        // ------ WATER INTERACTION MANAGER COMPONENTS AND EXTRACTION ON BEGIN PLAY --------
        private Renderer waterRendererRef = null;
        private WaterInteractionManager waterInteractionManagerRef = null;


        public Transform FindWaterRendererTransform()
        {
            Renderer waterRender = FindWaterRenderer();
            if (waterRender)
            {
                return waterRender.gameObject.transform;
            }
            return null;
        }

        public Renderer FindWaterRenderer()
        {
            if(waterRendererRef == null)
            {
                WaterInteractionManager managerRef = FindInteractionManager();
                if(managerRef != null)
                {
                    waterRendererRef = managerRef.FindMeshRenderer();
                }
                
            }
            return waterRendererRef;
        }

        private WaterInteractionManager FindInteractionManager()
        {   
            if(waterInteractionManagerRef == null)
            {
                List<WaterInteractionManager> all = new List<WaterInteractionManager>();
                GameObjectBase.TGetComponentsInChildrenRecursive<WaterInteractionManager>(gameObject, all);
                if (all.Count > 0){
                    waterInteractionManagerRef = all[0];
                }
            }
            return waterInteractionManagerRef;

        }

        public void DebugComponentFind()
        {
            WaterInteractionManager found = FindInteractionManager();
            if (found)
            {
                //Debug.Log("FIND INTERACTION MANAGER SUCCESS!");

                Renderer waterRenderer = found.FindMeshRenderer();
                if (waterRenderer)
                {
                    //Debug.Log("FIND INTERACTION MANAGER WATER RENDERER SUCCESS!");
                }
                else
                {
                    Debug.Log("FIND INTERACTION MANAGER WATER RENDERER FAILED!");
                }


            }
            else
            {
                Debug.Log("FIND INTERACTION MANAGER FAILED!");
            }
        }




    }
}