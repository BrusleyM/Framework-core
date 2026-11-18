using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Framework.Tests
{
    public class GuardedCoroutineTests
    {
        private static IEnumerator ThreeSteps(List<string> journal)
        {
            journal.Add("one");
            yield return 1;
            journal.Add("two");
            yield return 2;
            journal.Add("three");
        }

        private static IEnumerator FailsOnTheSecondStep()
        {
            yield return 1;
            throw new InvalidOperationException("second step failed");
        }

        private static List<object> Drain(IEnumerator routine)
        {
            var yielded = new List<object>();
            while (routine.MoveNext())
            {
                yielded.Add(routine.Current);
            }
            return yielded;
        }

        [Test]
        public void A_routine_that_succeeds_yields_the_same_values_and_finishes()
        {
            var journal = new List<string>();

            var yielded = Drain(GuardedCoroutine.Run(ThreeSteps(journal), _ => Assert.Fail("no failure expected")));

            CollectionAssert.AreEqual(new object[] { 1, 2 }, yielded);
            CollectionAssert.AreEqual(new[] { "one", "two", "three" }, journal);
        }

        [Test]
        public void A_failure_is_reported_once_and_ends_the_routine()
        {
            var failures = new List<Exception>();

            var yielded = Drain(GuardedCoroutine.Run(FailsOnTheSecondStep(), failures.Add));

            CollectionAssert.AreEqual(new object[] { 1 }, yielded);
            Assert.That(failures.Count, Is.EqualTo(1));
            Assert.That(failures[0].Message, Is.EqualTo("second step failed"));
        }

        [Test]
        public void Without_a_failure_handler_the_exception_is_logged_and_not_thrown()
        {
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("second step failed"));

            Assert.DoesNotThrow(() => Drain(GuardedCoroutine.Run(FailsOnTheSecondStep(), null)));
        }

        [Test]
        public void A_failure_handler_that_throws_is_logged_and_not_propagated()
        {
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("handler failed"));

            Assert.DoesNotThrow(() => Drain(GuardedCoroutine.Run(FailsOnTheSecondStep(), _ => throw new InvalidOperationException("handler failed"))));
        }

        [Test]
        public void A_routine_that_finishes_is_disposed_so_its_finally_blocks_run()
        {
            var finallyRan = false;
            IEnumerator WithFinally()
            {
                try
                {
                    yield return 1;
                }
                finally
                {
                    finallyRan = true;
                }
            }

            Drain(GuardedCoroutine.Run(WithFinally(), null));

            Assert.That(finallyRan, Is.True);
        }

        [Test]
        public void A_failing_routine_is_disposed_too()
        {
            var finallyRan = false;
            IEnumerator FailsWithFinally()
            {
                try
                {
                    yield return 1;
                    throw new InvalidOperationException("boom");
                }
                finally
                {
                    finallyRan = true;
                }
            }

            Drain(GuardedCoroutine.Run(FailsWithFinally(), _ => { }));

            Assert.That(finallyRan, Is.True);
        }
    }
}
