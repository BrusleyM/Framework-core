using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    public static class GuardedCoroutine
    {
        public static IEnumerator Run(IEnumerator routine, Action<Exception> onFailure, Object context = null)
        {
            var pending = new Stack<IEnumerator>();
            pending.Push(routine);
            try
            {
                while (pending.Count > 0)
                {
                    var current = pending.Peek();
                    object yielded = null;
                    var hasValue = false;
                    var failed = false;
                    try
                    {
                        hasValue = current.MoveNext();
                        if (hasValue)
                        {
                            yielded = current.Current;
                        }
                    }
                    catch (Exception failure)
                    {
                        failed = true;
                        Report(failure, onFailure, context);
                    }

                    if (failed)
                    {
                        yield break;
                    }

                    if (!hasValue)
                    {
                        pending.Pop();
                        (current as IDisposable)?.Dispose();
                    }
                    else if (yielded is IEnumerator nested)
                    {
                        pending.Push(nested);
                    }
                    else
                    {
                        yield return yielded;
                    }
                }
            }
            finally
            {
                while (pending.Count > 0)
                {
                    (pending.Pop() as IDisposable)?.Dispose();
                }
            }
        }

        private static void Report(Exception failure, Action<Exception> onFailure, Object context)
        {
            if (onFailure == null)
            {
                Debug.LogException(failure, context);
                return;
            }

            try
            {
                onFailure(failure);
            }
            catch (Exception handlerFailure)
            {
                Debug.LogException(failure, context);
                Debug.LogException(handlerFailure, context);
            }
        }
    }
}
