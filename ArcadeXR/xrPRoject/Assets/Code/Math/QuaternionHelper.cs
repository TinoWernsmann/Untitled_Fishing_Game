

using UnityEngine;

namespace util.Quat
{
    public class QuatHelper
    {
        public static Quaternion MakeQuatFromDirection2D(Vector3 direction)
        {
            direction.y = 0.0f;
            return MakeQuatFromDirection(direction);
        }

        public static Quaternion MakeQuatFromDirection(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            return targetRotation;
        }
    }
}