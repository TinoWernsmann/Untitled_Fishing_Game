using UnityEngine;
using Core.Catching;


namespace Game.FishingRod.transfer //use this to have include paths like in cpp!, includes all classes!
{
    /// <summary>
    /// holds info about fishing rod and buoy in one place.
    /// </summary>
    public class FBuoyFlags
    {
        private bool isReeledIn = true;
        private bool isBuoyThrown = false;
        private FishingBuoy buoyRef = null;

        public FishingBuoy GetBuoy()
        {
            return buoyRef;
        }

        public int GetBuoyDepthLevel()
        {
            if (!buoyRef)
            {
                return 1;
            }
            return buoyRef.DepthLevel();
        }

        public void Update(FishingBuoy buoy)
        {
            buoyRef = buoy;
            if (buoyRef != null)
            {
                isBuoyThrown = buoyRef.IsThrown();
            }
            else
            {
                isBuoyThrown = false;
            }
        }

        public void SetReeledIn(bool flag)
        {
            isReeledIn = flag;
        }

        public Vector3 GetPosition()
        {
            if(buoyRef != null)
            {
                return buoyRef.GetPosition();
            }
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        public bool RodIsReeledIn()
        {
            return isReeledIn;
        }

        /*public bool BuoyHasTargetFish()
        {
            return !buoyRef.HasTargetFish() && buoyRef.IsGroundedFlag();
        }*/


        public bool BuoyIsReadyToCatch()
        {
            if (buoyRef != null)
            {
                return
                isBuoyThrown &&
                !RodIsReeledIn() &&
                !buoyRef.HasCatchableAttached() &&
                !buoyRef.HasTargetFish() &&
                buoyRef.IsGrounded();
            }
            return false;
        }

        

        //notify boy for fish attach
        public void NotifyBuoyFishAttached(CatchBase any)
        {
            Debug.Log("try attach fish");
            if(buoyRef != null)
            {
                buoyRef.AttachCatchable(any);
            }
        }
    }
}