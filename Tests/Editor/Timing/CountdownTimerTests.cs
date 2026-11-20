using System;
using NUnit.Framework;

namespace Framework.Timing.Tests
{
    public class CountdownTimerTests
    {
        [Test]
        public void A_new_countdown_is_not_running_and_ignores_ticks()
        {
            var timer = new CountdownTimer(5f);

            var expired = timer.Advance(10f);

            Assert.That(expired, Is.False);
            Assert.That(timer.IsRunning, Is.False);
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void A_running_countdown_counts_down_as_it_is_ticked()
        {
            var timer = new CountdownTimer(5f);
            timer.Restart();

            timer.Advance(2f);

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(2f));
            Assert.That(timer.RemainingSeconds, Is.EqualTo(3f));
            Assert.That(timer.HasExpired, Is.False);
        }

        [Test]
        public void Reaching_the_duration_expires_it_once_and_raises_the_event()
        {
            var timer = new CountdownTimer(5f);
            var raised = 0;
            timer.Expired += () => raised++;
            timer.Restart();

            var firstTick = timer.Advance(3f);
            var secondTick = timer.Advance(3f);
            var afterExpiry = timer.Advance(3f);

            Assert.That(firstTick, Is.False);
            Assert.That(secondTick, Is.True);
            Assert.That(afterExpiry, Is.False);
            Assert.That(raised, Is.EqualTo(1));
            Assert.That(timer.HasExpired, Is.True);
            Assert.That(timer.IsRunning, Is.False);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void Restarting_after_expiry_counts_down_again()
        {
            var timer = new CountdownTimer(2f);
            var raised = 0;
            timer.Expired += () => raised++;
            timer.Restart();
            timer.Advance(3f);

            timer.Restart();
            timer.Advance(3f);

            Assert.That(raised, Is.EqualTo(2));
        }

        [Test]
        public void Restarting_before_expiry_goes_back_to_the_full_duration()
        {
            var timer = new CountdownTimer(5f);
            timer.Restart();
            timer.Advance(4f);

            timer.Restart();

            Assert.That(timer.RemainingSeconds, Is.EqualTo(5f));
            Assert.That(timer.HasExpired, Is.False);
        }

        [Test]
        public void A_paused_countdown_ignores_ticks_until_it_is_resumed()
        {
            var timer = new CountdownTimer(5f);
            timer.Restart();
            timer.Advance(1f);

            timer.Pause();
            timer.Advance(100f);
            var whilePaused = timer.ElapsedSeconds;
            timer.Resume();
            timer.Advance(1f);

            Assert.That(whilePaused, Is.EqualTo(1f));
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(2f));
            Assert.That(timer.IsPaused, Is.False);
        }

        [Test]
        public void A_stopped_countdown_ignores_ticks_and_cannot_be_resumed()
        {
            var timer = new CountdownTimer(5f);
            timer.Restart();
            timer.Advance(1f);

            timer.Stop();
            timer.Resume();
            timer.Advance(10f);

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(1f));
            Assert.That(timer.HasExpired, Is.False);
        }

        [Test]
        public void A_zero_length_countdown_expires_on_the_first_tick()
        {
            var timer = new CountdownTimer(0f);
            timer.Restart();

            Assert.That(timer.Advance(0.01f), Is.True);
        }

        [Test]
        public void Ticks_of_zero_or_less_do_nothing()
        {
            var timer = new CountdownTimer(1f);
            timer.Restart();

            timer.Advance(0f);
            timer.Advance(-1f);

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void A_negative_duration_is_refused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(-1f));
        }

        [Test]
        public void A_listener_can_restart_the_countdown_from_the_expired_event()
        {
            var timer = new CountdownTimer(1f);
            var raised = 0;
            timer.Expired += () =>
            {
                raised++;
                timer.Restart();
            };
            timer.Restart();

            timer.Advance(1f);
            timer.Advance(1f);

            Assert.That(raised, Is.EqualTo(2));
        }
    }
}
