


using System;
using UnityEngine;
using Core.Catching;
using util.Quat;
using Core.Game;

namespace Game.FishingRod //use this to have include paths like in cpp!, includes all classes!
{
    public class DebugRadius : GameObjectBase
    {



        public static DebugRadius MakeInstance(GameObject prefab)
        {
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                if (instance != null)
                {
                    return instance.GetComponent<DebugRadius>();
                }
            }
            return null;
        }
        
        public void Destroy()
        {
            Destroy(gameObject);
        }

        
        


    }

};