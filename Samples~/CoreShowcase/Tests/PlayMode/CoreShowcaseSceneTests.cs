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
        private const string SceneGuid = "cec83fac316344c098cbc919581f66c8";
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
            var scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(SceneGuid);
            Assert.That(scenePath, Is.Not.Empty, "The showcase scene was not found by its GUID.");
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
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
            _keys = Object.FindFirstObjectByType<ShowcaseKeys>();
        }

        private static T Named<T>(string name) where T : Component
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(candidate => candidate.name == name);
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
            var showcase = Object.FindFirstObjectByType<CheckPointsShowcase>();
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
        public IEnumerator Drawing_a_colour_snaps_every_ball_of_that_colour_into_its_tray_and_refuses_the_rest()
        {
            yield return SetUpTheShowcase();
            var machine = Object.FindFirstObjectByType<LottoMachine>();

            _keys.Press(KeyCode.Alpha2);
            yield return Until(() => !machine.IsDrawing);

            var occupied = machine.Trays.Select(tray => tray.OccupiedCount).ToArray();
            Assert.That(machine.Trays[1].ColourName, Is.EqualTo("CYAN"));
            Assert.That(occupied, Is.EqualTo(new[] { 0, 5, 0, 0 }));
            Assert.That(machine.Balls.Count(ball => ball.IsSnapped), Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator Drawing_two_colours_fills_both_trays_without_mixing_colours()
        {
            yield return SetUpTheShowcase();
            var machine = Object.FindFirstObjectByType<LottoMachine>();

            _keys.Press(KeyCode.Alpha3);
            yield return Until(() => !machine.IsDrawing);
            _keys.Press(KeyCode.Alpha4);
            yield return Until(() => !machine.IsDrawing);

            var occupied = machine.Trays.Select(tray => tray.OccupiedCount).ToArray();
            Assert.That(occupied, Is.EqualTo(new[] { 0, 0, 5, 5 }));
        }

        [UnityTest]
        public IEnumerator Pressing_space_empties_the_trays_so_a_colour_can_be_drawn_again()
        {
            yield return SetUpTheShowcase();
            var machine = Object.FindFirstObjectByType<LottoMachine>();
            _keys.Press(KeyCode.Alpha1);
            yield return Until(() => !machine.IsDrawing);

            _keys.Press(KeyCode.Space);
            var snappedRightAfterShuffling = machine.Balls.Count(ball => ball.IsSnapped);
            _keys.Press(KeyCode.Alpha1);
            yield return Until(() => !machine.IsDrawing);

            Assert.That(snappedRightAfterShuffling, Is.EqualTo(0));
            Assert.That(machine.Trays[0].OccupiedCount, Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator The_balls_keep_tumbling_inside_the_drum_until_they_are_drawn()
        {
            yield return SetUpTheShowcase();
            var machine = Object.FindFirstObjectByType<LottoMachine>();
            var before = machine.Balls.Select(ball => ball.transform.position).ToArray();

            yield return new WaitForSecondsRealtime(1.5f);

            var moved = machine.Balls.Where((ball, index) => Vector3.Distance(ball.transform.position, before[index]) > 0.05f).Count();
            var centre = machine.transform.position;
            var escaped = machine.Balls.Count(ball => Vector3.Distance(new Vector3(ball.transform.position.x, 0f, ball.transform.position.z), new Vector3(centre.x, 0f, centre.z)) > 0.6f);
            Assert.That(moved, Is.GreaterThan(10));
            Assert.That(escaped, Is.EqualTo(0));
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
