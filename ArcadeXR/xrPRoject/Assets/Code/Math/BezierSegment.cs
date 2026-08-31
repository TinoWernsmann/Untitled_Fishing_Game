using System;
using System.Collections.Generic;
using Core.Catching;
using Game.FishingRod.transfer;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace util
{
    /// <summary>
    /// creates a point list from Game Objects passed
    /// and the tangent scalars for constructing inside the 
    /// Spline Base class
    /// </summary>
    public class BezierSegment
    {
        private float sizeTangentScalarForBSpline_IN = 0.3f;
        private float sizeTangentScalarForBSpline_OUT = 0.3f;

        private List<Vector3> positions = new List<Vector3>();

        //if the positions are equal to 4, the inner will be treated as tangents
        //otherwise all points will be treated as positions and tangents
        //auto generated
        public List<Vector3> GetPositions()
        {
            return positions;
        }

        public void OverrideTangentScalars(float inTangent, float outTangent)
        {
            sizeTangentScalarForBSpline_IN = math.abs(inTangent);
            sizeTangentScalarForBSpline_OUT = math.abs(outTangent);
        }


        public float TangentInScalar()
        {
            return sizeTangentScalarForBSpline_IN;
        }

        public float TangentOutScalar()
        {
            return sizeTangentScalarForBSpline_OUT;
        }


        public BezierSegment(List<GameObject> childs)
        {
            BuildFromChildren(childs);
        }

        public void BuildFromChildren(List<GameObject> childs)
        {
            childs.Sort((a, b) => string.Compare(a.name, b.name));
            positions.Clear();
            for (int i = 0; i < childs.Count; i++)
            {
                positions.Add(childs[i].transform.position);
            }
        }

        public bool IsValid()
        {
            return positions.Count > 1;
        }


    }
}