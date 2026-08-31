using Unity.Collections;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode
{
    /// <summary>
    /// Implements a crossover Event which transferres scoring data (number).
    /// </summary>
    [CreateAssetMenu(menuName = "Crossover Event/Score Event")]
    public class ScoreEvent : CrossoverEvent
    {
        /// <summary>
        /// Payload of the event.
        /// </summary>
        public int Score {  get; private set; }

        protected override CrossoverEventType crossoverEventType => CrossoverEventType.ScoreChangeEvent;
        protected override int streamSize => 33;

        public override void InitializeFromStream(DataStreamReader reader)
        {
            Score = reader.ReadInt();
        }

        public override void WriteToStream(ref DataStreamWriter stream)
        {
            base.WriteToStream(ref stream);
            stream.WriteInt(Score);
        }

        /// <summary>
        /// Sets the Payload Score
        /// </summary>
        /// <param name="score">score to set it to</param>
        public void SetScore(int score)
        {
            Score = score;
        }

        public override string ToString()
        {
            return $"Score: {Score}";
        }
    }
}
