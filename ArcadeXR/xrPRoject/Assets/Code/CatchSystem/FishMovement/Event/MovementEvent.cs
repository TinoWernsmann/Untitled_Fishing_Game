using System;
using UnityEngine;
using util;

namespace Core.Catching
{
    /// <summary>
    /// event which can be triggered once and is part of "movement pattern with events"class
    /// </summary>
    public class MovementEvent
    {
        private float atScalar = 0.0f;

        private bool bWasExecuted = false;

        private String debugName = "no name";

        public MovementEvent(float t)
        {
            atScalar = t;
        }

        public MovementEvent(float t, String name)
        {
            atScalar = t;
            debugName = name;
        }

        public bool TPassedEvent(float tIn)
        {
            return tIn >= atScalar;
        }

        public bool WasExecuted()
        {
            return bWasExecuted;
        }

        public void Reset()
        {
            bWasExecuted = false;
        }
        
        public virtual void ExecuteEvent()
        {
            bWasExecuted = true;
            Debug.Log("Execute Event: " + debugName + " at: " + atScalar);
        }



    }
}