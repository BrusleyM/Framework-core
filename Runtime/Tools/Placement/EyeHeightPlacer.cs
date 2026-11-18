using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class EyeHeightPlacer : MonoBehaviour
    {
        [SerializeField]
        private Transform _eyes;
        [SerializeField]
        private Transform _target;
        [SerializeField, Min(0f)]
        private float _distanceInFront = 1f;
        [SerializeField]
        private float _heightOffset;
        [SerializeField]
        private PlacementFacing _facing = PlacementFacing.SameAsViewDirection;
        [SerializeField]
        private bool _placeOnEnable;
        [SerializeField, Min(0f)]
        private float _placeOnEnableDelaySeconds;
        [SerializeField]
        private UnityEvent _onPlaced = new UnityEvent();

        private Coroutine _delayedPlacement;

        public UnityEvent OnPlaced => _onPlaced;

        public void Place()
        {
            if (!TryPlace())
            {
                Debug.LogWarning("EyeHeightPlacer could not place the target: assign the eyes or have an active main camera, and do not use the eyes or one of their children as the target.", this);
            }
        }

        public bool TryPlace()
        {
            var eyes = ResolveEyes();
            var target = _target != null ? _target : transform;
            if (eyes == null || target.IsChildOf(eyes))
            {
                return false;
            }

            var pose = EyeHeightPlacement.InFrontOfEyes(
                eyes.position,
                eyes.forward,
                _distanceInFront,
                _heightOffset,
                _facing,
                target.rotation);
            target.SetPositionAndRotation(pose.position, pose.rotation);
            _onPlaced.Invoke();
            return true;
        }

        private void OnEnable()
        {
            if (!_placeOnEnable)
            {
                return;
            }

            if (_placeOnEnableDelaySeconds <= 0f)
            {
                Place();
            }
            else
            {
                _delayedPlacement = StartCoroutine(PlaceAfterDelay());
            }
        }

        private void OnDisable()
        {
            if (_delayedPlacement != null)
            {
                StopCoroutine(_delayedPlacement);
                _delayedPlacement = null;
            }
        }

        private IEnumerator PlaceAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_placeOnEnableDelaySeconds);
            _delayedPlacement = null;
            Place();
        }

        private Transform ResolveEyes()
        {
            if (_eyes != null)
            {
                return _eyes;
            }

            var mainCamera = Camera.main;
            return mainCamera != null ? mainCamera.transform : null;
        }
    }
}
