using System;
using MixedRealityArcade.CrossoverNetcode.CrossoverEvents;
using Unity.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode
{
    /// <summary>
    /// The CrossoverEventFactory implements a simple factory pattern. It creates <see cref="CrossoverEvent"/>s depending on a data stream read from the network layer.
    /// </summary>
    public static class CrossoverEventFactory
    {
        /// <summary>
        /// Creates a <see cref="CrossoverEvent"/> from a <see cref="DataStreamReader"/>. The data stream must have been created from a CrossoverEvent.
        /// </summary>
        /// <param name="stream"> the DataStreamReader which holds the serialized data of a CrossoverEvent</param>
        /// <returns>a CrossoverEvent that holds the data taken from the data stream</returns>
        public static CrossoverEvent CreateCrossoverEvent(DataStreamReader stream)
        {
            // Parse the type of crossover event.
            CrossoverEventType type = (CrossoverEventType)stream.ReadInt();
            
            // Create crossoverEvents depending on the parsed type.
            CrossoverEvent crossoverEvent;
            switch (type)
            {
                case CrossoverEventType.ModeTransitionEvent:
                    crossoverEvent = ScriptableObject.CreateInstance<ModeTransitionEvent>();
                    break;
                case CrossoverEventType.TestEvent:
                    crossoverEvent = ScriptableObject.CreateInstance<TestEvent>();
                    break;
                case CrossoverEventType.DispenserScreenEvent:
                    crossoverEvent = ScriptableObject.CreateInstance<DispenserScreenEvent>();
                    break;
                case CrossoverEventType.SimpleEvent:
                    crossoverEvent = ScriptableObject.CreateInstance<SimpleEvent>();
                    break;
                case CrossoverEventType.ScoreChangeEvent:
                    crossoverEvent = ScriptableObject.CreateInstance<ScoreEvent>();
                    break;
                
                // Append additional cases here when implementing new crossover event types
                
                default:
                    throw new NotImplementedException();
            }

            crossoverEvent.InitializeFromStream(stream);
            return crossoverEvent;
        }
    }
}