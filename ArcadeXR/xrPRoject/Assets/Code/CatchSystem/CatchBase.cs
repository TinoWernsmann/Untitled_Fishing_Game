using Core.Game.Entity;
using UnityEngine;
using Core.Game;
using Unity.VisualScripting;
using System.Collections;
using Game.FishingRod;

using Game.FishingRod.Minigame;
using Gameworld.Pond;

namespace Core.Catching
{
    /// <summary>
    /// This class serves as the base class of all catchable objects.
    /// Each catchable object override the OnObjectCaught function to determine what happens.
    /// </summary>
    public abstract class CatchBase : GameObjectBase, IFishable
    {
        protected CatchData _catchData;
        protected bool isAttachedToBuoy = false;

        [Header("Reel In Params Base")]

        /// value between 0 and 1 since when a catch is valid
        [SerializeField] private float reelDistanceMax = 0.5f;

        //reference needed for removing fish on destroy
        protected FishingBuoy currentBait;

        private Pond parentPond = null;

        public void SetPondReference(Pond pondIn)
        {
            parentPond = pondIn;
        }



        /// <summary>
        /// Plays the sequence of what happens on object catch.
        /// It destroys the game object.
        /// </summary>
        public void Catch()
        {
            OnObjectCaught();
            NotifyParentForCatch();
            Destroy(gameObject);
        }

        public void NotifyParentForCatch()
        {
            if(parentPond != null)
            {
                parentPond.AddCatchDataPendingToProcess(_catchData);
            }
        }

        /// <summary>
        /// Sets the data of the catch object
        /// </summary>
        /// <param name="catchData">Data of the catch object</param>
        public void SetData(CatchData catchData)
        {
            _catchData = catchData;
        }

        /// <summary>
        /// Determines what happens when the object is caught.
        /// Overwritten in each object class.
        /// </summary>
        protected virtual void OnObjectCaught()
        {
            
        }

        public CatchData GetData() => _catchData;


        public Vector3 GetPosition()
        {
            return gameObject.transform.position;
        }

        public float Dist2(Vector3 pos)
        {
            Vector3 ab = GetPosition() - pos;
            float distance = Vector3.Dot(ab, ab); //quadrierte distanz = self dot product. (schneller als wurzel)
            return distance;
        }

        public void MarkAttachedToBuoy(bool flag)
        {
            isAttachedToBuoy = flag;
            DetachFromParent(); //optional happens in buoy too.
        }

        public float MaxDistanceCatch()
        {
            return reelDistanceMax;
        }

        public IEnumerator DestroyRoutine(float time)
        {
            yield return new WaitForSeconds(Unity.Mathematics.math.abs(time));
            if (currentBait != null)
            {
                currentBait.RemoveReference(this);
            }




            Destroy(gameObject);

        }

        public virtual void NotifyCatchAttemptFailed()
        {
            //empty
        }

        public virtual void NotifyMiniGameStarted()
        {
            //empty
        }

        public virtual void NotifyMiniGameFinished(bool sucessFull)
        {
            //empty
        }


        protected EFishState state = EFishState.EDefault;
        public bool IsCaught()
        {
            return state == EFishState.ECaught;
        }

    }
}

