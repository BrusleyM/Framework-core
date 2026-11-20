using System;

namespace Framework.Timing
{
    public sealed class ManualTimeSource : ITimeSource
    {
        public double Seconds { get; private set; }

        public void Advance(double seconds)
        {
            if (seconds < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Time cannot go backwards.");
            }

            Seconds += seconds;
        }
    }
}
