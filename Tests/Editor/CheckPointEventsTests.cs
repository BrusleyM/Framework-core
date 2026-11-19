using Framework.CheckPoints;
using NUnit.Framework;
using UnityEngine;

namespace Framework.Tests
{
    public class CheckPointEventsTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void A_check_point_added_from_code_has_a_checked_event_that_can_be_listened_to()
        {
            _root = new GameObject("point");
            var point = _root.AddComponent<CheckPoint>();
            var raised = false;

            point.OnChecked.AddListener(() => raised = true);
            point.MarkAsChecked();

            Assert.That(raised, Is.True);
        }

        [Test]
        public void A_check_points_parent_created_from_code_has_an_all_checked_event_that_can_be_listened_to()
        {
            var parent = new CheckPointsParent();

            Assert.That(parent.OnAllChecked, Is.Not.Null);
        }

        [Test]
        public void A_check_points_parent_created_from_code_has_no_points_and_resets_without_failing()
        {
            var parent = new CheckPointsParent();

            Assert.That(parent.CheckPoints, Is.Empty);
            Assert.DoesNotThrow(() => parent.ResetAllPoints());
        }
    }
}
