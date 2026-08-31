using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Catching
{
    class CatchDataCollection
    {
        

        public CatchDataCollection()
        {
            
        }


        // Define your map (dictionary) instead of a list
        private Dictionary<int, List<CatchData>> layers = new Dictionary<int, List<CatchData>>();

        private List<CatchData> GetList(int depth)
        {
            // Check if the specific depth exists in the map
            if (layers.TryGetValue(depth, out List<CatchData> atIndex))
            {
                return atIndex;
            }
            else
            {
                layers[depth] = new List<CatchData>();
                return layers[depth];
            }
        }

        private List<CatchData> GetAnyListWithItems()
        {
            //von 0 nach x sortieren
            foreach (KeyValuePair<int, List<CatchData>> layer in layers.OrderBy(x => x.Key))
            {
                List<CatchData> catchList = layer.Value;
                if (catchList.Count > 0)
                {
                    return catchList;
                }
            }
            return GetList(1);
        }





        public void Add(CatchData data)
        {
            if (data != null)
            {
                List<CatchData> list = GetList(data.Layer);
                if (list != null)
                {
                    if (list.Contains(data) == false)
                    {
                        list.Add(data);
                    }
                }
            }
        }

        public void AddAll(CatchData[] fishData)
        {
            if (fishData != null)
            {
                foreach (CatchData current in fishData)
                {
                    Add(current);
                }
            }
        }

        /// <summary>
        /// a fish from the targeted layer is spawned
        ///
        /// - if the layer of fishes which can be spawned is empty, the first
        /// list with any items is used to spawn a fish.
        /// </summary>
        /// <param name="spawnPos"></param>
        /// <param name="spawnRotation"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public CatchBase CreateRandomFish(
            Vector3 spawnPos,
            Quaternion spawnRotation,
            int layer
        )
        {
            List<CatchData> list = GetList(layer);

            //create random if none found
            if (list.Count <= 0)
            {
                list = GetAnyListWithItems();
            }


            return CreateRandomFish(spawnPos, spawnRotation, list);
        }
        
        private CatchBase CreateRandomFish(
            Vector3 spawnPos,
            Quaternion spawnRotation,
            List<CatchData> list
        )
        {
            if (list != null)
            {
                if (list == null || list.Count == 0)
                {
                    return null;
                }

                int index = UnityEngine.Random.Range(0, list.Count);
                CatchData data = list[UnityEngine.Random.Range(0, list.Count)];
                //Debug.Log("Spawn Random Fish " + (index + 1) + " " + data.name + " of " + list.Count);


                if (data == null)
                {
                    return null;
                }

                CatchBase spawnedCatch = UnityEngine.Object.Instantiate(data.Prefab, spawnPos, spawnRotation);
                spawnedCatch.SetData(data);
                return spawnedCatch;
            }
            return null;
        }





    }

}