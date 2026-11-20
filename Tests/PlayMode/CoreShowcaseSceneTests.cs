using System.Collections;
using System.Linq;
using Framework.CheckPoints;
using Framework.Samples;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Framework.Tests
{
    public class CoreShowcaseSceneTests
    {
        private const string ScenePath = "Assets/Framework Core/Samples/CoreShowcase/Scenes/Core Showcase.unity";
        private const float PollSeconds = 8f;

        private Transform _player;
        private Transform _head;
        private LookDetector _detector;
        private ShowcaseKeys _keys;

        [UnityTearDown]
        public IEnumerator RemoveTheShowcase()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Object.Destroy(root);
            }
            yield return null;
        }

        private IEnumerator LoadTheShowcase()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield break;
#endif
        }

        private IEnumerator SetUpTheShowcase()
        {
            yield return LoadTheShowcase();
            yield return null;
            _head = Camera.main.transform;
            _player = _head.parent;
            _player.GetComponent<ShowcaseCameraRig>().enabled = false;
            _detector = _head.GetComponent<LookDetector>();
            _keys = Object.FindObjectOfType<ShowcaseKeys>();
        }

        private static T Named<T>(string name) where T : Component
        {
            return Object.FindObjectsOfType<T>(true).First(candidate => candidate.name == name);
        }

        private void StandNear(Component target)
        {
            var flat = new Vector3(target.transform.position.x, 0f, target.transform.position.z);
            _player.position = flat * 0.45f;
            _head.position = _player.position + Vector3.up * 1.7f;
            _head.rotation = Quaternion.LookRotation(target.transform.position - _head.position);
        }

        private static IEnumerator Until(System.Func<bool> condition)
        {
            var deadline = Time.realtimeSinceStartup + PollSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator The_showcase_loads_with_a_running_look_detector_and_all_five_stands()
        {
            yield return SetUpTheShowcase();

            Assert.That(_detector.IsLooking, Is.True);
            Assert.That(_detector.Targets.Count, Is.EqualTo(7));
            foreach (var stand in new[] { "Outline stand", "Look-at stand", "Check points stand", "Snap stand", "Eye height stand" })
            {
                Assert.That(GameObject.Find(stand), Is.Not.Null, stand);
            }
        }

        [UnityTest]
        public IEnumerator Looking_at_a_shape_switches_its_outline_on_and_looking_away_switches_it_off()
        {
            yield return SetUpTheShowcase();
            var sphere = Named<Outline>("Sphere (outline)");
            Assert.That(sphere.IsOutLined, Is.False);

            StandNear(sphere);
            yield return Until(() => sphere.IsOutLined);
            var outlinedWhileLooking = sphere.IsOutLined;
            _head.rotation = Quaternion.LookRotation(Vector3.up);
            yield return Until(() => !sphere.IsOutLined);

            Assert.That(outlinedWhileLooking, Is.True);
            Assert.That(sphere.IsOutLined, Is.False);
        }

        [UnityTest]
        public IEnumerator Keeping_the_eyes_on_the_orb_marks_it_as_seen()
        {
            yield return SetUpTheShowcase();
            var orb = Named<LookTarget>("Look orb");

            StandNear(orb);
            yield return Until(() => orb.IsSeen);

            Assert.That(orb.IsSeen, Is.True);
        }

        [UnityTest]
        public IEnumerator Looking_at_all_three_crystals_checks_every_point_and_raises_the_all_checked_event()
        {
            yield return SetUpTheShowcase();
            var showcase = Object.FindObjectOfType<CheckPointsShowcase>();
            var allChecked = false;
            showcase.Points.OnAllChecked.AddListener(() => allChecked = true);

            foreach (var crystal in new[] { "Crystal 1", "Crystal 2", "Crystal 3" })
            {
                var target = Named<LookTarget>(crystal);
                StandNear(target);
                yield return Until(() => target.IsSeen);
            }
            yield return Until(() => allChecked);

            Assert.That(allChecked, Is.True);
        }

        [UnityTest]
        public IEnumerator Pressing_r_after_the_orb_was_seen_lets_it_be_seen_again()
        {
            yield return SetUpTheShowcase();
            var orb = Named<LookTarget>("Look orb");
            StandNear(orb);
            yield return Until(() => orb.IsSeen);

            _keys.Press(KeyCode.R);
            var seenRightAfterReset = orb.IsSeen;
            yield return Until(() => orb.IsSeen);

            Assert.That(seenRightAfterReset, Is.False);
            Assert.That(orb.IsSeen, Is.True);
        }

        [UnityTest]
        public IEnumerator The_block_with_the_cyan_key_snaps_to_the_pad_and_the_red_block_is_refused()
        {
            yield return SetUpTheShowcase();
            var point = Named<SnapPoint>("Snap anchor");
            var cyan = Named<Snappable>("Cyan block");
            var red = Named<Snappable>("Red block");

            yield return Until(() => point.IsOccupied);
            yield return new WaitForSecondsRealtime(2.5f);

            Assert.That(point.Occupant, Is.SameAs(cyan));
            Assert.That(red.IsSnapped, Is.False);
        }

        [UnityTest]
        public IEnumerator Pressing_space_drops_the_blocks_again_and_the_cyan_block_snaps_a_second_time()
        {
            yield return SetUpTheShowcase();
            var point = Named<SnapPoint>("Snap anchor");
            yield return Until(() => point.IsOccupied);
            yield return new WaitForSecondsRealtime(2.5f);

            _keys.Press(KeyCode.Space);
            var occupiedRightAfterPressing = point.IsOccupied;
            yield return Until(() => point.IsOccupied);

            Assert.That(occupiedRightAfterPressing, Is.False);
            Assert.That(point.Occupant.name, Is.EqualTo("Cyan block"));
        }

        [UnityTest]
        public IEnumerator Pressing_p_brings_the_panel_to_eye_height_in_front_of_the_camera_and_o_sends_it_home()
        {
            yield return SetUpTheShowcase();
            var panel = Named<EyeHeightPlacer>("Eye height panel").transform;
            var home = panel.position;
            _head.position = new Vector3(-3f, 1.45f, -2f);
            _head.rotation = Quaternion.Euler(20f, 40f, 0f);

            _keys.Press(KeyCode.P);
            var flatDistance = Vector3.Distance(new Vector3(panel.position.x, 0f, panel.position.z), new Vector3(_head.position.x, 0f, _head.position.z));
            var placedHeight = panel.position.y;
            _keys.Press(KeyCode.O);

            Assert.That(placedHeight, Is.EqualTo(1.45f).Within(0.01f));
            Assert.That(flatDistance, Is.EqualTo(1.8f).Within(0.01f));
            Assert.That(Vector3.Distance(panel.position, home), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator A_ring_appears_on_the_orb_while_looking_at_it_fills_up_and_disappears_when_looking_away()
        {
            yield return SetUpTheShowcase();
            var orb = Named<LookTarget>("Look orb");
            var ringName = "Look orb gaze ring";

            StandNear(orb);
            yield return Until(() => GameObject.Find(ringName) != null);
            var ring = GameObject.Find(ringName);
            yield return Until(() => orb.IsSeen);
            yield return null;
            var fillWhenSeen = ring.GetComponent<Renderer>().sharedMaterial.GetFloat("_Fill");
            _head.rotation = Quaternion.LookRotation(Vector3.up);
            yield return Until(() => ring == null || !ring.activeSelf);

            Assert.That(fillWhenSeen, Is.EqualTo(1f));
            Assert.That(ring.activeSelf, Is.False);
        }
    }
}
