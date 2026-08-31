
using System;
using UnityEngine;
using util;

namespace Core.Catching
{

    //base class to lerp by time
    public class BaseLerp
    {
        public float timeMax = 10.0f;
        protected float timeIntegrated = 0.0f;
        protected bool reachedEnd = false;

        //delay in animation to have and keep inplace 
        public float time_DelayAfterMaxTime = 0.0f;
        public bool bTimeDelayAfterMaxTimeEnabled = false;

        public void SetTimeDelayAfterMaxTime(float time)
        {
            time_DelayAfterMaxTime = Mathf.Abs(time);
            bTimeDelayAfterMaxTimeEnabled = true;
        }

        public void DisableTimeDelayAfterMaxTime()
        {
            bTimeDelayAfterMaxTimeEnabled = false;
        }


        public virtual void Reset()
        {
            reachedEnd = false;
            timeIntegrated = 0.0f;
        }

        public void ResetWithRandomTime(float lower, float upper)
        {
            float min = Mathf.Min(lower, upper);
            float max = Mathf.Min(lower, upper);


            Reset();
            timeMax = UnityEngine.Random.Range(min, max);
        }



        //base tick: integrate time and update finish flags
        public void BaseTick(float deltatime)
        {
            if (reachedEnd)
            {
                return;
            }

            timeIntegrated += deltatime;
            //float t = GetScalarT();
            UpdateFinishFlag();
            OnTick(); //for derived classes
        }

        protected virtual void OnTick()
        {
            //for derived classes
        }






        public bool ReachedFlagRead()
        {
            return reachedEnd;
        }

        public bool ReachedEnd()
        {
            bool copy = reachedEnd;
            if (copy)
            {
                //ResetWithRotationUpdate();
                Reset();
            }
            reachedEnd = false;
            return copy;
        }

        public float GetScalarT()
        {
            float t = timeIntegrated / timeMax;
            t = Math.Clamp(t, 0.0f, 1.0f);
            return t;
        }

        protected void UpdateFinishFlag()
        {
            //if (timeIntegrated >= timeMax)
            if (timeIntegrated >= TimeMaxWithDelay())
            {
                reachedEnd = true;
                timeIntegrated = 0.0f;
            }
        }

        private float TimeMaxWithDelay()
        {
            if (bTimeDelayAfterMaxTimeEnabled)
            {
                return timeMax + time_DelayAfterMaxTime;
            }
            return timeMax;
        }

    }
}