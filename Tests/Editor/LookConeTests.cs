using NUnit.Framework;
using UnityEngine;

namespace Framework.Tests
{
    public class LookConeTests
    {
        [Test]
        public void A_target_straight_ahead_is_inside_the_cone()
        {
            Assert.That(LookCone.IsInside(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 5f), 10f), Is.True);
        }

        [Test]
        public void A_target_just_inside_the_angle_is_inside_the_cone()
        {
            var target = Quaternion.Euler(0f, 9f, 0f) * Vector3.forward * 5f;

            Assert.That(LookCone.IsInside(Vector3.zero, Vector3.forward, target, 10f), Is.True);
        }

        [Test]
        public void A_target_just_outside_the_angle_is_outside_the_cone()
        {
            var target = Quaternion.Euler(0f, 11f, 0f) * Vector3.forward * 5f;

            Assert.That(LookCone.IsInside(Vector3.zero, Vector3.forward, target, 10f), Is.False);
        }

        [Test]
        public void A_target_behind_the_eyes_is_outside_the_cone()
        {
            Assert.That(LookCone.IsInside(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -5f), 90f), Is.False);
        }

        [Test]
        public void A_target_at_the_eye_position_counts_as_inside()
        {
            Assert.That(LookCone.IsInside(new Vector3(1f, 2f, 3f), Vector3.forward, new Vector3(1f, 2f, 3f), 5f), Is.True);
        }

        [Test]
        public void The_angle_is_measured_in_three_dimensions_so_looking_too_high_is_outside_the_cone()
        {
            var target = Quaternion.Euler(-30f, 0f, 0f) * Vector3.forward * 5f;

            Assert.That(LookCone.IsInside(Vector3.zero, Vector3.forward, target, 10f), Is.False);
        }
    }
}
