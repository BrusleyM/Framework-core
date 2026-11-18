using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Framework.Tests
{
    public class EyeHeightPlacementTests
    {
        private static Pose Place(Vector3 eyePosition, Vector3 eyeForward, float distance = 1f, float heightOffset = 0f, PlacementFacing facing = PlacementFacing.SameAsViewDirection)
        {
            return EyeHeightPlacement.InFrontOfEyes(eyePosition, eyeForward, distance, heightOffset, facing, Quaternion.identity);
        }

        [Test]
        public void The_placed_pose_sits_at_the_height_of_the_eyes()
        {
            var pose = Place(new Vector3(2f, 1.6f, 3f), Vector3.forward);

            Assert.That(pose.position.y, Is.EqualTo(1.6f).Within(0.0001f));
        }

        [Test]
        public void The_height_offset_is_added_to_the_eye_height()
        {
            var pose = Place(new Vector3(0f, 1.6f, 0f), Vector3.forward, heightOffset: -0.2f);

            Assert.That(pose.position.y, Is.EqualTo(1.4f).Within(0.0001f));
        }

        [Test]
        public void The_placed_pose_is_the_requested_distance_in_front_of_the_eyes()
        {
            var pose = Place(new Vector3(1f, 1.6f, 1f), Vector3.right, distance: 2f);

            Assert.That(pose.position, Is.EqualTo(new Vector3(3f, 1.6f, 1f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Looking_down_still_places_the_pose_at_eye_height()
        {
            var pose = Place(new Vector3(0f, 1.6f, 0f), new Vector3(0f, -1f, 1f).normalized, distance: 1f);

            Assert.That(pose.position.y, Is.EqualTo(1.6f).Within(0.0001f));
            Assert.That(pose.position.z, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Looking_straight_up_falls_back_to_the_world_forward_direction()
        {
            var pose = Place(Vector3.zero, Vector3.up, distance: 1f);

            Assert.That(pose.position, Is.EqualTo(Vector3.forward).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void A_zero_distance_places_the_pose_at_the_eyes_horizontally()
        {
            var pose = Place(new Vector3(4f, 1.5f, -2f), Vector3.forward, distance: 0f);

            Assert.That(pose.position, Is.EqualTo(new Vector3(4f, 1.5f, -2f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Aligning_with_the_view_direction_points_the_pose_forward_along_the_floor_plane()
        {
            var pose = Place(Vector3.zero, new Vector3(1f, -0.5f, 1f).normalized);

            var forward = pose.rotation * Vector3.forward;

            Assert.That(forward.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(Vector3.Dot(forward, new Vector3(1f, 0f, 1f).normalized), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Not_aligning_keeps_the_current_rotation()
        {
            var current = Quaternion.Euler(10f, 20f, 30f);

            var pose = EyeHeightPlacement.InFrontOfEyes(new Vector3(1f, 2f, 3f), Vector3.right, 1f, 0f, PlacementFacing.KeepRotation, current);

            Assert.That(Quaternion.Angle(pose.rotation, current), Is.LessThan(0.01f));
            Assert.That(pose.position, Is.EqualTo(new Vector3(2f, 2f, 3f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Facing_towards_the_user_points_the_pose_forward_back_at_the_eyes()
        {
            var pose = Place(Vector3.zero, Vector3.forward, facing: PlacementFacing.TowardsUser);

            var forward = pose.rotation * Vector3.forward;

            Assert.That(Vector3.Dot(forward, Vector3.back), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void A_rolled_head_with_a_horizontal_view_places_the_pose_straight_ahead()
        {
            var rolledForward = (Quaternion.Euler(0f, 0f, 35f) * Vector3.forward);

            var pose = Place(new Vector3(0f, 1.6f, 0f), rolledForward, distance: 2f);

            Assert.That(pose.position, Is.EqualTo(new Vector3(0f, 1.6f, 2f)).Using(Vector3EqualityComparer.Instance));
        }
    }
}
