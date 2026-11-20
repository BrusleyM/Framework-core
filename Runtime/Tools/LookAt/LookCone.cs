using UnityEngine;

namespace Framework
{
    public static class LookCone
    {
        private const float SamePositionDistanceSquared = 0.000001f;

        public static bool IsInside(Vector3 eyePosition, Vector3 eyeForward, Vector3 targetPosition, float maxAngleDegrees)
        {
            var toTarget = targetPosition - eyePosition;
            if (toTarget.sqrMagnitude < SamePositionDistanceSquared)
            {
                return true;
            }

            return Vector3.Angle(eyeForward, toTarget) <= maxAngleDegrees;
        }
    }
}
