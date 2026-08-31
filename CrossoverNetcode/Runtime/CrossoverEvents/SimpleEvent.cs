using System;
using Unity.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode.CrossoverEvents
{
    /// <summary>
    /// This class represents a simple crossover event that only holds a 32 byte large string as its payload.
    /// In every case where no additional parameters are needed to be sent between network peers except for its type, this CrossoverEvent type should be used.
    /// Although it is possible encode other kinds of data into the string payload, this usage is not intended.
    /// To send different data over the network, other CrossoverEvent types should be used or implemented if needed instead.
    /// </summary>
    [CreateAssetMenu(menuName = "Crossover Event/Simple Event")]
    public class SimpleEvent : CrossoverEvent
    {
        [SerializeField] private string payload;
        /// <summary>
        /// The string that is sent via this crossover event. It must not exceed 32 characters.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">setting this property throws an exception if the new value exceeds 32 characters in length</exception>
        public string Payload
        {
            get => payload;
            set
            {
                if (value.Length > 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(Payload), value, "The payload must not exceed 32 characters");
                }
                payload = value;
            }
        }

        protected override CrossoverEventType crossoverEventType => CrossoverEventType.SimpleEvent;
        protected override int streamSize => 33;
        
        public override void InitializeFromStream(DataStreamReader reader)
        {
            payload = reader.ReadFixedString32().ToString();
        }

        public override void WriteToStream(ref DataStreamWriter stream)
        {
            base.WriteToStream(ref stream);
            stream.WriteFixedString32(payload);
        }

        private void OnValidate()
        {
            if (Payload.Length > 32)
            {
                Payload = Payload[..32];
            }
        }
        
        public override string ToString() => payload;
    }
}