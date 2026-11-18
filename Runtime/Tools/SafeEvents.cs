using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Framework
{
    public static class SafeEvents
    {
        public static void Invoke(UnityEvent unityEvent, Object context)
        {
            if (unityEvent == null)
            {
                return;
            }

            try
            {
                unityEvent.Invoke();
            }
            catch (Exception failure)
            {
                Debug.LogException(failure, context);
            }
        }

        public static void Invoke<T>(UnityEvent<T> unityEvent, T argument, Object context)
        {
            if (unityEvent == null)
            {
                return;
            }

            try
            {
                unityEvent.Invoke(argument);
            }
            catch (Exception failure)
            {
                Debug.LogException(failure, context);
            }
        }

        public static void Invoke(Action action, Object context)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception failure)
            {
                Debug.LogException(failure, context);
            }
        }

        public static void Invoke<T>(Action<T> action, T argument, Object context)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                action(argument);
            }
            catch (Exception failure)
            {
                Debug.LogException(failure, context);
            }
        }
    }
}
