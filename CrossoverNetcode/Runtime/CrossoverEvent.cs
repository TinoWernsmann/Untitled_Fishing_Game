using Unity.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode
{
    /// <summary>
    /// The base class for crossover events which are passed over the network between the ArcadeDisplay and ArcadeXR.
    /// </summary>
    public abstract class CrossoverEvent : ScriptableObject
    {
        /// <summary>
        /// This property must be overridden in each subclass to make its type identifiable when it is streamed over the network.
        /// It must correspond to the mapping used in <see cref="CrossoverEventFactory"/>
        /// </summary>
        protected abstract CrossoverEventType crossoverEventType { get; }
        
        /// <summary>
        /// Specifies the size of this crossover event when its data is serialized on a data stream in number of bytes.
        /// </summary>
        protected abstract int streamSize { get; }
        
        /// <summary>
        /// Initializes the CrossoverEvent by setting its attributes to the ones parsed from a data stream.
        /// </summary>
        /// <param name="reader">the DataStreamReader from which to parse the attributes</param>
        public abstract void InitializeFromStream(DataStreamReader reader);

        /// <summary>
        /// Writes the CrossoverEvent to a stream that can be sent over the network.
        /// The base Method must be called at the start when overriding this method to initialize the data stream with the parsed CrossoverEventType.
        /// </summary>
        /// <param name="stream">an empty DataStreamWriter that parses the CrossoverEvent.</param>
        public virtual void WriteToStream(ref DataStreamWriter stream)
        {
            // The type of the crossover event needs to be parsed at the start of the stream.
            stream.WriteInt((int)crossoverEventType);
        }
    }
}