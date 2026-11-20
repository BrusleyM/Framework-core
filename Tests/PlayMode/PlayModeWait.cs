using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Framework.Tests
{
    public static class PlayModeWait
    {
        public const float DefaultTimeoutSeconds = 10f;

        public static IEnumerator Until(Func<bool> condition, string description, float timeoutSeconds = DefaultTimeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Assert.Fail($"Timed out after {timeoutSeconds} seconds waiting until {description}.");
                }

                yield return null;
            }
        }

        public static WaitForSecondsRealtime WhileNothingShouldHappen(float seconds)
        {
            return new WaitForSecondsRealtime(seconds);
        }
    }
}
