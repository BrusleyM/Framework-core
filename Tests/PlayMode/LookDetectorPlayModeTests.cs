using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Framework.Tests
{
    public class LookDetectorPlayModeTests
    {
        private const float SampleSeconds = 0.03f;
        private const float SettleSeconds = 0.4f;

        private readonly List<GameObject> _created = new List<GameObject>();
        private GameObject _eyes;
        private LookDetector _detector;
        private LookTarget _ahead;
        private int _started;
        private int _foundCount;
        private int _allFoundCount;
        private readonly List<LookStopReason> _stopReasons = new List<LookStopReason>();

        [SetUp]
        public void SetUp()
        {
            _eyes = Track(new GameObject("eyes"));
            _detector = _eyes.AddComponent<LookDetector>();
            SetPrivate(_detector, "_sampleIntervalSeconds", SampleSeconds);
            _ahead = CreateTarget("ahead", new Vector3(0f, 0f, 5f));
            SetTargets(_ahead);
            _started = 0;
            _foundCount = 0;
            _allFoundCount = 0;
            _stopReasons.Clear();
            _detector.OnLookingStarted.AddListener(() => _started++);
            _detector.OnTargetFound.AddListener(_ => _foundCount++);
            _detector.OnAllTargetsFound.AddListener(() => _allFoundCount++);
            _detector.OnLookingStopped.AddListener(reason => _stopReasons.Add(reason));
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            _detector.OnLookingStarted.RemoveAllListeners();
            _detector.OnTargetFound.RemoveAllListeners();
            _detector.OnAllTargetsFound.RemoveAllListeners();
            _detector.OnLookingStopped.RemoveAllListeners();
            foreach (var created in _created)
            {
                Object.Destroy(created);
            }
            _created.Clear();
        }

        [UnityTest]
        public IEnumerator Looking_at_a_clear_target_finds_it_and_stops_for_that_reason()
        {
            yield return null;

            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(_ahead.IsSeen, Is.True);
            Assert.That(_foundCount, Is.EqualTo(1));
            Assert.That(_detector.IsLooking, Is.False);
            CollectionAssert.AreEqual(new[] { LookStopReason.AnyTargetSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator A_wall_between_the_eyes_and_the_target_blocks_the_look()
        {
            CreateWall(new Vector3(0f, 0f, 2.5f));
            yield return null;

            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(_ahead.IsSeen, Is.False);
            Assert.That(_detector.IsLooking, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_wall_that_is_removed_lets_the_look_through()
        {
            var wall = CreateWall(new Vector3(0f, 0f, 2.5f));
            yield return null;
            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Object.Destroy(wall);
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(_ahead.IsSeen, Is.True);
        }

        [UnityTest]
        public IEnumerator A_target_outside_the_cone_is_not_found()
        {
            _ahead.transform.position = new Vector3(5f, 0f, 1f);
            Physics.SyncTransforms();
            yield return null;

            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(_ahead.IsSeen, Is.False);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_target_farther_than_the_maximum_distance_is_not_found()
        {
            SetPrivate(_detector, "_maxDistance", 3f);
            yield return null;

            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(_ahead.IsSeen, Is.False);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_glance_shorter_than_the_dwell_time_does_not_count()
        {
            SetPrivate(_ahead, "_dwellSeconds", 0.5f);
            yield return null;
            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(0.15f);

            _eyes.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            yield return new WaitForSecondsRealtime(0.6f);

            Assert.That(_ahead.IsSeen, Is.False);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Looking_for_the_whole_dwell_time_finds_the_target()
        {
            SetPrivate(_ahead, "_dwellSeconds", 0.2f);
            yield return null;

            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(0.2f + SettleSeconds);

            Assert.That(_ahead.IsSeen, Is.True);
        }

        [UnityTest]
        public IEnumerator With_the_all_rule_the_run_continues_until_every_target_is_seen()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.WhenAllTargetsAreSeen);
            var side = CreateTarget("side", new Vector3(5f, 0f, 5f));
            SetTargets(_ahead, side);
            Physics.SyncTransforms();
            yield return null;
            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);
            var stillLookingWithOneMissing = _detector.IsLooking;
            var foundWithOneMissing = _foundCount;

            _eyes.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(foundWithOneMissing, Is.EqualTo(1));
            Assert.That(stillLookingWithOneMissing, Is.True);
            Assert.That(_foundCount, Is.EqualTo(2));
            Assert.That(_allFoundCount, Is.EqualTo(1));
            Assert.That(_detector.IsLooking, Is.False);
            CollectionAssert.AreEqual(new[] { LookStopReason.AllTargetsSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Stopping_on_request_raises_the_stopped_event_and_ends_the_look_of_the_target_being_looked_at()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            var ended = 0;
            _ahead.OnLookEnded.AddListener(() => ended++);
            yield return null;
            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            _detector.StopLooking();

            CollectionAssert.AreEqual(new[] { LookStopReason.Requested }, _stopReasons);
            Assert.That(ended, Is.EqualTo(1));
            Assert.That(_detector.IsLooking, Is.False);
        }

        [UnityTest]
        public IEnumerator Disabling_the_detector_stops_the_run_with_the_disabled_reason()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            yield return null;
            _detector.StartLooking();
            yield return null;

            _detector.enabled = false;

            CollectionAssert.AreEqual(new[] { LookStopReason.Disabled }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Starting_twice_raises_the_started_event_once()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            yield return null;

            _detector.StartLooking();
            _detector.StartLooking();

            Assert.That(_started, Is.EqualTo(1));
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_target_added_while_looking_is_found()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetTargets();
            yield return null;
            _detector.StartLooking();
            yield return null;

            _detector.AddTarget(_ahead);
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(_ahead.IsSeen, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_second_run_can_find_the_same_target_again()
        {
            yield return null;
            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            _detector.StartLooking();
            yield return new WaitForSecondsRealtime(SettleSeconds);

            Assert.That(_foundCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator Sampling_allocates_no_managed_memory()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetPrivate(_detector, "_sampleIntervalSeconds", 1000f);
            yield return null;
            _detector.StartLooking();
            _detector.SampleOnce(0.1f);

            Assert.That(() => _detector.SampleOnce(0.1f), new NUnit.Framework.Constraints.NotConstraint(UnityEngine.TestTools.Constraints.Is.AllocatingGCMemory()));
            _detector.StopLooking();
        }

        private GameObject Track(GameObject created)
        {
            _created.Add(created);
            return created;
        }

        private LookTarget CreateTarget(string name, Vector3 position)
        {
            var cube = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            cube.name = name;
            cube.transform.position = position;
            return cube.AddComponent<LookTarget>();
        }

        private GameObject CreateWall(Vector3 position)
        {
            var wall = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            wall.name = "wall";
            wall.transform.position = position;
            wall.transform.localScale = new Vector3(4f, 4f, 0.2f);
            Physics.SyncTransforms();
            return wall;
        }

        private void SetTargets(params LookTarget[] targets)
        {
            SetPrivate(_detector, "_targets", new List<LookTarget>(targets));
        }

        private static void SetPrivate(object owner, string fieldName, object value)
        {
            var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(owner, value);
        }
    }
}
