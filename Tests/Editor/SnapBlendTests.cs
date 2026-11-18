using NUnit.Framework;
using UnityEngine;

namespace Framework.Tests
{
    public class SnapBlendTests
    {
        private static readonly Pose Start = new Pose(Vector3.zero, Quaternion.identity);
        private static readonly Pose End = new Pose(new Vector3(2f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));

        [Test]
        public void A_blend_with_zero_duration_is_complete_immediately()
        {
            var blend = new SnapBlend(Start, End, 0f);

            Assert.That(blend.IsCompleteAt(0f), Is.True);
        }

        [Test]
        public void Progress_is_the_elapsed_share_of_the_duration()
        {
            var blend = new SnapBlend(Start, End, 2f);

            Assert.That(blend.ProgressAt(0.5f), Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void Progress_never_exceeds_one_after_the_duration_has_passed()
        {
            var blend = new SnapBlend(Start, End, 1f);

            Assert.That(blend.ProgressAt(5f), Is.EqualTo(1f));
        }

        [Test]
        public void A_negative_duration_is_treated_as_zero()
        {
            var blend = new SnapBlend(Start, End, -3f);

            Assert.That(blend.IsCompleteAt(0f), Is.True);
        }

        [Test]
        public void Half_eased_progress_sits_halfway_between_start_and_end_positions()
        {
            var blend = new SnapBlend(Start, End, 1f);

            var pose = blend.PoseAtEasedProgress(0.5f);

            Assert.That(pose.position.x, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Full_eased_progress_reaches_the_end_rotation()
        {
            var blend = new SnapBlend(Start, End, 1f);

            var pose = blend.PoseAtEasedProgress(1f);

            Assert.That(Quaternion.Angle(pose.rotation, End.rotation), Is.LessThan(0.01f));
        }

        [Test]
        public void Half_eased_progress_sits_halfway_between_start_and_end_rotations()
        {
            var blend = new SnapBlend(Start, End, 1f);

            var pose = blend.PoseAtEasedProgress(0.5f);

            Assert.That(Quaternion.Angle(pose.rotation, Quaternion.Euler(0f, 45f, 0f)), Is.LessThan(0.01f));
        }
    }
}
