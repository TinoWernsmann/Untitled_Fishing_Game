using Unity.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode.CrossoverEvents
{
    /// <summary>
    /// An example <see cref="CrossoverEvent"/> implementation that can be used for testing the net code solution.
    /// </summary>
    [CreateAssetMenu(menuName = "Crossover Event/Test Event")]
    public class TestEvent : CrossoverEvent
    {
        [field: SerializeField]public int testInt { get; private set; }
        [field: SerializeField]public bool testBool { get; private set; }
        [field: SerializeField]public Vector2 testVector{ get; private set; }
        protected override CrossoverEventType crossoverEventType => CrossoverEventType.TestEvent;
        protected override int streamSize => 17;

        public override void InitializeFromStream(DataStreamReader reader)
        {
            testInt = reader.ReadInt();
            
            testBool = reader.ReadByte() > 0;
            
            float vectorX = reader.ReadFloat();
            float vectorY = reader.ReadFloat();
            testVector = new Vector2(vectorX, vectorY);
        }

        public override void WriteToStream(ref DataStreamWriter stream)
        {
            base.WriteToStream(ref stream);
            
            stream.WriteInt(testInt);
            
            stream.WriteByte((byte)(testBool ? 1 : 0));
            
            stream.WriteFloat(testVector.x);
            stream.WriteFloat(testVector.y);
        }

        public override string ToString()
        {
            return $"testInt: {testInt}, testBool: {testBool}, testVector: {testVector}";
        }
    }
}