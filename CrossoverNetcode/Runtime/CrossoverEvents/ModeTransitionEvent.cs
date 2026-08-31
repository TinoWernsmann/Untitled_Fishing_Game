using Unity.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode.CrossoverEvents
{
    [CreateAssetMenu(menuName = "Crossover Event/Mode Transition Event")]
    public class ModeTransitionEvent : CrossoverEvent
    {
        public enum Mode
        {
            Arcade,
            MixedReality,
            VirtualReality
        }
        
        [field: SerializeField]public Mode fromMode { get; set; }
        [field: SerializeField]public Mode toMode { get; set; }
        
        protected override CrossoverEventType crossoverEventType => CrossoverEventType.ModeTransitionEvent;
        protected override int streamSize => (sizeof(Mode) * 2 + 1);

        public override void InitializeFromStream(DataStreamReader reader)
        {
            fromMode = (Mode)reader.ReadInt();
            toMode = (Mode)reader.ReadInt();
        }

        public override void WriteToStream(ref DataStreamWriter stream)
        {
            base.WriteToStream(ref stream);
            stream.WriteInt((int)fromMode);
            stream.WriteInt((int)toMode);
        }
    }
}
