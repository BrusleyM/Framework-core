using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class LookTarget : MonoBehaviour
    {
        [SerializeField]
        private Transform _aimPoint;
        [SerializeField, Min(0f)]
        private float _dwellSeconds;
        [SerializeField]
        private UnityEvent _onLookStarted = new UnityEvent();
        [SerializeField]
        private UnityEvent _onLookEnded = new UnityEvent();
        [SerializeField]
        private UnityEvent _onSeen = new UnityEvent();

        private LookDwell _dwell;

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

        public LookChange ReportGaze(bool isLookedAt, float deltaSeconds)
        {
            var change = Dwell.Update(isLookedAt, deltaSeconds);
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
            return change;
        }

        public void ResetLook()
        {
            Dwell.Reset();
        }
    }
}
