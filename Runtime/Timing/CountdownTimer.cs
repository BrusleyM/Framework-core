using System;

namespace Framework.Timing
{
    public sealed class CountdownTimer
    {
        public CountdownTimer(float durationSeconds)
        {
            if (durationSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), "A duration cannot be negative.");
            }

            DurationSeconds = durationSeconds;
        }

        public event Action Expired;

        public float DurationSeconds { get; }
        public float ElapsedSeconds { get; private set; }
        public float RemainingSeconds => Math.Max(0f, DurationSeconds - ElapsedSeconds);
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }
        public bool HasExpired { get; private set; }

        public void Restart()
        {
            ElapsedSeconds = 0f;
            HasExpired = false;
            IsRunning = true;
            IsPaused = false;
        }

        public void Pause()
        {
            if (IsRunning)
            {
                IsRunning = false;
                IsPaused = true;
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                IsRunning = true;
                IsPaused = false;
            }
        }

        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
        }

        public bool Advance(float deltaSeconds)
        {
            if (!IsRunning || deltaSeconds <= 0f)
            {
                return false;
            }

            ElapsedSeconds += deltaSeconds;
            if (ElapsedSeconds < DurationSeconds)
            {
                return false;
            }

            ElapsedSeconds = DurationSeconds;
            HasExpired = true;
            IsRunning = false;
            Expired?.Invoke();
            return true;
        }
    }
}
