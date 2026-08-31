using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Core.Catching;
using Game.FishingRod.transfer;
using UnityEngine;
using UnityEngine.Splines;

namespace util
{
    //will allow to extract bezier segments from (Anchor) and (support) points
    //which are extracted from a prefab attached to this script
    class BezierPrefabToolKit : MonoBehaviour
    {
        // must be setup.
        public GameObject splineScene = null;
        public float animationTime = 1.0f;

        public Boolean DegenerateCurve = false;

        //only used if having more than 4 points in a pattern segment
        public float tangentInScalar = 0.3f;
        public float tangentOutScalar = 0.3f;

        private GameObject splineSceneInstance;


        public void Awake()
        {

        }

        public float AnimationTime()
        {
            return animationTime;
        }

        public SplineBase makeSplineLocal()
        {
            Debug.Log("SPLINE BEZIER RUN");
            if (splineScene != null)
            {
                //instantiate scene and extract points
                splineSceneInstance = Instantiate(splineScene);
                splineSceneInstance.transform.position = new Vector3(0, 0, 0);
                splineSceneInstance.transform.rotation = new Quaternion();

                SplineBase spline = new SplineBase(splineSceneInstance);
                AppendSegmentsTo(spline, splineSceneInstance);
                spline.DeRotateSplineAlongZ();

                //hide
                //splineSceneInstance.transform.position = new Vector3(0, -10000.0f, 0);

                return spline;
            }
            return null;
        }


        private void AppendSegmentsTo(SplineBase spline, GameObject splineSceneInstance)
        {
            if (spline != null && splineSceneInstance != null)
            {
                Hide(splineSceneInstance);
                List<GameObject> childs = GetDirectChildren(splineSceneInstance);
                ProcessBezierSegments(childs, spline);
            }
        }
        
        private void ProcessBezierSegments(List<GameObject> childs, SplineBase spline)
        {
            for (int i = 0; i < childs.Count; i++)
            {
                GameObject current = childs[i];
                if (current != null)
                {
                    List<GameObject> innerChilds = GetDirectChildren(current);
                    Debug.Log("FOUND SEGMENTS INNER " + innerChilds.Count);
                    BezierSegment segment = new BezierSegment(innerChilds);
                    if (segment.IsValid())
                    {
                        segment.OverrideTangentScalars(tangentInScalar, tangentOutScalar);
                        spline.AddSegment(segment);
                        Debug.Log("FOUND SEGMENTS INNER " + innerChilds.Count + " MADE KNOTS" + spline.countKnots);

                    }
                    HideAll(innerChilds);
                }

            }
            if (DegenerateCurve)
            {
                spline.DegenerateCurve();
            }
        }


        private void HideAll(List<GameObject> childs)
        {
            Debug.Log("Hide all - bezier");
            foreach (GameObject c in childs)
            {
                Hide(c);
            }
        }

        private void Hide(GameObject target)
        {
            if (target == null) return;

            // Holt ALLE Renderer (MeshRenderer, SkinnedMeshRenderer, SpriteRenderer etc.) 
            // im Ziel-Objekt und allen darunter liegenden Kind-Objekten (true = inkl. inaktiven).
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length > 0)
            {
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = false;
                }
                Debug.Log($"Hide: {renderers.Length} Renderer auf/unter '{target.name}' deaktiviert.");
            }
            else
            {
                Debug.Log($"Hide: Keinen Renderer auf/unter '{target.name}' gefunden.");
            }
        }




        private List<GameObject> GetDirectChildren(GameObject extractFrom){
            List<GameObject> result = new List<GameObject>();
            if (extractFrom != null)
            {
                Transform parent = extractFrom.transform;
                for (int i = 0; i < parent.childCount; i++){
                    result.Add(parent.GetChild(i).gameObject);
                }
            }
            return result;
        }



    }



}