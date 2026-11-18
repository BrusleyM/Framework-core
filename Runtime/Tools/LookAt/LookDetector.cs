using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class LookDetector : MonoBehaviour
    {
        private const float RayEndMargin = 0.05f;
        private const int IgnoreRaycastLayer = 2;

        [SerializeField]
        private Transform _eyes;
        [SerializeField]
        private List<LookTarget> _targets = new List<LookTarget>();
        [SerializeField, Range(1f, 90f)]
        private float _maxAngleDegrees = 10f;
        [SerializeField, Min(0.1f)]
        private float _maxDistance = 20f;
        [SerializeField, Min(0.02f)]
        private float _sampleIntervalSeconds = 0.1f;
        [SerializeField]
        private LayerMask _blockingLayers = ~(1 << IgnoreRaycastLayer);
        [SerializeField]
        private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField]
        private LookStopRule _stopRule = LookStopRule.WhenAnyTargetIsSeen;
        [SerializeField]
        private bool _startOnEnable;
        [SerializeField]
        private UnityEvent _onLookingStarted = new UnityEvent();
        [SerializeField]
        private LookStoppedEvent _onLookingStopped = new LookStoppedEvent();
        [SerializeField]
        private LookTargetEvent _onTargetFound = new LookTargetEvent();
        [SerializeField]
        private UnityEvent _onAllTargetsFound = new UnityEvent();

        private readonly List<LookTarget> _sampleBuffer = new List<LookTarget>();
        private readonly List<LookTarget> _stopBuffer = new List<LookTarget>();
        private Coroutine _sampling;
        private bool _allTargetsFoundRaised;
        private bool _anyTargetSeenInRun;
        private bool _isSampling;
        private ReadOnlyCollection<LookTarget> _readOnlyTargets;
        private List<LookTarget> _readOnlyTargetsSource;

        public UnityEvent OnLookingStarted => _onLookingStarted;
        public LookStoppedEvent OnLookingStopped => _onLookingStopped;
        public LookTargetEvent OnTargetFound => _onTargetFound;
        public UnityEvent OnAllTargetsFound => _onAllTargetsFound;
        public bool IsLooking { get; private set; }

        public IReadOnlyList<LookTarget> Targets
        {
            get
            {
                if (_readOnlyTargets == null || !ReferenceEquals(_readOnlyTargetsSource, _targets))
                {
                    _readOnlyTargetsSource = _targets;
                    _readOnlyTargets = new ReadOnlyCollection<LookTarget>(_targets);
                }
                return _readOnlyTargets;
            }
        }

        public event Action LookingStarted;
        public event Action<LookStopReason> LookingStopped;
        public event Action<LookTarget> TargetFound;
        public event Action AllTargetsFound;

        public bool AddTarget(LookTarget target)
        {
            if (target == null || _targets.Contains(target))
            {
                return false;
            }

            if (IsLooking)
            {
                target.ResetLook();
            }

            _targets.Add(target);
            return true;
        }

        public bool RemoveTarget(LookTarget target)
        {
            if (!_targets.Remove(target))
            {
                return false;
            }

            if (target != null)
            {
                target.ReportGaze(false, 0f);
            }
            return true;
        }

        public void StartLooking()
        {
            if (IsLooking)
            {
                return;
            }

            if (!isActiveAndEnabled)
            {
                Debug.LogWarning("LookDetector cannot start looking while it is inactive or disabled.", this);
                return;
            }

            IsLooking = true;
            _allTargetsFoundRaised = false;
            _anyTargetSeenInRun = false;
            foreach (var target in _targets)
            {
                if (target != null)
                {
                    target.ResetLook();
                }
            }

            _sampling = StartCoroutine(SampleRepeatedly());
            SafeEvents.Invoke(_onLookingStarted, this);
            if (IsLooking)
            {
                SafeEvents.Invoke(LookingStarted, this);
            }
        }

        public void StopLooking()
        {
            Stop(LookStopReason.Requested);
        }

        public void SampleOnce(float deltaSeconds)
        {
            if (!IsLooking || _isSampling)
            {
                return;
            }

            _isSampling = true;
            try
            {
                SampleTargets(deltaSeconds);
            }
            finally
            {
                _isSampling = false;
            }
        }

        private void SampleTargets(float deltaSeconds)
        {
            var eyes = _eyes != null ? _eyes : transform;
            CollectDistinctTargetsInto(_sampleBuffer);
            for (var index = 0; index < _sampleBuffer.Count; index++)
            {
                if (!IsLooking)
                {
                    return;
                }

                var target = _sampleBuffer[index];
                if (target == null || !_targets.Contains(target))
                {
                    continue;
                }

                var change = target.ReportGaze(IsLookedAt(eyes, target), deltaSeconds);
                if ((change & LookChange.Seen) != 0)
                {
                    _anyTargetSeenInRun = true;
                    SafeEvents.Invoke(_onTargetFound, target, this);
                    SafeEvents.Invoke(TargetFound, target, this);
                }
            }

            if (!IsLooking)
            {
                return;
            }

            CollectDistinctTargetsInto(_sampleBuffer);
            var seenCount = CountSeen(_sampleBuffer);
            RaiseAllTargetsFoundOnce(seenCount, _sampleBuffer.Count);
            var seenForStopRule = _stopRule == LookStopRule.WhenAnyTargetIsSeen && _anyTargetSeenInRun ? Math.Max(1, seenCount) : seenCount;
            if (IsLooking && LookStopRules.ShouldStop(_stopRule, seenForStopRule, _sampleBuffer.Count))
            {
                Stop(LookStopRules.ReasonFor(_stopRule));
            }
        }

        private void OnEnable()
        {
            if (_startOnEnable)
            {
                StartLooking();
            }
        }

        private void OnDisable()
        {
            Stop(LookStopReason.Disabled);
        }

        private IEnumerator SampleRepeatedly()
        {
            var wait = new WaitForSecondsRealtime(_sampleIntervalSeconds);
            var lastSampleTime = Time.unscaledTime;
            while (IsLooking)
            {
                yield return wait;
                wait.waitTime = _sampleIntervalSeconds;
                wait.Reset();
                var now = Time.unscaledTime;
                var elapsed = LookSampling.ClampedDelta(now - lastSampleTime, _sampleIntervalSeconds);
                lastSampleTime = now;
                try
                {
                    SampleOnce(elapsed);
                }
                catch (Exception failure)
                {
                    Debug.LogException(failure, this);
                }
            }
        }

        private void Stop(LookStopReason reason)
        {
            if (!IsLooking)
            {
                return;
            }

            IsLooking = false;
            if (_sampling != null)
            {
                StopCoroutine(_sampling);
                _sampling = null;
            }

            CollectDistinctTargetsInto(_stopBuffer);
            for (var index = 0; index < _stopBuffer.Count; index++)
            {
                _stopBuffer[index].ReportGaze(false, 0f);
            }

            SafeEvents.Invoke(_onLookingStopped, reason, this);
            SafeEvents.Invoke(LookingStopped, reason, this);
        }

        private void CollectDistinctTargetsInto(List<LookTarget> destination)
        {
            destination.Clear();
            foreach (var target in _targets)
            {
                if (target != null && !destination.Contains(target))
                {
                    destination.Add(target);
                }
            }
        }

        private static int CountSeen(List<LookTarget> targets)
        {
            var seen = 0;
            foreach (var target in targets)
            {
                if (target != null && target.IsSeen)
                {
                    seen++;
                }
            }
            return seen;
        }

        private void RaiseAllTargetsFoundOnce(int seenCount, int targetCount)
        {
            if (_allTargetsFoundRaised || targetCount == 0 || seenCount < targetCount)
            {
                return;
            }

            _allTargetsFoundRaised = true;
            SafeEvents.Invoke(_onAllTargetsFound, this);
            SafeEvents.Invoke(AllTargetsFound, this);
        }

        private bool IsLookedAt(Transform eyes, LookTarget target)
        {
            var eyePosition = eyes.position;
            var aimPosition = target.AimPoint.position;
            if (!LookCone.IsInside(eyePosition, eyes.forward, aimPosition, _maxAngleDegrees))
            {
                return false;
            }

            var toAim = aimPosition - eyePosition;
            var distance = toAim.magnitude;
            if (distance > _maxDistance)
            {
                return false;
            }

            var rayDistance = distance - RayEndMargin;
            if (rayDistance <= 0f)
            {
                return true;
            }

            var blocked = Physics.Raycast(eyePosition, toAim / distance, out var hit, rayDistance, _blockingLayers, _triggerInteraction);
            return !blocked || target.OwnsCollider(hit.collider);
        }
    }
}
