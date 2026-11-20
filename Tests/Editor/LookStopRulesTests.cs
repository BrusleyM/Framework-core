using NUnit.Framework;

namespace Framework.Tests
{
    public class LookStopRulesTests
    {
        [Test]
        public void Stopping_on_any_target_stops_after_the_first_target_is_seen()
        {
            Assert.That(LookStopRules.ShouldStop(LookStopRule.WhenAnyTargetIsSeen, 1, 4), Is.True);
        }

        [Test]
        public void Stopping_on_any_target_keeps_looking_while_none_is_seen()
        {
            Assert.That(LookStopRules.ShouldStop(LookStopRule.WhenAnyTargetIsSeen, 0, 4), Is.False);
        }

        [Test]
        public void Stopping_on_all_targets_keeps_looking_while_one_is_missing()
        {
            Assert.That(LookStopRules.ShouldStop(LookStopRule.WhenAllTargetsAreSeen, 3, 4), Is.False);
        }

        [Test]
        public void Stopping_on_all_targets_stops_when_every_target_is_seen()
        {
            Assert.That(LookStopRules.ShouldStop(LookStopRule.WhenAllTargetsAreSeen, 4, 4), Is.True);
        }

        [Test]
        public void Stopping_on_all_targets_never_stops_when_there_are_no_targets()
        {
            Assert.That(LookStopRules.ShouldStop(LookStopRule.WhenAllTargetsAreSeen, 0, 0), Is.False);
        }

        [Test]
        public void The_never_rule_does_not_stop_even_when_everything_is_seen()
        {
            Assert.That(LookStopRules.ShouldStop(LookStopRule.Never, 4, 4), Is.False);
        }

        [Test]
        public void The_stop_reason_follows_the_rule()
        {
            Assert.That(LookStopRules.ReasonFor(LookStopRule.WhenAnyTargetIsSeen), Is.EqualTo(LookStopReason.AnyTargetSeen));
            Assert.That(LookStopRules.ReasonFor(LookStopRule.WhenAllTargetsAreSeen), Is.EqualTo(LookStopReason.AllTargetsSeen));
        }
    }
}
