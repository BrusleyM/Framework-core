using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class LookTarget : MonoBehaviour
    {
        [SerializeField]
        private Transform _aimPoint;
        [SerializeField]
        private Collider[] _extraColliders = new Collider[0];
        [SerializeField, Min(0f)]
        private float _dwellSeconds;
        [SerializeField]
        private UnityEvent _onLookStarted = new UnityEvent();
        [SerializeField]
        private UnityEvent _onLookEnded = new UnityEvent();
        [SerializeField]
        private UnityEvent _onSeen = new UnityEvent();

        private LookDwell _dwell;
        private bool _isReporting;

        public UnityEvent OnLookStarted => _onLookStarted;
        public UnityEvent OnLookEnded => _onLookEnded;
        public UnityEvent OnSeen => _onSeen;
        public Transform AimPoint => _aimPoint != null ? _aimPoint : transform;
        public bool IsBeingLookedAt => Dwell.IsLooking;
        public bool IsSeen => Dwell.HasBeenSeen;
        public float LookProgress => Dwell.Progress;

        public event Action LookStarted;
        public event Action LookEnded;
        public event Action Seen;

        private LookDwell Dwell
        {
            get
            {
                if (_dwell == null)
                {
                    _dwell = new LookDwell(_dwellSeconds);
                }
                _dwell.RequiredSeconds = _dwellSeconds;
                return _dwell;
            }
        }

        public bool OwnsCollider(Collider collider)
        {
            return collider != null
                && (collider.transform.IsChildOf(transform) || Array.IndexOf(_extraColliders, collider) >= 0);
        }

        public LookChange ReportGaze(bool isLookedAt, float deltaSeconds)
        {
            if (_isReporting)
            {
                return LookChange.None;
            }

            var change = Dwell.Update(isLookedAt, deltaSeconds);
            _isReporting = true;
            try
            {
                RaiseEvents(change);
            }
            finally
            {
                _isReporting = false;
            }
            return change;
        }

        private void RaiseEvents(LookChange change)
        {
            if ((change & LookChange.Started) != 0)
            {
                _onLookStarted.Invoke();
                LookStarted?.Invoke();
            }
            if ((change & LookChange.Seen) != 0)
            {
                _onSeen.Invoke();
                Seen?.Invoke();
            }
            if ((change & LookChange.Ended) != 0)
            {
                _onLookEnded.Invoke();
                LookEnded?.Invoke();
            }
        }

        public void ResetLook()
        {
            Dwell.Reset();
        }
    }
}
