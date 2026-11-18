using NUnit.Framework;

namespace Framework.Tests
{
    public class LookSamplingTests
    {
        [Test]
        public void A_normal_step_is_passed_through_unchanged()
        {
            Assert.That(LookSampling.ClampedDelta(0.1f, 0.1f), Is.EqualTo(0.1f).Within(0.0001f));
        }

        [Test]
        public void A_long_stall_is_limited_to_two_sample_intervals()
        {
            Assert.That(LookSampling.ClampedDelta(30f, 0.1f), Is.EqualTo(0.2f).Within(0.0001f));
        }

        [Test]
        public void A_negative_step_becomes_zero()
        {
            Assert.That(LookSampling.ClampedDelta(-1f, 0.1f), Is.EqualTo(0f));
        }
    }
}
