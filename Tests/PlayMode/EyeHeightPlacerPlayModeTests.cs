using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Framework.Tests
{
    public class EyeHeightPlacerPlayModeTests
    {
        private const float DelaySeconds = 0.4f;

        private GameObject _eyesObject;
        private GameObject _targetObject;
        private EyeHeightPlacer _placer;

        [SetUp]
        public void SetUp()
        {
            _eyesObject = new GameObject("eyes");
            _eyesObject.transform.position = new Vector3(0f, 1.6f, 0f);
            _targetObject = new GameObject("target");
            _targetObject.SetActive(false);
            _targetObject.transform.position = new Vector3(5f, 0f, 5f);
            _placer = _targetObject.AddComponent<EyeHeightPlacer>();
            SetPrivate("_eyes", _eyesObject.transform);
            SetPrivate("_placeOnEnable", true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_eyesObject);
            Object.Destroy(_targetObject);
        }

        [UnityTest]
        public IEnumerator Enabling_with_place_on_enable_and_no_delay_places_the_target_immediately()
        {
            _targetObject.SetActive(true);
            yield return null;

            Assert.That(_targetObject.transform.position.y, Is.EqualTo(1.6f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator Enabling_with_a_delay_places_the_target_only_after_the_delay()
        {
            SetPrivate("_placeOnEnableDelaySeconds", DelaySeconds);
            _targetObject.SetActive(true);
            yield return null;
            var yBeforeDelay = _targetObject.transform.position.y;

            yield return new WaitForSecondsRealtime(DelaySeconds + 0.3f);

            Assert.That(yBeforeDelay, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(_targetObject.transform.position.y, Is.EqualTo(1.6f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator Disabling_before_the_delay_ends_cancels_the_placement()
        {
            SetPrivate("_placeOnEnableDelaySeconds", DelaySeconds);
            _targetObject.SetActive(true);
            yield return null;

            _targetObject.SetActive(false);
            yield return new WaitForSecondsRealtime(DelaySeconds + 0.3f);

            Assert.That(_targetObject.transform.position.y, Is.EqualTo(0f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator Eyes_that_move_during_the_delay_are_read_when_the_delay_ends()
        {
            SetPrivate("_placeOnEnableDelaySeconds", DelaySeconds);
            _targetObject.SetActive(true);
            yield return null;

            _eyesObject.transform.position = new Vector3(0f, 1.2f, 0f);
            yield return new WaitForSecondsRealtime(DelaySeconds + 0.3f);

            Assert.That(_targetObject.transform.position.y, Is.EqualTo(1.2f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator A_placed_listener_that_places_again_does_not_recurse()
        {
            SetPrivate("_placeOnEnable", false);
            _targetObject.SetActive(true);
            yield return null;
            var placedCount = 0;
            _placer.OnPlaced.AddListener(() =>
            {
                placedCount++;
                _placer.Place();
            });
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("already running"));

            _placer.Place();

            Assert.That(placedCount, Is.EqualTo(1));
        }

        private void SetPrivate(string fieldName, object value)
        {
            var field = typeof(EyeHeightPlacer).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(_placer, value);
        }
    }
}
