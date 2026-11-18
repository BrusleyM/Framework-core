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

        private GameObject _pointObject;
        private GameObject _snappableObject;
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
            Object.Destroy(_pointObject);
            Object.Destroy(_snappableObject);
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
            _point.TryOffer(_snappable);
            yield return null;
            yield return null;

            _point.Release();
            var positionWhenReleased = _snappableObject.transform.position;
            yield return new WaitForSecondsRealtime(SnapSeconds + 0.3f);

            Assert.That(_snappableObject.transform.position, Is.EqualTo(positionWhenReleased));
            Assert.That(_snappedCount, Is.EqualTo(0));
            Assert.That(_releasedCount, Is.EqualTo(1));
            Assert.That(_point.IsOccupied, Is.False);
        }

        [UnityTest]
        public IEnumerator Destroying_the_snappable_during_the_animation_frees_the_point_without_errors()
        {
            _point.TryOffer(_snappable);
            yield return null;

            Object.Destroy(_snappableObject);
            yield return new WaitForSecondsRealtime(SnapSeconds + 0.3f);

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
            _point.TryOffer(_snappable);
            yield return null;

            _pointObject.transform.position = new Vector3(0f, 5f, 0f);
            yield return new WaitForSecondsRealtime(SnapSeconds + 0.3f);

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

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }
    }
}
