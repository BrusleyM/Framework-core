using System;
using NUnit.Framework;

namespace Framework.Timing.Tests
{
    public class ElapsedTimerTests
    {
        private ManualTimeSource _time;
        private ElapsedTimer _timer;

        [SetUp]
        public void SetUp()
        {
            _time = new ManualTimeSource();
            _timer = new ElapsedTimer(_time);
        }

        [Test]
        public void A_new_timer_is_not_running_and_has_measured_nothing()
        {
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(0d));
        }

        [Test]
        public void A_started_timer_measures_the_time_that_passes_without_being_ticked()
        {
            _timer.Start();

            _time.Advance(2.5d);

            Assert.That(_timer.IsRunning, Is.True);
            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(2.5d));
        }

        [Test]
        public void Starting_measures_from_the_moment_it_was_started_not_from_time_zero()
        {
            _time.Advance(100d);

            _timer.Start();
            _time.Advance(3d);

            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(3d));
        }

        [Test]
        public void Stopping_freezes_the_elapsed_time_and_returns_it()
        {
            _timer.Start();
            _time.Advance(4d);

            var stopped = _timer.Stop();
            _time.Advance(10d);

            Assert.That(stopped, Is.EqualTo(4d));
            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(4d));
            Assert.That(_timer.IsRunning, Is.False);
        }

        [Test]
        public void Time_that_passes_while_paused_is_not_counted()
        {
            _timer.Start();
            _time.Advance(1d);

            _timer.Pause();
            _time.Advance(50d);
            _timer.Resume();
            _time.Advance(2d);

            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(3d));
            Assert.That(_timer.IsPaused, Is.False);
        }

        [Test]
        public void A_paused_timer_reports_that_it_is_paused_and_holds_its_time()
        {
            _timer.Start();
            _time.Advance(1d);

            _timer.Pause();
            _time.Advance(5d);

            Assert.That(_timer.IsPaused, Is.True);
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(1d));
        }

        [Test]
        public void Resuming_a_timer_that_is_not_paused_changes_nothing()
        {
            _timer.Start();
            _time.Advance(1d);

            _timer.Resume();
            _time.Advance(1d);

            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(2d));
        }

        [Test]
        public void Starting_again_measures_a_new_run_from_zero()
        {
            _timer.Start();
            _time.Advance(5d);

            _timer.Start();
            _time.Advance(1d);

            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(1d));
        }

        [Test]
        public void Resetting_clears_the_time_and_stops_the_timer()
        {
            _timer.Start();
            _time.Advance(5d);

            _timer.Reset();

            Assert.That(_timer.ElapsedSeconds, Is.EqualTo(0d));
            Assert.That(_timer.IsRunning, Is.False);
        }

        [Test]
        public void The_time_source_cannot_go_backwards()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _time.Advance(-1d));
        }
    }
}
