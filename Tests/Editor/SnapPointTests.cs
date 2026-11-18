using NUnit.Framework;
using UnityEngine;

namespace Framework.Tests
{
    public class SnapPointTests
    {
        private GameObject _pointObject;
        private GameObject _snappableObject;
        private SnapKey _key;
        private SnapPoint _point;
        private Snappable _snappable;

        [SetUp]
        public void SetUp()
        {
            _key = ScriptableObject.CreateInstance<SnapKey>();
            _pointObject = new GameObject("point");
            _pointObject.transform.position = new Vector3(3f, 1f, 0f);
            _point = _pointObject.AddComponent<SnapPoint>();
            SetPrivate(_point, "_acceptedKey", _key);
            SetPrivate(_point, "_snapDurationSeconds", 0f);
            _snappableObject = new GameObject("snappable");
            _snappable = _snappableObject.AddComponent<Snappable>();
            SetPrivate(_snappable, "_key", _key);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_pointObject);
            Object.DestroyImmediate(_snappableObject);
            Object.DestroyImmediate(_key);
        }

        [Test]
        public void An_object_with_the_accepted_key_snaps_to_the_point_position()
        {
            _point.TryOffer(_snappable);

            Assert.That(_snappableObject.transform.position, Is.EqualTo(_pointObject.transform.position));
        }

        [Test]
        public void An_object_with_another_key_is_refused()
        {
            var otherKey = ScriptableObject.CreateInstance<SnapKey>();
            SetPrivate(_snappable, "_key", otherKey);

            var accepted = _point.TryOffer(_snappable);

            Assert.That(accepted, Is.False);
            Object.DestroyImmediate(otherKey);
        }

        [Test]
        public void A_point_without_an_accepted_key_refuses_everything()
        {
            SetPrivate(_point, "_acceptedKey", null);

            Assert.That(_point.TryOffer(_snappable), Is.False);
        }

        [Test]
        public void An_occupied_point_refuses_a_second_object()
        {
            _point.TryOffer(_snappable);
            var second = new GameObject("second").AddComponent<Snappable>();
            SetPrivate(second, "_key", _key);

            var accepted = _point.TryOffer(second);

            Assert.That(accepted, Is.False);
            Object.DestroyImmediate(second.gameObject);
        }

        [Test]
        public void Releasing_frees_the_point_for_the_next_object()
        {
            _point.TryOffer(_snappable);

            _point.Release();

            Assert.That(_point.IsOccupied, Is.False);
            Assert.That(_snappable.IsSnapped, Is.False);
        }

        [Test]
        public void Releasing_an_empty_point_reports_nothing_was_released()
        {
            Assert.That(_point.Release(), Is.False);
        }

        [Test]
        public void The_snapped_event_carries_the_snapped_object()
        {
            Snappable reported = null;
            _point.OnSnapped.AddListener(snapped => reported = snapped);

            _point.TryOffer(_snappable);

            Assert.That(reported, Is.SameAs(_snappable));
        }

        [Test]
        public void The_released_event_carries_the_released_object()
        {
            Snappable reported = null;
            _point.OnReleased.AddListener(released => reported = released);
            _point.TryOffer(_snappable);

            _point.Release();

            Assert.That(reported, Is.SameAs(_snappable));
        }

        [Test]
        public void A_rigidbody_is_kinematic_while_snapped_and_restored_on_release()
        {
            var body = _snappableObject.AddComponent<Rigidbody>();

            _point.TryOffer(_snappable);
            var kinematicWhileSnapped = body.isKinematic;
            _point.Release();

            Assert.That(kinematicWhileSnapped, Is.True);
            Assert.That(body.isKinematic, Is.False);
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(target, value);
        }
    }
}
