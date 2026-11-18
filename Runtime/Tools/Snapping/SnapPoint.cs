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
        private AnimationCurve _easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        private UnityEvent<Snappable> _onSnapped = new UnityEvent<Snappable>();
        [SerializeField]
        private UnityEvent<Snappable> _onReleased = new UnityEvent<Snappable>();

        private Coroutine _snapRoutine;

        public UnityEvent<Snappable> OnSnapped => _onSnapped;
        public UnityEvent<Snappable> OnReleased => _onReleased;
        public Snappable Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;

        public bool Accepts(Snappable candidate)
        {
            return !IsOccupied
                && candidate != null
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
            var blend = new SnapBlend(
                new Pose(candidate.transform.position, candidate.transform.rotation),
                new Pose(transform.position, transform.rotation),
                _snapDurationSeconds);
            if (blend.IsCompleteAt(0f) || !isActiveAndEnabled)
            {
                CompleteSnap(candidate, blend);
            }
            else
            {
                _snapRoutine = StartCoroutine(AnimateSnap(candidate, blend));
            }
            return true;
        }

        public bool Release()
        {
            if (!IsOccupied)
            {
                return false;
            }
            var released = Occupant;
            if (_snapRoutine != null)
            {
                StopCoroutine(_snapRoutine);
                _snapRoutine = null;
            }
            Occupant = null;
            released.EndSnap();
            _onReleased?.Invoke(released);
            return true;
        }

        private IEnumerator AnimateSnap(Snappable snapped, SnapBlend blend)
        {
            var elapsed = 0f;
            while (!blend.IsCompleteAt(elapsed))
            {
                ApplyPose(snapped, blend, _easing.Evaluate(blend.ProgressAt(elapsed)));
                yield return null;
                elapsed += Time.deltaTime;
            }
            _snapRoutine = null;
            CompleteSnap(snapped, blend);
        }

        private void CompleteSnap(Snappable snapped, SnapBlend blend)
        {
            ApplyPose(snapped, blend, 1f);
            _onSnapped?.Invoke(snapped);
        }

        private static void ApplyPose(Snappable snapped, SnapBlend blend, float easedProgress)
        {
            var pose = blend.PoseAtEasedProgress(easedProgress);
            snapped.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
}
