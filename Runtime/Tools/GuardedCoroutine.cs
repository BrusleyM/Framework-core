using System;
using System.Collections;
using UnityEngine;

namespace Framework
{
    public static class GuardedCoroutine
    {
        public static IEnumerator Run(IEnumerator routine, Action<Exception> onFailure)
        {
            while (true)
            {
                object current = null;
                var finished = false;
                try
                {
                    finished = !routine.MoveNext();
                    if (!finished)
                    {
                        current = routine.Current;
                    }
                }
                catch (Exception failure)
                {
                    finished = true;
                    Report(failure, onFailure);
                }

                if (finished)
                {
                    (routine as IDisposable)?.Dispose();
                    yield break;
                }

                yield return current;
            }
        }

        private static void Report(Exception failure, Action<Exception> onFailure)
        {
            try
            {
                if (onFailure != null)
                {
                    onFailure(failure);
                }
                else
                {
                    Debug.LogException(failure);
                }
            }
            catch (Exception handlerFailure)
            {
                Debug.LogException(handlerFailure);
            }
        }
    }
}
