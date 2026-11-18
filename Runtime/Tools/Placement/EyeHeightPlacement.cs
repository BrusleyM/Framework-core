using UnityEngine;

namespace Framework
{
    public static class EyeHeightPlacement
    {
        public static Pose InFrontOfEyes(
            Vector3 eyePosition,
            Vector3 eyeForward,
            float distanceInFront,
            float heightOffset,
            bool alignWithViewDirection,
            Quaternion currentRotation)
        {
            var viewDirectionOnFloorPlane = ViewDirectionOnFloorPlane(eyeForward);
            var position = eyePosition
                + Vector3.up * heightOffset
                + viewDirectionOnFloorPlane * distanceInFront;
            var rotation = alignWithViewDirection
                ? Quaternion.LookRotation(viewDirectionOnFloorPlane, Vector3.up)
                : currentRotation;
            return new Pose(position, rotation);
        }

        private static Vector3 ViewDirectionOnFloorPlane(Vector3 eyeForward)
        {
            var onFloorPlane = Vector3.ProjectOnPlane(eyeForward, Vector3.up);
            return onFloorPlane.sqrMagnitude < 0.000001f ? Vector3.forward : onFloorPlane.normalized;
        }
    }
}
