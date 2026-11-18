using System;
using System.Collections;
using UnityEngine;

namespace Framework.Tests
{
    public static class PlayModeWait
    {
        public static IEnumerator Until(Func<bool> condition, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
        }
    }
}
