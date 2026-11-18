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
            PlacementFacing facing,
            Quaternion currentRotation)
        {
            var viewDirectionOnFloorPlane = ViewDirectionOnFloorPlane(eyeForward);
            var position = eyePosition
                + Vector3.up * heightOffset
                + viewDirectionOnFloorPlane * distanceInFront;
            return new Pose(position, RotationFor(facing, viewDirectionOnFloorPlane, currentRotation));
        }

        private static Quaternion RotationFor(PlacementFacing facing, Vector3 viewDirectionOnFloorPlane, Quaternion currentRotation)
        {
            switch (facing)
            {
                case PlacementFacing.SameAsViewDirection:
                    return Quaternion.LookRotation(viewDirectionOnFloorPlane, Vector3.up);
                case PlacementFacing.TowardsUser:
                    return Quaternion.LookRotation(-viewDirectionOnFloorPlane, Vector3.up);
                default:
                    return currentRotation;
            }
        }

        private static Vector3 ViewDirectionOnFloorPlane(Vector3 eyeForward)
        {
            var onFloorPlane = Vector3.ProjectOnPlane(eyeForward, Vector3.up);
            return onFloorPlane.sqrMagnitude < 0.000001f ? Vector3.forward : onFloorPlane.normalized;
        }
    }
}
