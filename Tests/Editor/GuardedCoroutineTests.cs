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
        public void Disposing_the_guarded_routine_early_disposes_the_inner_routine_so_its_finally_block_runs()
        {
            var finallyRan = false;
            IEnumerator WithFinally()
            {
                try
                {
                    yield return 1;
                    yield return 2;
                }
                finally
                {
                    finallyRan = true;
                }
            }
            var guarded = GuardedCoroutine.Run(WithFinally(), null);
            guarded.MoveNext();

            ((IDisposable)guarded).Dispose();

            Assert.That(finallyRan, Is.True);
        }

        private sealed class WaitForChecks : CustomYieldInstruction
        {
            private int _remaining;

            public WaitForChecks(int checks)
            {
                _remaining = checks;
            }

            public override bool keepWaiting => _remaining-- > 0;
        }

        [Test]
        public void A_custom_yield_instruction_is_waited_on_one_step_at_a_time_and_then_the_routine_continues()
        {
            var journal = new List<string>();
            IEnumerator Waiting()
            {
                journal.Add("before");
                yield return new WaitForChecks(3);
                journal.Add("after");
            }

            var yielded = Drain(GuardedCoroutine.Run(Waiting(), _ => Assert.Fail("no failure expected")));

            Assert.That(yielded.Count, Is.EqualTo(3));
            CollectionAssert.AreEqual(new[] { "before", "after" }, journal);
        }

        [Test]
        public void A_failure_in_a_nested_routine_is_reported_to_the_failure_handler()
        {
            IEnumerator Outer()
            {
                yield return 1;
                yield return FailsOnTheSecondStep();
                yield return 3;
            }
            var failures = new List<Exception>();

            var yielded = Drain(GuardedCoroutine.Run(Outer(), failures.Add));

            CollectionAssert.AreEqual(new object[] { 1, 1 }, yielded);
            Assert.That(failures.Count, Is.EqualTo(1));
        }

        [Test]
        public void A_nested_routine_that_succeeds_hands_control_back_to_the_outer_routine()
        {
            IEnumerator Inner()
            {
                yield return "inner";
            }
            IEnumerator Outer()
            {
                yield return "before";
                yield return Inner();
                yield return "after";
            }

            var yielded = Drain(GuardedCoroutine.Run(Outer(), _ => Assert.Fail("no failure expected")));

            CollectionAssert.AreEqual(new object[] { "before", "inner", "after" }, yielded);
        }

        [Test]
        public void When_the_failure_handler_also_throws_both_exceptions_are_logged()
        {
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("second step failed"));
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("handler failed"));

            Assert.DoesNotThrow(() => Drain(GuardedCoroutine.Run(FailsOnTheSecondStep(), _ => throw new InvalidOperationException("handler failed"))));
        }
    }
}
