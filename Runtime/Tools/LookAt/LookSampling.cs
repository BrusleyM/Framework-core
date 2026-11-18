using UnityEngine;

namespace Framework
{
    public static class LookSampling
    {
        private const float LongestAcceptedStepInIntervals = 2f;

        public static float ClampedDelta(float elapsedSeconds, float sampleIntervalSeconds)
        {
            return Mathf.Clamp(elapsedSeconds, 0f, sampleIntervalSeconds * LongestAcceptedStepInIntervals);
        }
    }
}
