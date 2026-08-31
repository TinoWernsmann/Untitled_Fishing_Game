using UnityEngine;
using Core.Game;
using Core.Game.Entity;
using System.Collections.Generic;
using Core.Catching;
using Game.FishingRod.Minigame;
using Game.FishingRod.transfer;
using Game.Player;


namespace Game.FishingRod //use this to have include paths like in cpp!, includes all classes!
{

    /// <summary>
    /// class will allow to switch the fishing buoy lure by depth level
    /// - the type bait / level is attached as scene object (inside buoy scene tree)
    /// - the updates are changed automatically
    /// - the lure can be disabled
    /// </summary>
    public class FishingBuoyLureSwitcher : MonoBehaviour
    {
        [SerializeField] public List<GameObject> luresInScene = new List<GameObject>();




        public void ShowLure(bool show)
        {
            Debug.Log("SHOW LURE " + show);
            Show(gameObject, show);
        }


        public void Update()
        {
            AutoUpdateLure();
        }


        public void AutoUpdateLure()
        {
            FishingRodParameters parameters = FishingRodParameters.Instance;
            if (parameters != null)
            {
                SetLure(parameters.DepthLevelAsIndex());
            }
        }


        public void SetLure(int targetIndex)
        {
            if (targetIndex >= 0 && targetIndex < luresInScene.Count)
            {
                for (int i = 0; i < luresInScene.Count; i++)
                {
                    bool visible = i == targetIndex;
                    Show(luresInScene[i], visible);
                }
            }
        }

        private void Show(GameObject other, bool show)
        {
            if (other)
            {
                other.SetActive(show);
            }
        }






    }
}