using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Catching
{
    /// <summary>
    /// Responsible for Instantiating catchable objects.
    /// Collects all available data (ScriptableObjects) that is available.
    /// FOLDER: Assets/Resources/ScriptableObjects/...
    /// 
    /// <<HOW TO ADD A FISH PREFAB TO THE CATCH FACTORY>>
    /// create the scriptable asset there with:
    /// Create -> Catch -> New catch Data
    /// 
    /// assign your prefab fish into the given field
    /// 
    /// give it a name and description, and a spawn layer
    /// 
    /// --> now the fish will be recognized by the catch factory!
    /// <<HOW TO ADD A FISH PREFAB TO THE CATCH FACTORY>>
    /// 
    /// Here, which data is used for Instantiation is random.
    /// </summary>
    public class CatchFactory : MonoBehaviour
    {
        public event Action OnDataLoaded;
        public bool DataLoaded { get; private set; }

        private CatchDataCollection collection = new CatchDataCollection();


        /// <summary>
        /// ---> TODO REFACTURE: Liste an Listen muss per Level
        /// gepsiechert werden (tiefen level aus catch data und nicht aktualisiert werden)
        /// alles am anfang laden, passt doch.
        /// </summary>
        private List<CatchData> _availableCatchData;
        //private List<CatchData> _fishCatchData;
        private const string FISH_DIR = "ScriptableObjects/Fish/";

        


        /// <summary>
        /// Collects all the available SO data available.
        /// </summary>
        private void Start()
        {
            _availableCatchData = new List<CatchData>();

            CatchData[] fishData = Resources.LoadAll<CatchData>(FISH_DIR);

            collection.AddAll(fishData);
            if (fishData.Length > 0)
            {
                DataLoaded = true;
                OnDataLoaded?.Invoke();
            }
            else{
                Debug.Log("Error Loading Catch Data!");
            }



            /*_fishCatchData = new List<CatchData>();

            foreach (CatchData fish in fishData)
            {
                _fishCatchData.Add(fish);
                _availableCatchData.Add(fish);
                Debug.Log("Fisch geladen: " + fish.name);
            }

            if (_availableCatchData.Count > 0)
            {
                DataLoaded = true;
                OnDataLoaded?.Invoke();
            }
            else
            {
                Debug.Log("Error Loading Catch Data!");
            }*/
        }

        public void RefreshCatchPool(int depth)
        {
            /*_fishCatchData.Clear();
            foreach (CatchData fish in _availableCatchData)
            {
                if (fish.Layer == depth)
                {
                    _fishCatchData.Add(fish);
                }
            }*/
        }

        

        public CatchBase CreateRandomFish(Vector3 spawnPos, Quaternion spawnRotation, int layer)
        {
            return collection.CreateRandomFish(spawnPos, spawnRotation, layer);
        }
    }
}