using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Framework.Tests
{
    public class SafeEventsTests
    {
        [Test]
        public void A_unity_event_listener_that_throws_is_logged_and_not_propagated()
        {
            var unityEvent = new UnityEvent();
            unityEvent.AddListener(() => throw new InvalidOperationException("listener failed"));
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("listener failed"));

            Assert.DoesNotThrow(() => SafeEvents.Invoke(unityEvent, null));
        }

        [Test]
        public void A_unity_event_with_an_argument_passes_the_argument_to_its_listeners()
        {
            var unityEvent = new UnityEvent<int>();
            var received = 0;
            unityEvent.AddListener(value => received = value);

            SafeEvents.Invoke(unityEvent, 7, null);

            Assert.That(received, Is.EqualTo(7));
        }

        [Test]
        public void A_code_event_that_throws_is_logged_and_not_propagated()
        {
            Action failing = () => throw new InvalidOperationException("subscriber failed");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("subscriber failed"));

            Assert.DoesNotThrow(() => SafeEvents.Invoke(failing, null));
        }

        [Test]
        public void A_missing_code_event_is_ignored()
        {
            Action none = null;
            Action<int> noneWithArgument = null;

            Assert.DoesNotThrow(() => SafeEvents.Invoke(none, null));
            Assert.DoesNotThrow(() => SafeEvents.Invoke(noneWithArgument, 1, null));
        }

        [Test]
        public void A_code_event_with_an_argument_passes_the_argument_to_its_subscribers()
        {
            var received = 0;
            Action<int> subscriber = value => received = value;

            SafeEvents.Invoke(subscriber, 9, null);

            Assert.That(received, Is.EqualTo(9));
        }
    }
}
