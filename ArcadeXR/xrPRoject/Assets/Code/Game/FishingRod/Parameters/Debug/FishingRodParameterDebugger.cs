using UnityEngine;
using MixedRealityArcade.ArcadeXR.Network;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;

namespace Game
{
    /// <summary>
    /// tests the parameter dispatch / Simple event dispatching, works
    /// </summary>
    public class FishingRodParameterDebugger
    {
        public void Setup()
        {
            RunDebug();
        }
        
        private void RunDebug()
        {
            if (NetworkManager.Instance == null)
            {
                Debug.Log("FishingRodParameterDebugger::RUN FAILED - NetworkManager Instance is missing");
                return;
            }

            if (true)
            {
                return;
            }

            //is tested.

            /*
            ParseEventString("rodparams_minigameradius_5");
            ParseEventString("rodparams_minigametime_2");
            ParseEventString("rodparams_pullstrength_2");
            ParseEventString("rodparams_depthlevel_2");
            */

            Debug.Log("FishingRodParameterDebugger::RUN");

            SimpleEvent eventNew = new SimpleEvent();
            
            eventNew.Payload = "rodparams_minigameradius_10";
            Debug.Log("FishingRodParameterDebugger::RUN " + eventNew.Payload);
            NetworkManager.Instance.HandleSimpleEvent(eventNew);

            eventNew.Payload = "rodparams_minigametime_4";
            Debug.Log("FishingRodParameterDebugger::RUN " + eventNew.Payload);
            NetworkManager.Instance.HandleSimpleEvent(eventNew);

            eventNew.Payload = "rodparams_pullstrength_4";
            Debug.Log("FishingRodParameterDebugger::RUN " + eventNew.Payload);
            NetworkManager.Instance.HandleSimpleEvent(eventNew);

            eventNew.Payload = "rodparams_depthlevel_1";
            Debug.Log("FishingRodParameterDebugger::RUN " + eventNew.Payload);
            NetworkManager.Instance.HandleSimpleEvent(eventNew);
        }
    }
}