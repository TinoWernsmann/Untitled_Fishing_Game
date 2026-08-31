
using UnityEngine;

using UnityEngine.InputSystem;

using Player.FirstPerson;
using Core.Game;
using Core.Game.Entity;
using UnityEngine.Splines;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using util;



namespace Game.Fishing //use this to have include paths like in cpp!, includes all classes!
{

    public class FishingLine : EntityBase
    {

        //public SplineContainer splineContainer;
        public int resolution = 100;


        private SplineBase spline;



        private LineRenderer line;

        public Material lineMaterial;
        public float lineWidth = 0.02f;

        private Raycaster caster = null;

        protected override void Start()
        {
            spline = new SplineBase(gameObject);
            //splineContainer = gameObject.AddComponent<SplineContainer>();
            caster = new Raycaster();
            line = gameObject.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;

        }

        public void ResetColor()
        {
            SetColor(Color.black);
        }

        public void SetColorRed()
        {
            SetColor(Color.red);
        }
        
        public void SetColorGreen()
        {
            SetColor(Color.green);
        }

        
        public void SetColor(Color color)
        {
            line.startColor = color;
            line.endColor = color;

            // Optional Direct Override: Modifies the material property directly
            if (line.material != null){
                line.material.color = color; // Works for standard shaders with a _Color property
                
                // Enable glowing emission if the shader supports _EmissionColor
                if (line.material.HasProperty("_EmissionColor"))
                {
                    line.material.EnableKeyword("_EMISSION");
                    line.material.SetColor("_EmissionColor", color * 1.0f); // Multiply for intensity
                }
            }
        }



        public void Enable(bool flag)
        {
            if (line)
            {
                line.enabled = flag;
            }
            
        }



        void Update()
        {
            RebuildRender();
        }

        void RebuildRender()
        {
            if (line != null && spline != null)
            {
                spline.UpdateRenderForLineRenderComponent(line);

                /*List<Vector3> list = spline.RebuildSpline(resolution);
                line.positionCount = resolution;
                for(int i = 0; i < list.Count; i++)
                {
                    line.SetPosition(i, list[i]);
                }*/
            }

        }
        






        

        
        





        // -- abstract Positions, auto tangets

        public void RebuildMiddleProjected(
            Vector3 start,
            Vector3 end,
            float sizeTangentScalar,
            bool project
        )
        {   
            if(Vector3.Distance(start, end) < 0.1f)
            {
                return;
            }


            //AB = B - A
            //gx = A + t (B-A), t in [0,1]
            float t = 0.5f;
            Vector3 middle = start + t * (end - start);
            RebuildMiddleProjected(start, middle, end, sizeTangentScalar, project);
        }


        //middle point projected to ground
        public void RebuildMiddleProjected(
            Vector3 start,
            Vector3 middle,
            Vector3 end,
            float sizeTangentScalar,
            bool project
        )
        {
            float sizeray = 10.0f;
            if(caster == null)
            {
                caster = new Raycaster();
            }


            Vector3 projected = middle;
            if (project)
            {
                projected = caster.PerformRaycastHit(
                    middle,
                    new Vector3(0, -1, 0), //down
                    sizeray
                );
            }
            
            Rebuild(
                start,
                projected,
                end,
                sizeTangentScalar
            );
        


            
        }




        //rebuild from 3 points.
        public void Rebuild(
            Vector3 start,
            Vector3 middle,
            Vector3 end,
            float sizeTangentScalar
        )
        {
            List<Vector3> list = new List<Vector3> { start, middle, end };
            Rebuild(list, sizeTangentScalar);
        }


        



        public void Rebuild(
            List<Vector3> points,
            float sizeTangentScalar
        )
        {
            Rebuild(points, sizeTangentScalar, sizeTangentScalar);
        }
        public void Rebuild(
            List<Vector3> points,
            float sizeTangentInScalar,
            float sizeTangentOutScalar
        )
        {
            if(spline != null)
            {
                spline.ClearSpline();
                spline.Rebuild(points, sizeTangentInScalar, sizeTangentOutScalar);
            }
            
        }




        //line for rod start to tip
        public void RebuildWithOrthogonalExtraPoints(
            List<Transform> transforms,
            float sizeTangentScalar,
            float orthogonalPushBetweenPoints
        )
        {
            List<Vector3> points = new List<Vector3>();
            foreach (Transform m in transforms)
            {
                points.Add(m.position);
            }
            RebuildWithOrthogonalExtraPoints(points, sizeTangentScalar, orthogonalPushBetweenPoints);
        }




        public void RebuildWithOrthogonalExtraPoints(
            List<Vector3> points,
            float sizeTangentScalar,
            float orthogonalPushBetweenPoints
        )
        {
            //sollte so stimmen
            //side = up x dir
            //forthogonal = up x side * -1
            List<Vector3> pointsUpdate = new List<Vector3>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 current = points[i];

                Vector3 next = points[i + 1];
                Vector3 dir = (next - current); //AB = B - A 
                Vector3 side = Vector3.Cross(dir, Vector3.up);

                Vector3 orthogonal = Vector3.Cross(side, Vector3.up).normalized;

                Vector3 center = current + dir * 0.5f;
                Vector3 centerPushed = center + orthogonal * orthogonalPushBetweenPoints;
                
                
                


                //punkte doppelt um spline zu brechen.
                pointsUpdate.Add(current);
                pointsUpdate.Add(centerPushed);
                pointsUpdate.Add(next);
            }

            Rebuild(
                pointsUpdate,
                sizeTangentScalar,
                sizeTangentScalar
            );


        }










    }
}