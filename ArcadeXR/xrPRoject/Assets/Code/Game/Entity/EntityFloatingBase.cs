



////WaterInteractionManager.Instance.UpdateTransformSurfacePositionFromRender(transform);

using UnityEngine;

using UnityEngine.InputSystem;
using Core.Game;
using System.Linq;
using System.Collections.Generic;
using System;



namespace Core.Game.Entity
{
    /// <summary>
    /// base class for a floating entity on the water.
    /// </summary>
    public class EntityFloatingBase : EntityBase
    {
        protected Vector3 latestWaterGroundPosition;
        



        protected virtual void Update()
        {
            GroundUpdate();
        }


        void GroundUpdate()
        {
            MaterialHeightReaderTask task = new MaterialHeightReaderTask(transform.position, GroundUpdateEvent());
            ExecuteTask(task);
        }

        protected string GroundUpdateEvent(){
            return "GroundUpdate";
        }

        protected virtual void OnReceiveWaterHeight(MaterialHeightReaderTask task)
        {
            if (task.GetName().Equals(GroundUpdateEvent()))
            {
                latestWaterGroundPosition = task.result;
            }


            //Debug.Log("WATER HEIGHT FROM RENDER " + task.result.y);
        }

        /// <summary>
        /// call on Update:
        /// if using rigidbody physics, do not use this,
        /// use ApplyLatestWaterLocationVerticalOnly(true) instead!
        /// </summary>
        protected void ApplyLatestWaterLocation()
        {
            ApplyLatestWaterLocationVerticalOnly(false);
        }


        /// <summary>
        /// call on Update:
        /// Applies the water height to the transform.
        /// Use Vertical Only True, if using rigidbody transformations / impulses
        /// to avoid instability in movement!
        /// </summary>
        /// <param name="flag"></param>
        protected void ApplyLatestWaterLocationVerticalOnly(bool flag)
        {
            if (BlockApplyWaterHeight())
            {
                return;
            }
            /// ---- So lassen, nicht anfasse ----
            Vector3 currentLocationUpdate = transform.position;
            if (flag)
            {
                //stabiler für rigidbody transformation, kein direkter pos override!
                currentLocationUpdate.y = latestWaterGroundPosition.y;
            }
            else
            {
                currentLocationUpdate = latestWaterGroundPosition;
            }

            SetLocation(currentLocationUpdate);

        }
        
        /// <summary>
        /// might be blocked to allow a object to be below the water
        /// </summary>
        /// <returns></returns>
        protected virtual bool BlockApplyWaterHeight()
        {
            return false;
        }





        //may be called on tick
        protected void ProjectOntoWaterSurface(Vector3 pos)
        {
            MaterialHeightReaderTask task = new MaterialHeightReaderTask(pos, "ground");
            ExecuteTask(task);
        }


        protected void ExecuteTask(MaterialHeightReaderTask task)
        {
            WaterInteractionManager manager = WaterInteractionManager.Instance;
            if (manager)
            {
                manager.ExtractSurfacePositonFromRender(task);
                OnReceiveWaterHeight(task);
            }
        }


        public override void ApplyImpulse(Vector3 forceDir, float v)
        {
            base.ApplyImpulse(forceDir, v);

            /*if (IsWatered())
            {
                forceDir.y = 0.0f;
                impulseDrag = forceDir.normalized * v;
            }*/
            
        }


        protected bool IsWatered()
        {
            return InRangeVertical(0.2f, latestWaterGroundPosition);
        }
        

        protected bool InRangeVertical(float range, Vector3 hitpos)
        {
            Vector3 ownPos = gameObject.transform.position;
            return hitpos.y + Mathf.Abs(range) >= ownPos.y;
        }



    }


}