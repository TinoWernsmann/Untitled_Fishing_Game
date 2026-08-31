using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Splines;

namespace Core.Catching
{
    public class RotationPattern : MovementPattern
    {
        private bool bHasTarget = false;

        public RotationPattern(GameObject owner) : base(null)
        {
            ownerPtr = owner;
            CopyStartLocation();
            CopyStartRotation();
            reachedEnd = true;
            spline = new util.SplineBase(new GameObject());
            timeMax = 1.0f;
            lineDebug = owner.GetComponent<LineRenderer>();
            timeMax = 1.0f;
        }

        public void UpdateTarget(Vector3 target)
        {
            ResetWithRotationUpdate(ownerPtr.transform.rotation);
            float minDist = 1.0f;
            float maxDist = 2.0f;



            target = ownerPtr.transform.InverseTransformPoint(target);
            Vector3 p0 = new Vector3(0, 0, 0);
            Vector3 dir = (target - p0).normalized;
            float dist = dir.magnitude;
            float s = math.clamp(dist, minDist, maxDist);

            target = p0 + dir * s;

            
            

            float tangentScale = 0.3f;

            Vector3 p1 = p0 + Vector3.forward * tangentScale;
            Vector3 p3 = target;
            Vector3 p2 = target - dir * tangentScale;

            Debug.Log("RotationMade Spline " + p0 + " " + p1 + " " + p2 + " " + p3);
            
            spline.ClearSpline();
            spline.AddSegment(p0, p1, p2, p3);
            
            bHasTarget = true;

            if (lineDebug)
            {
                //spline.UpdateRenderForLineRenderComponent(lineDebug);
            }
        }

        public void ResetTarget()
        {
            bHasTarget = false;
            Debug.Log("Rotation RESET TARGET");
        }

        public bool HasTarget()
        {
            return bHasTarget;
        }
        
        public override void Reset()
        {
            base.Reset();
            ResetTarget();
        }


    }
}