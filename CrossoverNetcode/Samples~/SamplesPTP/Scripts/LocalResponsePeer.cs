using System;
using System.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode.Samples
{
    public class LocalResponsePeer : MonoBehaviour
    {
        public void Awake()
        {
            GetComponent<NetworkPeerBehavior>().CrossoverEventReceived += OnCrossoverEventReceived;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            GetComponent<NetworkPeerBehavior>().StartListenForConnections();
        }

        private void OnCrossoverEventReceived(CrossoverEvent crossoverEvent)
        {
            Debug.Log($"Received crossover event: {crossoverEvent}");
        }
    }
}
