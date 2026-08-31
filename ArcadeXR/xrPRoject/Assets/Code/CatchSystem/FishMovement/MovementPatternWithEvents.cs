

using System;
using System.Collections.Generic;
using UnityEngine;
using util;

namespace Core.Catching
{
    /// <summary>
    /// allows to add events to a movement pattern
    /// at different scalars in [0,1] of animation
    /// can be disabled and enabled completly
    /// </summary>
    public class MovementPatternWithEvents : MovementPattern
    {
        private bool eventsEnabled = false;

        List<MovementEvent> events = new List<MovementEvent>();


        public MovementPatternWithEvents(GameObject owner) : base(owner)
        {
            eventsEnabled = true;
            events.Clear();
        }

        /// <summary>
        /// enables or disables events completly
        /// </summary>
        /// <param name="flag"></param>
        public void SetEventsEnabled(bool flag)
        {
            eventsEnabled = flag;
        }



        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < events.Count; i++)
            {
                MovementEvent current = events[i];
                current.Reset();
            }
        }

        protected override void OnTick()
        {
            ProcessEvents();
        }

        /// <summary>
        /// re checks all events to be processed based on current 
        /// progress of animation: GetScalarT
        /// </summary>
        protected void ProcessEvents()
        {
            if (eventsEnabled)
            {
                float t = GetScalarT();
                for (int i = 0; i < events.Count; i++)
                {
                    MovementEvent current = events[i];
                    ProcessEvent(current, t);
                }
            }
        }

        /// <summary>
        /// if a scalar t has passed an event which is not executed yet, 
        /// it will be executed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="currentT"></param>
        protected void ProcessEvent(MovementEvent item, float currentT)
        {
            if (item != null && eventsEnabled)
            {
                if (item.TPassedEvent(currentT) && !item.WasExecuted())
                {
                    item.ExecuteEvent();
                }
            }
        }
        
        /// <summary>
        /// adds a event to the event list
        /// </summary>
        /// <param name="eventIn"></param>
        public void AddEvent(MovementEvent eventIn)
        {
            if(eventIn != null && events != null)
            {
                events.Add(eventIn);
            }
        }





    } 
}