using System;
using Unity.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode.CrossoverEvents
{
    /// <summary>
    /// This class sends the upgrade information to the HMD. It contains a target and a level.
    /// </summary>
    [CreateAssetMenu(menuName = "Crossover Event/Upgrade Event")]
    public class UpgradeEvent : CrossoverEvent
    {
        //Target can be fishing rod or buoy
        public enum TargetMode
        {
            FishingRod,
            Buoy
        }
        public TargetMode Target {  get; private set; }
        public int Level {  get; private set; }
        /// <summary>
        /// The string that is sent via this crossover event. It must not exceed 32 characters.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">setting this property throws an exception if the new value exceeds 32 characters in length</exception>
       
        protected override CrossoverEventType crossoverEventType => CrossoverEventType.UpgradeEvent;
        protected override int streamSize => sizeof(TargetMode) * 2 + 1+33;
        
        public override void InitializeFromStream(DataStreamReader reader)
        {
            Target = (TargetMode)reader.ReadInt();
            Level = reader.ReadInt();

        }

        public override void WriteToStream(ref DataStreamWriter stream)
        {
            base.WriteToStream(ref stream);
            stream.WriteInt((int)Target);
            stream.WriteInt(Level);
        }

        
        public override string ToString() => "Target: "+Target+" Level:"+Level;
    }
}