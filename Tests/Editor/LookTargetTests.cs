using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Framework.Tests
{
    public class LookTargetTests
    {
        private GameObject _host;
        private LookTarget _target;
        private int _started;
        private int _ended;
        private int _seen;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("target");
            _target = _host.AddComponent<LookTarget>();
            _started = 0;
            _ended = 0;
            _seen = 0;
            _target.OnLookStarted.AddListener(() => _started++);
            _target.OnLookEnded.AddListener(() => _ended++);
            _target.OnSeen.AddListener(() => _seen++);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void Looking_at_a_target_with_no_dwell_raises_started_and_seen_once()
        {
            _target.ReportGaze(true, 0.1f);
            _target.ReportGaze(true, 0.1f);

            Assert.That(_started, Is.EqualTo(1));
            Assert.That(_seen, Is.EqualTo(1));
        }

        [Test]
        public void Looking_away_raises_ended_once()
        {
            _target.ReportGaze(true, 0.1f);

            _target.ReportGaze(false, 0.1f);
            _target.ReportGaze(false, 0.1f);

            Assert.That(_ended, Is.EqualTo(1));
        }

        [Test]
        public void The_code_events_fire_with_the_unity_events()
        {
            var codeSeen = 0;
            _target.Seen += () => codeSeen++;

            _target.ReportGaze(true, 0.1f);

            Assert.That(codeSeen, Is.EqualTo(1));
        }

        [Test]
        public void The_aim_point_defaults_to_the_target_transform()
        {
            Assert.That(_target.AimPoint, Is.SameAs(_host.transform));
        }

        [Test]
        public void An_assigned_aim_point_is_used_instead_of_the_target_transform()
        {
            var aim = new GameObject("aim").transform;
            var serialized = new SerializedObject(_target);
            serialized.FindProperty("_aimPoint").objectReferenceValue = aim;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(_target.AimPoint, Is.SameAs(aim));
            Object.DestroyImmediate(aim.gameObject);
        }

        [Test]
        public void Reset_look_lets_the_target_be_seen_again()
        {
            _target.ReportGaze(true, 0.1f);

            _target.ResetLook();
            _target.ReportGaze(true, 0.1f);

            Assert.That(_seen, Is.EqualTo(2));
            Assert.That(_target.IsSeen, Is.True);
        }

        [Test]
        public void A_dwell_time_set_on_the_target_delays_seen()
        {
            var serialized = new SerializedObject(_target);
            serialized.FindProperty("_dwellSeconds").floatValue = 1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _target.ReportGaze(true, 0.1f);
            _target.ReportGaze(true, 0.4f);

            Assert.That(_seen, Is.EqualTo(0));
            Assert.That(_target.LookProgress, Is.EqualTo(0.4f).Within(0.0001f));
        }
    }
}
