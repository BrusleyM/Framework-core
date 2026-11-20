using UnityEngine;

namespace Framework.Timing
{
    public sealed class UnityTimeSource : ITimeSource
    {
        public static readonly UnityTimeSource Instance = new UnityTimeSource();

        public double Seconds => Time.unscaledTimeAsDouble;
    }
}
