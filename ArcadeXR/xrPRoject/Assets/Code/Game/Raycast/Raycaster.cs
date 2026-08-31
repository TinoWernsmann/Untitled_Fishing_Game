

using UnityEngine;

using UnityEngine.InputSystem;

/// <summary>
/// component to control a player with mouse and keyboard
/// </summary>
namespace Core.Game //use this to have include paths like in cpp!, includes all classes!
{
    public class Raycaster
    {
        public Raycaster()
        {

        }

        public bool PerformRaycast(
            Vector3 pos,
            Vector3 forward,
            float sizeRay,
            out GameObject outObject //& ref like in cpp for ref &, use out, or ptr.
        )
        {
            RaycastHit hit;
            if (Physics.Raycast(pos, forward, out hit, sizeRay))
            {
                Debug.Log(hit.collider.name);
                outObject = hit.collider.gameObject;
                return true;
            }
            outObject = null;
            return false;
        }



        //returns hit point or pos if no hit.
        public Vector3 PerformRaycastHit(
            Vector3 pos,
            Vector3 forward,
            float sizeRay
        )
        {
            RaycastHit hit;
            if (Physics.Raycast(pos, forward, out hit, sizeRay))
            {
                return hit.point;
            }
            return new Vector3();
        }


        public bool bPerformRaycastHit(
            Vector3 pos,
            Vector3 forward,
            float sizeRay,
            Collider[] ignoreCollider
        )
        {
            Vector3 posNone;
            return bPerformRaycastHit(pos, forward, sizeRay, ignoreCollider, out posNone);
        }
        
        public bool bPerformRaycastHit(
            Vector3 pos,
            Vector3 forward,
            float sizeRay,
            Collider[] ignoreCollider,
            out Vector3 outPos
        ) {
            RaycastHit[] hits = Physics.RaycastAll(pos, forward, sizeRay);
            foreach (RaycastHit hit in hits)
            {
                bool ignored = false;
                foreach (Collider c in ignoreCollider)
                {
                    if (hit.collider == c){
                        ignored = true;
                        break;
                    }
                }
                if (!ignored)
                {
                    outPos = hit.point;
                    return true;
                }
                    
            }
            outPos = new Vector3(0,0,0);
            return false;
        }





        public bool bPerformRaycastHit(
            Vector3 pos,
            Vector3 forward,
            float sizeRay,
            Collider ignoreCollider
        )
        {
            RaycastHit[] hits = Physics.RaycastAll(
                pos,
                forward,
                sizeRay
            );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == ignoreCollider)
                    continue;

                return true;
            }

            return false;
        }






    };
}


