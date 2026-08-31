
using System;
using UnityEngine;
using util;

namespace Core.Catching
{
    public class VerticalLerp : BaseLerp
    {
        private float DeltaVerticalDirection = 0.3f;

        public void SetDeltaVertical(float inValue)
        {
            DeltaVerticalDirection = Math.Abs(inValue);
        }

        //adds delta movement from current height to a target with deltatime
        //if target is not reached (max time)
        public Vector3 Tick(Vector3 currentPositon, Vector3 target, float deltatime, float direction)
        {
            if (reachedEnd)
            {
                return target;
            }
            BaseTick(deltatime);

            float dirS = direction > 0.0f ? 1.0f : -1.0f;

            float heightCurrent = currentPositon.y;
            heightCurrent += dirS * DeltaVerticalDirection * deltatime;
            target.y = heightCurrent;
            return target;

        }


    }
}