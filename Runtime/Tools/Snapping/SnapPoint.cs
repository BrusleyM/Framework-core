using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class SnapPoint : MonoBehaviour
    {
        [SerializeField]
        private SnapKey _acceptedKey;
        [SerializeField]
        private float _snapDurationSeconds = 0.25f;
        [SerializeField]
        private bool _ignoreTimeScale;
        [SerializeField]
        private AnimationCurve _easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        private UnityEvent<Snappable> _onSnapped = new UnityEvent<Snappable>();
        [SerializeField]
        private UnityEvent<Snappable> _onReleased = new UnityEvent<Snappable>();

        private Coroutine _snapRoutine;
        private bool _isAnimating;
        private Pose _snapStartPose;

        public UnityEvent<Snappable> OnSnapped => _onSnapped;
        public UnityEvent<Snappable> OnReleased => _onReleased;
        public Snappable Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;

        public bool Accepts(Snappable candidate)
        {
            return !IsOccupied
                && candidate != null
                && !candidate.IsSnapped
                && _acceptedKey != null
                && candidate.Key == _acceptedKey;
        }

        public bool TryOffer(Snappable candidate)
        {
            if (!Accepts(candidate))
            {
                return false;
            }
            Occupant = candidate;
            candidate.BeginSnapTo(this);
            _snapStartPose = new Pose(candidate.transform.position, candidate.transform.rotation);
            if (_snapDurationSeconds <= 0f || !isActiveAndEnabled)
            {
                CompleteSnap(candidate);
            }
            else
            {
                _isAnimating = true;
                var routine = StartCoroutine(GuardedCoroutine.Run(AnimateSnap(candidate), AbortAnimatedSnap, this));
                _snapRoutine = _isAnimating ? routine : null;
                return Occupant == candidate;
            }
            return true;
        }

        public bool TryOfferFrom(Collider other)
        {
            var candidate = other != null ? other.GetComponentInParent<Snappable>() : null;
            return TryOffer(candidate);
        }

        public bool Release()
        {
            if (!IsOccupied)
            {
                return false;
            }
            var released = Occupant;
            StopAnimating();
            Occupant = null;
            released.EndSnap();
            SafeEvents.Invoke(_onReleased, released, this);
            return true;
        }

        private void OnDestroy()
        {
            Release();
        }

        private void OnDisable()
        {
            if (_isAnimating)
            {
                Release();
            }
        }

        private IEnumerator AnimateSnap(Snappable snapped)
        {
            var elapsed = 0f;
            var blend = BlendTowardsThisPoint();
            while (!blend.IsCompleteAt(elapsed))
            {
                ApplyPose(snapped, blend, _easing.Evaluate(blend.ProgressAt(elapsed)));
                yield return null;
                elapsed += _ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                blend = BlendTowardsThisPoint();
            }
            _isAnimating = false;
            _snapRoutine = null;
            CompleteSnap(snapped);
        }

        private void AbortAnimatedSnap(Exception failure)
        {
            Debug.LogException(failure, this);
            if (_isAnimating)
            {
                Release();
            }
        }

        private void StopAnimating()
        {
            _isAnimating = false;
            if (_snapRoutine != null)
            {
                StopCoroutine(_snapRoutine);
                _snapRoutine = null;
            }
        }

        private SnapBlend BlendTowardsThisPoint()
        {
            return new SnapBlend(_snapStartPose, new Pose(transform.position, transform.rotation), _snapDurationSeconds);
        }

        private void CompleteSnap(Snappable snapped)
        {
            ApplyPose(snapped, BlendTowardsThisPoint(), 1f);
            SafeEvents.Invoke(_onSnapped, snapped, this);
        }

        private static void ApplyPose(Snappable snapped, SnapBlend blend, float easedProgress)
        {
            var pose = blend.PoseAtEasedProgress(easedProgress);
            snapped.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
}
