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
        private const float StepSeconds = 0.1f;
        private const float ManualSamplingInterval = 1000f;

        private readonly List<GameObject> _created = new List<GameObject>();
        private readonly List<LookStopReason> _stopReasons = new List<LookStopReason>();
        private GameObject _eyes;
        private LookDetector _detector;
        private LookTarget _ahead;
        private int _started;
        private int _foundCount;
        private int _allFoundCount;

        [SetUp]
        public void SetUp()
        {
            _eyes = Track(new GameObject("eyes"));
            _detector = _eyes.AddComponent<LookDetector>();
            SetPrivate(_detector, "_sampleIntervalSeconds", ManualSamplingInterval);
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
        public IEnumerator The_real_time_sampling_loop_finds_a_clear_target_and_stops()
        {
            SetPrivate(_detector, "_sampleIntervalSeconds", 0.03f);
            yield return null;

            _detector.StartLooking();
            yield return PlayModeWait.Until(() => _stopReasons.Count == 1, "the detector stops after seeing the target");
            yield return null;

            Assert.That(_ahead.IsSeen, Is.True);
            CollectionAssert.AreEqual(new[] { LookStopReason.AnyTargetSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Looking_at_a_clear_target_finds_it_and_stops_for_that_reason()
        {
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

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

            _detector.SampleOnce(StepSeconds);

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
            _detector.SampleOnce(StepSeconds);

            Object.Destroy(wall);
            yield return null;
            Physics.SyncTransforms();
            _detector.SampleOnce(StepSeconds);

            Assert.That(_ahead.IsSeen, Is.True);
        }

        [UnityTest]
        public IEnumerator A_collider_on_the_ignore_raycast_layer_does_not_block_the_look()
        {
            var wall = CreateWall(new Vector3(0f, 0f, 2.5f));
            wall.layer = 2;
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(_ahead.IsSeen, Is.True);
        }

        [UnityTest]
        public IEnumerator A_target_outside_the_cone_is_not_found()
        {
            _ahead.transform.position = new Vector3(5f, 0f, 1f);
            Physics.SyncTransforms();
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(_ahead.IsSeen, Is.False);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_target_farther_than_the_maximum_distance_is_not_found()
        {
            SetPrivate(_detector, "_maxDistance", 3f);
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(_ahead.IsSeen, Is.False);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator An_aim_point_on_a_wall_surface_is_not_blocked_by_that_wall()
        {
            _ahead.gameObject.SetActive(false);
            var wall = CreateWall(new Vector3(0f, 0f, 5f));
            var aim = Track(new GameObject("poster")).transform;
            aim.position = new Vector3(0f, 0f, 4.9f);
            var poster = aim.gameObject.AddComponent<LookTarget>();
            SetTargets(poster);
            Physics.SyncTransforms();
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(poster.IsSeen, Is.True);
            Assert.That(wall, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator A_collider_on_a_parent_of_the_target_counts_as_the_target_not_a_blocker()
        {
            var parent = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            parent.transform.position = new Vector3(0f, 0f, 5f);
            var child = Track(new GameObject("on the parent"));
            child.transform.SetParent(parent.transform, true);
            var target = child.AddComponent<LookTarget>();
            SetPrivate(target, "_extraColliders", new[] { parent.GetComponent<Collider>() });
            SetTargets(target);
            Physics.SyncTransforms();
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(target.IsSeen, Is.True);
        }

        [UnityTest]
        public IEnumerator A_wall_that_is_a_parent_of_the_target_still_blocks_when_it_is_not_listed_as_part_of_the_target()
        {
            var wall = CreateWall(new Vector3(0f, 0f, 2.5f));
            var child = Track(new GameObject("behind its parent wall"));
            child.transform.SetParent(wall.transform, true);
            child.transform.position = new Vector3(0f, 0f, 5f);
            var target = child.AddComponent<LookTarget>();
            SetTargets(target);
            Physics.SyncTransforms();
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(target.IsSeen, Is.False);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_glance_shorter_than_the_dwell_time_does_not_count()
        {
            SetPrivate(_ahead, "_dwellSeconds", 0.5f);
            yield return null;
            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);
            _detector.SampleOnce(StepSeconds);

            _eyes.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            _detector.SampleOnce(StepSeconds);
            _eyes.transform.rotation = Quaternion.identity;
            _detector.SampleOnce(StepSeconds);
            _detector.SampleOnce(StepSeconds);

            Assert.That(_ahead.IsSeen, Is.False);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Looking_for_the_whole_dwell_time_finds_the_target()
        {
            SetPrivate(_ahead, "_dwellSeconds", 0.25f);
            yield return null;
            _detector.StartLooking();

            for (var step = 0; step < 5; step++)
            {
                _detector.SampleOnce(StepSeconds);
            }

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
            _detector.SampleOnce(StepSeconds);
            var stillLookingWithOneMissing = _detector.IsLooking;
            var foundWithOneMissing = _foundCount;

            _eyes.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            _detector.SampleOnce(StepSeconds);

            Assert.That(foundWithOneMissing, Is.EqualTo(1));
            Assert.That(stillLookingWithOneMissing, Is.True);
            Assert.That(_foundCount, Is.EqualTo(2));
            Assert.That(_allFoundCount, Is.EqualTo(1));
            Assert.That(_detector.IsLooking, Is.False);
            CollectionAssert.AreEqual(new[] { LookStopReason.AllTargetsSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator A_target_listed_twice_counts_once_so_the_all_rule_can_finish()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.WhenAllTargetsAreSeen);
            SetTargets(_ahead, _ahead);
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            CollectionAssert.AreEqual(new[] { LookStopReason.AllTargetsSeen }, _stopReasons);
            Assert.That(_foundCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator A_target_listed_twice_fills_its_dwell_at_the_normal_speed()
        {
            SetPrivate(_ahead, "_dwellSeconds", 0.25f);
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetTargets(_ahead, _ahead);
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);
            _detector.SampleOnce(StepSeconds);
            _detector.SampleOnce(StepSeconds);

            Assert.That(_ahead.IsSeen, Is.False);
            _detector.SampleOnce(StepSeconds);
            Assert.That(_ahead.IsSeen, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator An_empty_slot_in_the_list_does_not_stop_the_all_rule_from_finishing()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.WhenAllTargetsAreSeen);
            SetTargets(null, _ahead);
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            CollectionAssert.AreEqual(new[] { LookStopReason.AllTargetsSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator A_destroyed_target_does_not_stop_the_all_rule_from_finishing()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.WhenAllTargetsAreSeen);
            var doomed = CreateTarget("doomed", new Vector3(0f, 0f, 6f));
            SetTargets(_ahead, doomed);
            yield return null;
            _detector.StartLooking();
            Object.Destroy(doomed.gameObject);
            yield return null;

            _detector.SampleOnce(StepSeconds);

            CollectionAssert.AreEqual(new[] { LookStopReason.AllTargetsSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Removing_the_only_unseen_target_lets_the_all_rule_finish_on_the_next_sample()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.WhenAllTargetsAreSeen);
            var hidden = CreateTarget("hidden", new Vector3(5f, 0f, 1f));
            SetTargets(_ahead, hidden);
            Physics.SyncTransforms();
            yield return null;
            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);

            _detector.RemoveTarget(hidden);
            _detector.SampleOnce(StepSeconds);

            CollectionAssert.AreEqual(new[] { LookStopReason.AllTargetsSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Stopping_from_a_found_listener_leaves_the_remaining_targets_untouched()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetPrivate(_detector, "_maxAngleDegrees", 60f);
            var other = CreateTarget("other", new Vector3(2f, 0f, 4f));
            var otherStarted = 0;
            other.OnLookStarted.AddListener(() => otherStarted++);
            SetTargets(_ahead, other);
            Physics.SyncTransforms();
            yield return null;
            _detector.OnTargetFound.AddListener(_ => _detector.StopLooking());
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(otherStarted, Is.EqualTo(0));
            Assert.That(other.IsSeen, Is.False);
            Assert.That(_allFoundCount, Is.EqualTo(0));
            CollectionAssert.AreEqual(new[] { LookStopReason.Requested }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Removing_a_target_from_its_own_found_listener_does_not_skip_the_next_target()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetPrivate(_detector, "_maxAngleDegrees", 60f);
            var other = CreateTarget("other", new Vector3(2f, 0f, 4f));
            SetTargets(_ahead, other);
            Physics.SyncTransforms();
            yield return null;
            _detector.OnTargetFound.AddListener(found => _detector.RemoveTarget(found));
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(other.IsSeen, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_look_ended_listener_that_removes_a_target_does_not_break_stopping()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetPrivate(_detector, "_maxAngleDegrees", 60f);
            var other = CreateTarget("other", new Vector3(2f, 0f, 4f));
            SetTargets(_ahead, other);
            Physics.SyncTransforms();
            yield return null;
            _ahead.OnLookEnded.AddListener(() => _detector.RemoveTarget(other));
            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);

            Assert.DoesNotThrow(() => _detector.StopLooking());
            CollectionAssert.AreEqual(new[] { LookStopReason.Requested }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Removing_the_found_target_from_its_listener_still_stops_the_any_rule()
        {
            yield return null;
            _detector.OnTargetFound.AddListener(found => _detector.RemoveTarget(found));
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            CollectionAssert.AreEqual(new[] { LookStopReason.AnyTargetSeen }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator Starting_again_from_a_look_ended_listener_during_a_stop_leaves_the_new_run_running()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            yield return null;
            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);
            var restarted = false;
            _ahead.OnLookEnded.AddListener(() =>
            {
                if (!restarted)
                {
                    restarted = true;
                    _detector.StartLooking();
                }
            });

            _detector.StopLooking();

            Assert.That(_stopReasons.Count, Is.EqualTo(1));
            Assert.That(_started, Is.EqualTo(2));
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_sample_requested_from_inside_a_sample_is_ignored()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetPrivate(_detector, "_maxAngleDegrees", 60f);
            var other = CreateTarget("other", new Vector3(2f, 0f, 4f));
            SetTargets(_ahead, other);
            Physics.SyncTransforms();
            var otherWasLookedAtDuringTheListener = true;
            _ahead.OnLookStarted.AddListener(() =>
            {
                _detector.SampleOnce(StepSeconds);
                otherWasLookedAtDuringTheListener = other.IsBeingLookedAt;
            });
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(otherWasLookedAtDuringTheListener, Is.False);
            Assert.That(other.IsBeingLookedAt, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Stopping_on_request_raises_the_stopped_event_and_ends_the_look_of_the_target_being_looked_at()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            var ended = 0;
            _ahead.OnLookEnded.AddListener(() => ended++);
            yield return null;
            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);

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
        public IEnumerator Starting_while_inactive_warns_and_does_not_leave_the_detector_stuck()
        {
            yield return null;
            _eyes.SetActive(false);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("cannot start looking"));

            _detector.StartLooking();
            var stuck = _detector.IsLooking;
            _eyes.SetActive(true);
            _detector.StartLooking();

            Assert.That(stuck, Is.False);
            Assert.That(_detector.IsLooking, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Start_on_enable_starts_a_run_each_time_the_detector_is_enabled()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetPrivate(_detector, "_startOnEnable", true);
            yield return null;

            _detector.enabled = false;
            _detector.enabled = true;

            Assert.That(_started, Is.EqualTo(1));
            Assert.That(_detector.IsLooking, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Stopping_and_starting_again_from_the_started_listener_ends_with_a_running_detector()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            var restarts = 0;
            _detector.OnLookingStarted.AddListener(() =>
            {
                if (restarts++ == 0)
                {
                    _detector.StopLooking();
                    _detector.StartLooking();
                }
            });
            yield return null;

            Assert.DoesNotThrow(() => _detector.StartLooking());

            Assert.That(_detector.IsLooking, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_target_added_while_looking_is_found()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetTargets();
            yield return null;
            _detector.StartLooking();

            _detector.AddTarget(_ahead);
            _detector.SampleOnce(StepSeconds);

            Assert.That(_ahead.IsSeen, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Adding_the_same_or_an_empty_target_is_refused()
        {
            yield return null;

            Assert.That(_detector.AddTarget(_ahead), Is.False);
            Assert.That(_detector.AddTarget(null), Is.False);
        }

        [UnityTest]
        public IEnumerator The_all_targets_found_event_is_raised_once_however_long_the_run_continues()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            yield return null;
            _detector.StartLooking();

            for (var step = 0; step < 5; step++)
            {
                _detector.SampleOnce(StepSeconds);
            }

            Assert.That(_allFoundCount, Is.EqualTo(1));
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator A_second_run_can_find_the_same_target_again()
        {
            yield return null;
            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);

            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);

            Assert.That(_foundCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator A_look_target_listener_that_flips_the_gaze_back_and_forth_does_not_recurse_without_end()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            var started = 0;
            _ahead.OnLookStarted.AddListener(() =>
            {
                started++;
                _ahead.ReportGaze(false, StepSeconds);
                _ahead.ReportGaze(true, StepSeconds);
            });
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("kept retriggering"));
            yield return null;
            _detector.StartLooking();

            Assert.DoesNotThrow(() => _detector.SampleOnce(StepSeconds));

            Assert.That(started, Is.GreaterThan(0));
            Assert.That(started, Is.LessThan(10));
            Assert.That(_ahead.IsBeingLookedAt, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Stopping_the_run_from_the_seen_listener_still_ends_the_look_of_that_target()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            var ended = 0;
            _ahead.OnSeen.AddListener(() => _detector.StopLooking());
            _ahead.OnLookEnded.AddListener(() => ended++);
            yield return null;
            _detector.StartLooking();

            _detector.SampleOnce(StepSeconds);

            Assert.That(ended, Is.EqualTo(1));
            Assert.That(_ahead.IsBeingLookedAt, Is.False);
            CollectionAssert.AreEqual(new[] { LookStopReason.Requested }, _stopReasons);
        }

        [UnityTest]
        public IEnumerator A_seen_listener_that_throws_is_logged_and_the_found_event_and_sampling_still_run()
        {
            SetPrivate(_detector, "_sampleIntervalSeconds", 0.03f);
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            SetPrivate(_detector, "_maxAngleDegrees", 60f);
            var side = CreateTarget("side", new Vector3(2f, 0f, 4f));
            SetTargets(_ahead, side);
            Physics.SyncTransforms();
            _ahead.OnSeen.AddListener(() => throw new System.InvalidOperationException("listener failed"));
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("listener failed"));
            yield return null;

            _detector.StartLooking();
            yield return PlayModeWait.Until(() => side.IsSeen, "the side target is seen", 3f);

            Assert.That(_foundCount, Is.EqualTo(2));
            Assert.That(side.IsSeen, Is.True);
            Assert.That(_detector.IsLooking, Is.True);
            _detector.StopLooking();
        }

        [UnityTest]
        public IEnumerator Sampling_allocates_no_managed_memory()
        {
            SetPrivate(_detector, "_stopRule", LookStopRule.Never);
            _detector.OnTargetFound.AddListener(_ => { });
            yield return null;
            _detector.StartLooking();
            _detector.SampleOnce(StepSeconds);

            Assert.That(() => _detector.SampleOnce(StepSeconds), new NUnit.Framework.Constraints.NotConstraint(UnityEngine.TestTools.Constraints.Is.AllocatingGCMemory()));
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
