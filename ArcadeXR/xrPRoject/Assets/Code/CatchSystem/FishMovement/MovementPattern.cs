


using System;
using UnityEngine;
using util;

namespace Core.Catching
{
    public class MovementPattern : BaseLerp
    {
        protected LineRenderer lineDebug;
        protected SplineBase spline = null;
        
        protected GameObject ownerPtr = null;

        /// DO NOT CHANGE DO NOT REMOVE
        protected Vector3 startLocation;
        protected Quaternion startRotation;
        /// DO NOT CHANGE DO NOT REMOVE
        

        


        public MovementPattern(GameObject owner)
        {
            //Debug.Log("MovementPattern INIT");
            if (!owner)
            {
                return;
            }
            //Debug.Log("MovementPattern INIT B");
            BezierPrefabToolKit toolkit = owner.GetComponent<BezierPrefabToolKit>();
            if (toolkit != null)
            {
                timeMax = toolkit.AnimationTime();
                ownerPtr = owner;
                spline = toolkit.makeSplineLocal();
                //Debug.Log("MovementPattern INIT C");

                bool makeLine = false;
                if (makeLine && lineDebug == null) // != ?
                {
                    lineDebug = owner.AddComponent<LineRenderer>();
                    lineDebug.startWidth = 0.1f;
                    lineDebug.endWidth = 0.1f;
                    spline.UpdateRenderForLineRenderComponent(lineDebug);

                    lineDebug.gameObject.name = "FishMoveSpline"; // <-- Set the name here
                }

            }
            CopyStartLocation();
            CopyStartRotation();

            //Debug.Log("MovementPattern INIT D time: " + timeMax);
        }

        protected Vector3 PatternDirectionScaled()
        {
            return startRotation * spline.GlobalDirection();
        }
        public Vector3 PatternForward()
        {
            return PatternDirectionScaled().normalized;
        }

        public Quaternion GetStartRotation()
        {
            return startRotation;
        }

        public float SizeDirectionOfPattern()
        {
            return PatternDirectionScaled().magnitude;
        }

        public Vector3 LocalEnd()
        {
            Vector3 outPos;
            if (spline.Evaluate(1.0f, out outPos))
            {
                //Debug.Log("Local End Spline " + outPos);
                return outPos;
            }
            return outPos;
        }




        public override void Reset()
        {
            CopyStartLocation();
            reachedEnd = false;
            timeIntegrated = 0.0f;
        }

        

        public void ResetWithRotationUpdate(Quaternion rot)
        {
            startRotation = rot;
            Reset();
        }



        public bool IsValid()
        {
            return spline != null;
        }

        /// <summary>
        /// DO NOT CHANGE DO NOT REMOVE
        /// copies the location of the fish 
        /// to move the pattern towards its global
        /// location on begin interpolation 
        /// (which ensures the pattern is consistent and doesnt require inv transform of any kind
        /// and stays in world place: StartAnim + localBezier = WorldPos //during anim, until restarted)
        /// </summary>
        protected void CopyStartLocation()
        {
            startLocation = ownerPtr.transform.position;
        }


        /// <summary>
        /// DO NOT CHANGE DO NOT REMOVE
        /// copies the rotation of the fish 
        /// to rotate the pattern towards its global
        /// moving direction on begin interpolation
        /// </summary>
        public void CopyStartRotation()
        {
            startRotation = ownerPtr.transform.rotation; 
        }
        





        public Vector3 Tick(float deltatime)
        {
            if (!IsValid())
            {
                return new Vector3();
            }


            //integrate time and update flags
            BaseTick(deltatime);
            if (reachedEnd)
            {
                return ownerPtr.transform.position;
            }


            //Debug.Log("SCALAR " + t);

            //t *= 0.1f;
            return TickScalar(GetScalarT());
        }

        //for with events class
        
        
        public Quaternion Rotation()
        {
            if (reachedEnd)
            {
                return ownerPtr.transform.rotation;
            }
            return  spline.RotationAt(GetScalarT()) * startRotation;
        }


        


        
        

        

        
        

        protected Vector3 TickScalar(float t)
        {
            if (spline != null)
            {

                Vector3 outPoint = new Vector3();
                if (spline.Evaluate(t, out outPoint))
                {
                    //Debug.Log("Evaluate LOCAL OUT!" + outPoint);
                    //outPoint += transform.position;
                    //Debug.Log("Evaluate WORLD OUT!" + outPoint);

                    //M = T * R * p
                    return startLocation + startRotation * outPoint;
                }
            }
            return new Vector3();
        }

    };
}