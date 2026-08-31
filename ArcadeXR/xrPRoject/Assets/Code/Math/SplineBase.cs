using System;
using System.Collections.Generic;
using Core.Catching;
using Game.FishingRod.transfer;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using util.Quat;

namespace util
{
    /// <summary>
    /// allows to create C1 continous curves from a set of points.
    /// use The Method with 4 positions to create C0 continous splines
    /// with no in out tangents at start and end respectively
    /// 
    /// all positions are converted to LOCAL space of parent Game Object
    /// </summary>
    public class SplineBase
    {
        private Bounds bounds;
        private GameObject parentObj;
        private SplineContainer splineContainer;
        public int resolution = 100;
        public int countKnots = 0;

        public SplineBase(GameObject parent)
        {
            bounds = new Bounds();
            parentObj = parent;
            splineContainer = parentObj.AddComponent<SplineContainer>();
        }

        


        public void UpdateRenderForLineRenderComponent(LineRenderer renderer)
        {
            if (renderer != null)
            {
                List<Vector3> list = RebuildSpline(resolution);
                renderer.positionCount = resolution;
                for(int i = 0; i < list.Count; i++)
                {
                    renderer.SetPosition(i, list[i]);
                }
            }
        }

        /// <summary>
        /// local build spline of n positions
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<Vector3> RebuildSpline(int count)
        {
            List<Vector3> listOut = new List<Vector3>();

            if (splineContainer != null)
            {
                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)(resolution - 1);
                    /*Vector3 pos = splineContainer.EvaluatePosition(t);
                    if (IsValid(pos)){
                        listOut.Add(pos);
                    }*/
                    Vector3 pos = new Vector3();
                    if (Evaluate(t, out pos))
                    {
                        listOut.Add(pos);
                    }
                }
            }
            return listOut;
        }

        /// <summary>
        /// spline rebuild in world space / transform space
        /// </summary>
        /// <param name="count"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public List<Vector3> RebuildSplineWorld(int count, Transform transform)
        {
            List<Vector3> localSpace = RebuildSpline(count);
            for (int i = 0; i < localSpace.Count; i++)
            {
                localSpace[i] = transform.TransformPoint(localSpace[i]);
            }
            return localSpace;
        }

        /// <summary>
        /// evaluated spline point at t, outpoint is updated.
        /// Returns false if the spline has some other issue / pos contains nAn for example
        /// </summary>
        /// <param name="t"></param>
        /// <param name="outPoint"></param>
        /// <returns></returns>
        public bool Evaluate(float t, out Vector3 outPoint)
        {
            if (splineContainer != null)
            {
                //t = Math.Clamp(t, 0.0f, 1.0f);
                Vector3 pos = splineContainer.EvaluatePosition(t);
                if (IsValid(pos))
                {
                    outPoint = pos;
                    return true;
                }
            }
            outPoint = new Vector3();
            return false;
        }

        /// returns the rotation at a given t 
        /// caution: Rotation along Yaw Only, to pitch / roll
        public Quaternion RotationAt(float t)
        {
            float range = 0.1f;
            if(t >= 1.0f)
            {
                t = 1.0f - range;
            }

            Vector3 a = splineContainer.EvaluatePosition(t);
            Vector3 b = splineContainer.EvaluatePosition(t + range);
            return QuatHelper.MakeQuatFromDirection2D(b - a);
            //MakeQuatFromDirection(b - a);

        }
        
        private Quaternion MakeQuatFromDirection(Vector3 direction)
        {
            direction.y = 0.0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            return targetRotation;
        }



        public Vector3 GlobalDirection()
        {
            Vector3 a = new Vector3();
            Vector3 b = new Vector3();
            Evaluate(0.0f, out a);
            Evaluate(1.0f, out b);
            return b - a;
        }

        /// <summary>
        /// rotates the spline so the starting and ending points
        /// are along the z (forward) axis
        /// </summary>
        public void DeRotateSplineAlongZ()
        {
            Vector3 dir = GlobalDirection().normalized;
            //float dot = Vector3.Dot(Vector3.forward, GlobalDirection().normalized);
            float angle = math.atan2(dir.x, dir.z); //Z is forward, x side
            
            Debug.Log("Spline Derotation " + angle * Mathf.Rad2Deg + " from dir " + dir);
            RotateSplineFromAngle(-angle);
        }

        /// <summary>
        /// rotates all points and tangents with an angle in[-pi, pi], (yaw axis)
        /// </summary>
        /// <param name="angle"></param>
        public void RotateSplineFromAngle(float angle)
        {
            
            Quaternion rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
            RotateSpline(rotation);
        }

        public void RotateSpline(Quaternion rotation)
        {
            Spline spline = getSpline();
            if (spline != null)
            {
                if (spline.Count > 0)
                {
                    Vector3 pivot = spline[0].Position;
                    RotateSpline(rotation, pivot);

                    parentObj.transform.rotation *= rotation;
                }
            }
        }

        public void RotateSpline(Quaternion rotation, Vector3 pivot)
        {
            Spline spline = getSpline();
            if (spline != null)
            {
                for (int i = 0; i < spline.Count; i++)
                {
                    BezierKnot knot = spline[i];

                    // Position rotieren
                    Vector3 localPos = knot.Position; //AB = B - A
                    //Debug.Log("Spline Derotation " + localPos);
                    localPos = localPos - pivot; //T^-1

                    Vector3 rotateLocal = rotation * localPos;
                    Vector3 world = rotateLocal + pivot;
                    knot.Position = world;

                    //Debug.Log("Spline Derotation After " + knot.Position);

                    // Tangenten ebenfalls rotieren
                    knot.TangentIn = rotation * knot.TangentIn;
                    knot.TangentOut = rotation * knot.TangentOut;

                    //spline[i] = knot;
                    splineContainer.Spline.SetKnot(i, knot);
                }

            }
        }


        //Degenerates the curve to a 2 order spline / 1 degree lerp
        public void DegenerateCurve()
        {
            //aus tangenten müssen leere knots gebaut werden
            List<BezierKnot> copyKnots = CopyAll();
            List<BezierKnot> extendedList = new List<BezierKnot>();
            for (int i = 0; i < copyKnots.Count; i++)
            {
                BezierKnot current = copyKnots[i];

                //adding a point at the next tangent point 
                BezierKnot constructNext = ConstructKnotFromTangent(current);

                extendedList.Add(current);
                extendedList.Add(constructNext);
            }

            ClearSpline();
            AddAll(extendedList);
            ClearAllTangents();
        }
        
        //constructs a knot with no tangents
        private BezierKnot ConstructKnotFromTangent(BezierKnot other)
        {
            Vector3 pos = other.Position + other.TangentOut;

            BezierKnot knot = new BezierKnot(
                position: pos,
                tangentIn: new float3(0, 0, 0),
                tangentOut: new float3(0, 0, 0)
            );
            return knot;
        }





        private void ClearAllTangents()
        {
            for(int i = 0; i < Count(); i++)
            {
                BezierKnot current = GetKnot(i);

                current.TangentIn = new Vector3(0, 0, 0);
                current.TangentOut = new Vector3(0, 0, 0);

                SetKnot(current, i);
            }
        }






        public bool IsInideBounds(Vector3 world, Transform transform)
        {
            Vector3 local = transform.InverseTransformPoint(world);
            return IsInideBounds(local);
        }

        public bool IsInideBounds(Vector3 localPos){
            if(bounds != null)
            {
                //only xz bounds.
                Vector3 p = localPos;
                Vector3 c = bounds.center;
                Vector3 e = bounds.extents;
                return Mathf.Abs(p.x - c.x) <= e.x && Mathf.Abs(p.z - c.z) <= e.z;
            }
            return false;
        }
       

        


        private bool IsValid(Vector3 v)
        {
            return
            float.IsFinite(v.x) &&
            float.IsFinite(v.y) &&
            float.IsFinite(v.z);
        }
        private Spline getSpline()
        {
            if (splineContainer)
            {
                Spline spline = splineContainer.Spline;
                return spline;
            }

            return null;
        }
        
        public void ClearSpline()
        {
            Spline s = getSpline();
            if (s != null)
            {
                s.Clear();
                countKnots = 0;
            }
        }

        private void AddKnot(BezierKnot knot)
        {
            bounds.Encapsulate(knot.Position);

            Spline s = getSpline();
            if (s != null)
            {
                s.Add(knot);
                countKnots++;
            }
        }

        private void AddAll(List<BezierKnot> knots)
        {
            foreach(BezierKnot current in knots)
            {
                AddKnot(current);
            }
        }

        private void SetKnot(BezierKnot knot, int index)
        {
            Spline spline = getSpline();
            if (spline != null)
            {
                if (index >= 0 && index < spline.Count)
                {
                    spline.SetKnot(index, knot);
                }
            }
        }

        private BezierKnot GetKnot(int index)
        {
            Spline spline = getSpline();
            if (spline != null)
            {
                if (index >= 0 && index < spline.Count)
                {
                    return spline[index];
                }
            }
            return new BezierKnot();
        }

        private List<BezierKnot> CopyAll()
        {
            List<BezierKnot> copy = new List<BezierKnot>();
            for(int i = 0; i < Count(); i++)
            {
                copy.Add(GetKnot(i));   
            }
            return copy;
        }





        private int Count()
        {
            Spline spline = getSpline();
            if (spline != null)
            {
                return spline.Count;
            }
            return 0;
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
            ClearSpline();
            AddSegment(points, sizeTangentInScalar, sizeTangentOutScalar);
        }

        

        public void AddSegment(BezierSegment segment)
        {
            //AddSegment(segment.GetPositions(), segment.TangentInScalar(), segment.TangentOutScalar());

            //4 point bezier
            List<Vector3> positions = segment.GetPositions();

            if (positions.Count == 4)
            {
                for (int i = 0; i < positions.Count; i += 4)
                {
                    if (i + 4 <= positions.Count)
                    {
                        Vector3 p0 = positions[i];
                        Vector3 p1 = positions[i + 1];
                        Vector3 p2 = positions[i + 2];
                        Vector3 p3 = positions[i + 3];
                        AddSegment(p0, p1, p2, p3);
                    }
                }
            }
            else
            {
                AddSegment(
                    positions, segment.TangentInScalar(), segment.TangentOutScalar()
                );
            }


        }
        

        

        public void AddSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 tangentA = (p1 - p0);
            Vector3 tangentB = (p3 - p2) * -1.0f;

            

            BezierKnot knot = new BezierKnot(
                p0, //current,
                Vector3.zero,
                tangentA
            );

            AddKnot(knot);

            BezierKnot knot1 = new BezierKnot(
                p3, //current,
                tangentB,
                Vector3.zero
            );
            AddKnot(knot1);
        }

        



        //add single segment, no C1 contunuity between segments added later!
        public void AddSegment(
            List<Vector3> points,
            float sizeTangentInOutScalar
        )
        {
            AddSegment(points, sizeTangentInOutScalar, sizeTangentInOutScalar);
        }

        public void AddSegment(
            List<Vector3> points,
            float sizeTangentInScalar,
            float sizeTangentOutScalar
        )
        {
            AddSegment(points, sizeTangentInScalar, sizeTangentOutScalar, false);
        }


        public void AddSegment(
            List<Vector3> points,
            float sizeTangentInScalar,
            float sizeTangentOutScalar,
            bool degenerate
        )
        {
            sizeTangentInScalar = Math.Clamp(sizeTangentInScalar, 0.0f, 1.0f);
            sizeTangentOutScalar = Math.Clamp(sizeTangentOutScalar, 0.0f, 1.0f);

            /*
            //bezier knot constructor
            BezierKnot start = new BezierKnot(
                position: new float3(0, 0, 0),
                tangentIn: new float3(-2, 0, 0),
                tangentOut: new float3(2, 5, 0)
            );
            */
            if (points.Count > 1)
            {

                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 prev = i > 0 ? points[i - 1] : points[i];
                    Vector3 current = points[i];
                    Vector3 next = i < points.Count - 1 ? points[i + 1] : points[i];

                    Vector3 dir = (next - prev) * 0.5f;

                    Vector3 tangentIn = -dir * sizeTangentInScalar; //muss in die andere richtung zeigen
                    Vector3 tangentOut = dir * sizeTangentOutScalar;

                    //tangent in und out gleich: erste ableitung stetig.

                    //move all to local space
                    Vector3 local = parentObj.transform.InverseTransformPoint(current);

                    BezierKnot knot = new BezierKnot(
                        local, //current,
                        tangentIn,
                        tangentOut
                    );

                    AddKnot(knot);
                }



            }
        }




    }
}