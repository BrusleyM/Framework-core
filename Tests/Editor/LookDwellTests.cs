using NUnit.Framework;

namespace Framework.Tests
{
    public class LookDwellTests
    {
        [Test]
        public void The_first_sample_that_looks_at_the_target_reports_started()
        {
            var dwell = new LookDwell(1f);

            var change = dwell.Update(true, 0.1f);

            Assert.That(change, Is.EqualTo(LookChange.Started));
        }

        [Test]
        public void With_no_dwell_time_the_first_look_reports_started_and_seen_together()
        {
            var dwell = new LookDwell(0f);

            var change = dwell.Update(true, 0.1f);

            Assert.That(change, Is.EqualTo(LookChange.Started | LookChange.Seen));
        }

        [Test]
        public void The_target_is_seen_once_the_dwell_time_has_passed()
        {
            var dwell = new LookDwell(0.5f);
            dwell.Update(true, 0.1f);
            dwell.Update(true, 0.3f);

            var change = dwell.Update(true, 0.2f);

            Assert.That(change, Is.EqualTo(LookChange.Seen));
        }

        [Test]
        public void The_time_of_the_sample_that_starts_the_look_does_not_count_towards_the_dwell()
        {
            var dwell = new LookDwell(0.5f);

            dwell.Update(true, 10f);

            Assert.That(dwell.HasBeenSeen, Is.False);
        }

        [Test]
        public void A_glance_shorter_than_the_dwell_time_ends_without_being_seen()
        {
            var dwell = new LookDwell(0.5f);
            dwell.Update(true, 0.1f);
            dwell.Update(true, 0.2f);

            var change = dwell.Update(false, 0.1f);

            Assert.That(change, Is.EqualTo(LookChange.Ended));
            Assert.That(dwell.HasBeenSeen, Is.False);
        }

        [Test]
        public void Looking_away_resets_the_dwell_so_it_has_to_start_again()
        {
            var dwell = new LookDwell(0.5f);
            dwell.Update(true, 0.1f);
            dwell.Update(true, 0.4f);
            dwell.Update(false, 0.1f);
            dwell.Update(true, 0.1f);

            var change = dwell.Update(true, 0.3f);

            Assert.That(change, Is.EqualTo(LookChange.None));
            Assert.That(dwell.HasBeenSeen, Is.False);
        }

        [Test]
        public void A_seen_target_is_not_reported_as_seen_again_when_looked_at_again()
        {
            var dwell = new LookDwell(0f);
            dwell.Update(true, 0.1f);
            dwell.Update(false, 0.1f);

            var change = dwell.Update(true, 0.1f);

            Assert.That(change, Is.EqualTo(LookChange.Started));
        }

        [Test]
        public void Looking_away_after_being_seen_still_reports_ended()
        {
            var dwell = new LookDwell(0f);
            dwell.Update(true, 0.1f);

            Assert.That(dwell.Update(false, 0.1f), Is.EqualTo(LookChange.Ended));
        }

        [Test]
        public void Progress_rises_with_the_dwell_and_stays_full_once_seen()
        {
            var dwell = new LookDwell(1f);
            dwell.Update(true, 0.1f);
            dwell.Update(true, 0.5f);
            var halfway = dwell.Progress;
            dwell.Update(true, 0.6f);
            dwell.Update(false, 0.1f);

            Assert.That(halfway, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(dwell.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void Not_looking_at_all_reports_no_change()
        {
            var dwell = new LookDwell(0.5f);

            Assert.That(dwell.Update(false, 0.1f), Is.EqualTo(LookChange.None));
        }

        [Test]
        public void Reset_forgets_that_the_target_was_seen()
        {
            var dwell = new LookDwell(0f);
            dwell.Update(true, 0.1f);

            dwell.Reset();

            Assert.That(dwell.HasBeenSeen, Is.False);
            Assert.That(dwell.IsLooking, Is.False);
            Assert.That(dwell.Update(true, 0.1f), Is.EqualTo(LookChange.Started | LookChange.Seen));
        }

        [Test]
        public void A_negative_delta_never_reduces_the_dwell()
        {
            var dwell = new LookDwell(0.5f);
            dwell.Update(true, 0.1f);
            dwell.Update(true, 0.3f);

            dwell.Update(true, -5f);

            Assert.That(dwell.Progress, Is.EqualTo(0.6f).Within(0.0001f));
        }
    }
}
