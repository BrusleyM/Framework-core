using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class LookTarget : MonoBehaviour
    {
        private const int MaxEventRounds = 8;

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
        private bool _resetDuringReport;
        private readonly Queue<LookChange> _pendingChanges = new Queue<LookChange>();

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
            var change = Dwell.Update(isLookedAt, deltaSeconds);
            if (_isReporting)
            {
                if (change != LookChange.None)
                {
                    _pendingChanges.Enqueue(change);
                }
                return change;
            }

            _isReporting = true;
            _resetDuringReport = false;
            try
            {
                RaiseInOrder(change);
            }
            finally
            {
                _isReporting = false;
                _resetDuringReport = false;
                _pendingChanges.Clear();
            }
            return change;
        }

        private void RaiseInOrder(LookChange first)
        {
            var next = first;
            var rounds = 0;
            while (next != LookChange.None)
            {
                if (++rounds > MaxEventRounds)
                {
                    Debug.LogError($"{name}: look events kept retriggering themselves, so they were stopped after {MaxEventRounds} rounds.", this);
                    return;
                }

                RaiseEvents(next);
                next = _pendingChanges.Count > 0 ? _pendingChanges.Dequeue() : LookChange.None;
            }
        }

        private void RaiseEvents(LookChange change)
        {
            _resetDuringReport = false;
            if ((change & LookChange.Started) != 0)
            {
                SafeEvents.Invoke(_onLookStarted, this);
                SafeEvents.Invoke(LookStarted, this);
            }
            if ((change & LookChange.Seen) != 0 && !_resetDuringReport)
            {
                SafeEvents.Invoke(_onSeen, this);
                SafeEvents.Invoke(Seen, this);
            }
            if ((change & LookChange.Ended) != 0 && !_resetDuringReport)
            {
                SafeEvents.Invoke(_onLookEnded, this);
                SafeEvents.Invoke(LookEnded, this);
            }
        }

        public void ResetLook()
        {
            Dwell.Reset();
            _pendingChanges.Clear();
            _resetDuringReport = _isReporting;
        }
    }
}
