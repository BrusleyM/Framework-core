using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Framework.Tests
{
    public class SnapPointPlayModeTests
    {
        private const float SnapSeconds = 0.3f;
        private const float LongSnapSeconds = 1.5f;

        private GameObject _pointObject;
        private GameObject _snappableObject;
        private GameObject _lonely;
        private SnapKey _key;
        private SnapPoint _point;
        private Snappable _snappable;
        private int _snappedCount;
        private int _releasedCount;

        [SetUp]
        public void SetUp()
        {
            _key = ScriptableObject.CreateInstance<SnapKey>();
            _pointObject = new GameObject("point");
            _pointObject.transform.position = new Vector3(4f, 0f, 0f);
            _point = _pointObject.AddComponent<SnapPoint>();
            SetPrivate(_point, "_acceptedKey", _key);
            SetPrivate(_point, "_snapDurationSeconds", SnapSeconds);
            _snappableObject = new GameObject("snappable");
            _snappable = _snappableObject.AddComponent<Snappable>();
            SetPrivate(_snappable, "_key", _key);
            _snappedCount = 0;
            _releasedCount = 0;
            _point.OnSnapped.AddListener(_ => _snappedCount++);
            _point.OnReleased.AddListener(_ => _releasedCount++);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            if (_point != null)
            {
                _point.OnSnapped.RemoveAllListeners();
                _point.OnReleased.RemoveAllListeners();
            }
            Object.Destroy(_pointObject);
            Object.Destroy(_snappableObject);
            Object.Destroy(_lonely);
            Object.Destroy(_key);
        }

        [UnityTest]
        public IEnumerator An_animated_snap_ends_at_the_point_and_raises_the_snapped_event_once()
        {
            _point.TryOffer(_snappable);
            Assert.That(_snappedCount, Is.EqualTo(0));

            yield return new WaitForSecondsRealtime(SnapSeconds + 0.3f);

            Assert.That(_snappableObject.transform.position, Is.EqualTo(_pointObject.transform.position));
            Assert.That(_snappedCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Releasing_during_the_animation_stops_the_object_and_raises_only_the_released_event()
        {
            SetPrivate(_point, "_snapDurationSeconds", LongSnapSeconds);
            _point.TryOffer(_snappable);
            yield return null;
            yield return null;

            _point.Release();
            var positionWhenReleased = _snappableObject.transform.position;
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(_snappableObject.transform.position, Is.EqualTo(positionWhenReleased));
            Assert.That(_snappedCount, Is.EqualTo(0));
            Assert.That(_releasedCount, Is.EqualTo(1));
            Assert.That(_point.IsOccupied, Is.False);
        }

        [UnityTest]
        public IEnumerator Destroying_the_snappable_during_the_animation_frees_the_point_without_errors()
        {
            SetPrivate(_point, "_snapDurationSeconds", LongSnapSeconds);
            _point.TryOffer(_snappable);
            yield return null;

            Object.Destroy(_snappableObject);
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(_point.IsOccupied, Is.False);
            Assert.That(_releasedCount, Is.EqualTo(1));
            Assert.That(_snappedCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Destroying_a_snapped_snappable_raises_the_released_event()
        {
            SetPrivate(_point, "_snapDurationSeconds", 0f);
            _point.TryOffer(_snappable);

            Object.Destroy(_snappableObject);
            yield return null;

            Assert.That(_releasedCount, Is.EqualTo(1));
            Assert.That(_point.IsOccupied, Is.False);
        }

        [UnityTest]
        public IEnumerator Disabling_the_point_during_the_animation_releases_the_object()
        {
            SetPrivate(_point, "_snapDurationSeconds", LongSnapSeconds);
            var body = _snappableObject.AddComponent<Rigidbody>();
            _point.TryOffer(_snappable);
            yield return null;

            _point.enabled = false;
            yield return null;

            Assert.That(_point.IsOccupied, Is.False);
            Assert.That(_snappable.IsSnapped, Is.False);
            Assert.That(body.isKinematic, Is.False);
            Assert.That(_releasedCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Disabling_the_point_after_the_snap_finished_keeps_the_object_snapped()
        {
            _point.TryOffer(_snappable);
            yield return new WaitForSecondsRealtime(SnapSeconds + 0.3f);

            _point.enabled = false;
            yield return null;

            Assert.That(_point.IsOccupied, Is.True);
            Assert.That(_releasedCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator A_point_that_moves_during_the_animation_gets_the_object_at_its_final_position()
        {
            SetPrivate(_point, "_snapDurationSeconds", LongSnapSeconds);
            _point.TryOffer(_snappable);
            yield return null;

            _pointObject.transform.position = new Vector3(0f, 5f, 0f);
            yield return new WaitForSecondsRealtime(LongSnapSeconds + 0.3f);

            Assert.That(Vector3.Distance(_snappableObject.transform.position, _pointObject.transform.position), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator A_paused_time_scale_stops_the_snap_unless_the_point_ignores_it()
        {
            Time.timeScale = 0f;
            _point.TryOffer(_snappable);
            yield return new WaitForSecondsRealtime(SnapSeconds + 0.3f);
            var snappedWhilePaused = _snappedCount;
            _point.Release();

            SetPrivate(_point, "_ignoreTimeScale", true);
            _point.TryOffer(_snappable);
            yield return new WaitForSecondsRealtime(SnapSeconds + 0.3f);

            Assert.That(snappedWhilePaused, Is.EqualTo(0));
            Assert.That(_snappedCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Destroying_the_point_while_it_holds_a_finished_snap_releases_the_object()
        {
            SetPrivate(_point, "_snapDurationSeconds", 0f);
            var body = _snappableObject.AddComponent<Rigidbody>();
            _point.TryOffer(_snappable);

            Object.Destroy(_pointObject);
            yield return null;

            Assert.That(_snappable.IsSnapped, Is.False);
            Assert.That(body.isKinematic, Is.False);
        }

        [UnityTest]
        public IEnumerator A_trigger_contact_offers_the_touching_snappable_to_the_point()
        {
            SetPrivate(_point, "_snapDurationSeconds", 0f);
            _pointObject.AddComponent<BoxCollider>().isTrigger = true;
            var contact = _pointObject.AddComponent<SnapOnTriggerEnter>();
            var touching = _snappableObject.AddComponent<BoxCollider>();
            yield return null;

            contact.SendMessage("OnTriggerEnter", touching);

            Assert.That(_point.Occupant, Is.SameAs(_snappable));
        }

        [UnityTest]
        public IEnumerator A_contact_component_without_a_point_ignores_the_contact()
        {
            _lonely = new GameObject("lonely");
            _lonely.AddComponent<BoxCollider>();
            var contact = _lonely.AddComponent<SnapOnTriggerEnter>();
            var touching = _snappableObject.AddComponent<BoxCollider>();
            yield return null;

            Assert.DoesNotThrow(() => contact.SendMessage("OnTriggerEnter", touching));
        }

        [UnityTest]
        public IEnumerator A_physical_collision_snaps_the_object_that_hit_the_point()
        {
            SetPrivate(_point, "_snapDurationSeconds", 0f);
            _pointObject.transform.position = Vector3.zero;
            _pointObject.AddComponent<BoxCollider>();
            _pointObject.AddComponent<SnapOnCollisionEnter>();
            _snappableObject.transform.position = new Vector3(0f, 3f, 0f);
            _snappableObject.AddComponent<BoxCollider>();
            var body = _snappableObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            yield return null;

            body.AddForce(Vector3.down * 6f, ForceMode.VelocityChange);
            var deadline = Time.realtimeSinceStartup + 3f;
            while (!_point.IsOccupied && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(_point.Occupant, Is.SameAs(_snappable));
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }
    }
}
