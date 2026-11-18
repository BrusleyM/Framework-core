using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Framework.Tests
{
    public class EyeHeightPlacerTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<GameObject> _hiddenMainCameras = new List<GameObject>();
        private GameObject _eyesObject;
        private GameObject _targetObject;
        private EyeHeightPlacer _placer;

        [SetUp]
        public void SetUp()
        {
            _eyesObject = Create("eyes");
            _eyesObject.transform.position = new Vector3(0f, 1.7f, 0f);
            _targetObject = Create("target");
            _targetObject.transform.position = new Vector3(5f, 0f, 5f);
            _placer = _targetObject.AddComponent<EyeHeightPlacer>();
            AssignObject("_eyes", _eyesObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var hidden in _hiddenMainCameras)
            {
                hidden.SetActive(true);
            }
            _hiddenMainCameras.Clear();
            foreach (var created in _created)
            {
                Object.DestroyImmediate(created);
            }
            _created.Clear();
        }

        [Test]
        public void Placing_moves_the_placer_object_to_eye_height_in_front_of_the_eyes()
        {
            _placer.Place();

            Assert.That(_targetObject.transform.position, Is.EqualTo(new Vector3(0f, 1.7f, 1f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Placing_moves_the_assigned_target_instead_of_the_placer_object()
        {
            var panel = Create("panel");
            AssignObject("_target", panel.transform);

            _placer.Place();

            Assert.That(panel.transform.position.y, Is.EqualTo(1.7f).Within(0.0001f));
            Assert.That(_targetObject.transform.position, Is.EqualTo(new Vector3(5f, 0f, 5f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Placing_without_any_eyes_reports_failure_and_leaves_the_target_alone()
        {
            AssignObject("_eyes", null);
            HideEveryMainCamera();

            var placed = _placer.TryPlace();

            Assert.That(placed, Is.False);
            Assert.That(_targetObject.transform.position, Is.EqualTo(new Vector3(5f, 0f, 5f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Placing_raises_the_placed_event_once()
        {
            var raised = 0;
            _placer.OnPlaced.AddListener(() => raised++);

            _placer.Place();

            Assert.That(raised, Is.EqualTo(1));
        }

        [Test]
        public void A_failed_placement_does_not_raise_the_placed_event()
        {
            AssignObject("_eyes", null);
            HideEveryMainCamera();
            var raised = 0;
            _placer.OnPlaced.AddListener(() => raised++);

            _placer.TryPlace();

            Assert.That(raised, Is.EqualTo(0));
        }

        [Test]
        public void Placing_again_after_the_eyes_move_follows_the_new_eye_height()
        {
            _placer.Place();
            _eyesObject.transform.position = new Vector3(0f, 1.1f, 0f);

            _placer.Place();

            Assert.That(_targetObject.transform.position.y, Is.EqualTo(1.1f).Within(0.0001f));
        }

        [Test]
        public void A_target_that_is_a_child_of_the_eyes_is_refused_and_left_alone()
        {
            var panel = Create("panel");
            panel.transform.SetParent(_eyesObject.transform, false);
            panel.transform.localPosition = new Vector3(0f, 0f, 3f);
            AssignObject("_target", panel.transform);

            var placed = _placer.TryPlace();

            Assert.That(placed, Is.False);
            Assert.That(panel.transform.localPosition, Is.EqualTo(new Vector3(0f, 0f, 3f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void A_target_that_is_a_parent_of_the_eyes_is_refused_and_left_alone()
        {
            var rig = Create("rig");
            rig.transform.position = new Vector3(2f, 0f, 2f);
            _eyesObject.transform.SetParent(rig.transform, true);
            AssignObject("_target", rig.transform);

            var placed = _placer.TryPlace();

            Assert.That(placed, Is.False);
            Assert.That(rig.transform.position, Is.EqualTo(new Vector3(2f, 0f, 2f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void The_warning_for_missing_eyes_names_the_missing_eyes()
        {
            AssignObject("_eyes", null);
            HideEveryMainCamera();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("no eyes are assigned"));

            _placer.Place();
        }

        [Test]
        public void The_warning_for_a_target_related_to_the_eyes_names_that_cause()
        {
            AssignObject("_target", _eyesObject.transform);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("one of their ancestors"));

            _placer.Place();
        }

        [Test]
        public void The_eyes_themselves_are_refused_as_the_target()
        {
            AssignObject("_target", _eyesObject.transform);

            Assert.That(_placer.TryPlace(), Is.False);
        }

        [Test]
        public void A_target_parented_under_a_moved_rig_still_lands_in_front_of_the_eyes_in_world_space()
        {
            var rig = Create("rig");
            rig.transform.position = new Vector3(10f, 0f, 10f);
            rig.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            var panel = Create("panel");
            panel.transform.SetParent(rig.transform, false);
            AssignObject("_target", panel.transform);

            _placer.Place();

            Assert.That(panel.transform.position, Is.EqualTo(new Vector3(0f, 1.7f, 1f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Placing_with_nothing_to_place_against_logs_a_warning()
        {
            AssignObject("_eyes", null);
            HideEveryMainCamera();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("could not place"));

            _placer.Place();
        }

        [Test]
        public void Without_assigned_eyes_the_main_camera_is_used()
        {
            AssignObject("_eyes", null);
            HideEveryMainCamera();
            var cameraObject = Create("main camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.transform.position = new Vector3(0f, 1.3f, 0f);

            _placer.Place();

            Assert.That(_targetObject.transform.position.y, Is.EqualTo(1.3f).Within(0.0001f));
        }

        private void HideEveryMainCamera()
        {
            foreach (var camera in Camera.allCameras)
            {
                if (camera.CompareTag("MainCamera") && camera.gameObject.activeSelf)
                {
                    camera.gameObject.SetActive(false);
                    _hiddenMainCameras.Add(camera.gameObject);
                }
            }
        }

        private GameObject Create(string name)
        {
            var created = new GameObject(name);
            _created.Add(created);
            return created;
        }

        private void AssignObject(string field, Object value)
        {
            var serialized = new SerializedObject(_placer);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
