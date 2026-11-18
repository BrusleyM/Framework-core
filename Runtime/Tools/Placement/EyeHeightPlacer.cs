using System;
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
        private bool _isPlacing;

        public UnityEvent OnPlaced => _onPlaced;

        public void Place()
        {
            if (!TryPlace(out var failureReason))
            {
                Debug.LogWarning($"EyeHeightPlacer could not place the target: {failureReason}.", this);
            }
        }

        public bool TryPlace()
        {
            return TryPlace(out _);
        }

        private bool TryPlace(out string failureReason)
        {
            if (_isPlacing)
            {
                failureReason = "a placement is already running, so a placed listener cannot place again";
                return false;
            }

            var eyes = ResolveEyes();
            var target = _target != null ? _target : transform;
            failureReason = ReasonItCannotPlace(eyes, target);
            if (failureReason != null)
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
            _isPlacing = true;
            try
            {
                SafeEvents.Invoke(_onPlaced, this);
            }
            finally
            {
                _isPlacing = false;
            }
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
                _delayedPlacement = StartCoroutine(GuardedCoroutine.Run(PlaceAfterDelay(), ReportDelayedPlacementFailure, this));
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

        private void ReportDelayedPlacementFailure(Exception failure)
        {
            Debug.LogException(failure, this);
            _delayedPlacement = null;
        }

        private IEnumerator PlaceAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_placeOnEnableDelaySeconds);
            _delayedPlacement = null;
            Place();
        }

        private static string ReasonItCannotPlace(Transform eyes, Transform target)
        {
            if (eyes == null)
            {
                return "no eyes are assigned and there is no active main camera";
            }

            if (target.IsChildOf(eyes) || eyes.IsChildOf(target))
            {
                return "the target is the eyes, one of their children or one of their ancestors";
            }

            return null;
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
