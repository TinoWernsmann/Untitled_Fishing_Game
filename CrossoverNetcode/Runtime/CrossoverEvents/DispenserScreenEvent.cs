using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace MixedRealityArcade.CrossoverNetcode.CrossoverEvents
{
    /// <summary>
    /// A <see cref="CrossoverEvent"/> that signifies an item being thrown out of the arcade's screen.
    /// </summary>
    [CreateAssetMenu(menuName = "Crossover Event/Dispenser Screen Event")]
    public class DispenserScreenEvent : CrossoverEvent
    {
        [SerializeField] private Vector2 relativeScreenSpacePosition;
        /// <summary>
        /// A vector holding two float values between 0.0 and 1.0 that represent the position on the screen at which an item is dispensed.
        /// The coordinate system's origin is placed in the top left corner of the screen with the x-axis pointing towards the right
        /// and the y-axis downwards. the axes are scaled such that x = 1 is in the right edge of the screen and y = 1 in the bottom edge.
        /// </summary>
        public Vector2 RelativeScreenSpacePosition {
            get => relativeScreenSpacePosition;
            // The vector needs to be clamped such that it does not point to a spot outside the screen.
            set => relativeScreenSpacePosition = new Vector2(
                Mathf.Clamp(value.x, 0, 1),
                Mathf.Clamp(value.y, 0, 1));
        }

        [SerializeField]private string itemName;
        /// <summary>
        /// The name of the item that is being dispensed. It has a maximum length of 32 characters.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws an exception if it is attempted to set this value to a string that exceeds 32 characters.
        /// </exception>
        public string ItemName
        {
            get => itemName;
            set {
                if (value.Length > 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(ItemName), value, "Item name must not exceed 32 characters");
                }
                itemName = value;
            }
        }
        protected override CrossoverEventType crossoverEventType => CrossoverEventType.DispenserScreenEvent;
        protected override int streamSize => sizeof(float) * 2 + 32;

        public override void InitializeFromStream(DataStreamReader reader)
        {
            ItemName = reader.ReadFixedString32().ToString();

            float x, y;
            x = reader.ReadFloat();
            y = reader.ReadFloat();
            RelativeScreenSpacePosition = new Vector2(x, y);
        }
        public override void WriteToStream(ref DataStreamWriter stream)
        {
            base.WriteToStream(ref stream);
            
            stream.WriteFixedString32(ItemName);
            stream.WriteFloat(RelativeScreenSpacePosition.x);
            stream.WriteFloat(RelativeScreenSpacePosition.y);
        }

        private void OnValidate()
        {
            if (ItemName.Length > 32)
            {
                ItemName = ItemName[..32];
            }

            // Validate RelativeScreenSpacePosition by implicitly calling its set accessor.
            RelativeScreenSpacePosition = RelativeScreenSpacePosition;
        }
    }
}