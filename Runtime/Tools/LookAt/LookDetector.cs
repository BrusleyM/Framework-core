using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    [Serializable]
    public class LookStoppedEvent : UnityEvent<LookStopReason>
    {
    }

    [Serializable]
    public class LookTargetEvent : UnityEvent<LookTarget>
    {
    }

    public class LookDetector : MonoBehaviour
    {
        private const float SamePositionDistance = 0.001f;

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
        private LayerMask _blockingLayers = ~0;
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

        private Coroutine _sampling;
        private int _seenCount;
        private bool _allTargetsFoundRaised;

        public UnityEvent OnLookingStarted => _onLookingStarted;
        public LookStoppedEvent OnLookingStopped => _onLookingStopped;
        public LookTargetEvent OnTargetFound => _onTargetFound;
        public UnityEvent OnAllTargetsFound => _onAllTargetsFound;
        public IReadOnlyList<LookTarget> Targets => _targets;
        public bool IsLooking { get; private set; }

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
            var index = _targets.IndexOf(target);
            if (index < 0)
            {
                return false;
            }

            if (target != null)
            {
                if (target.IsSeen)
                {
                    _seenCount--;
                }
                target.ReportGaze(false, 0f);
            }
            _targets.RemoveAt(index);
            return true;
        }

        public void StartLooking()
        {
            if (IsLooking)
            {
                return;
            }

            IsLooking = true;
            _seenCount = 0;
            _allTargetsFoundRaised = false;
            foreach (var target in _targets)
            {
                if (target != null)
                {
                    target.ResetLook();
                }
            }

            _onLookingStarted.Invoke();
            LookingStarted?.Invoke();
            _sampling = StartCoroutine(SampleRepeatedly());
        }

        public void StopLooking()
        {
            Stop(LookStopReason.Requested);
        }

        public void SampleOnce(float deltaSeconds)
        {
            if (!IsLooking)
            {
                return;
            }

            var eyes = _eyes != null ? _eyes : transform;
            for (var index = 0; index < _targets.Count; index++)
            {
                var target = _targets[index];
                if (target == null)
                {
                    continue;
                }

                var change = target.ReportGaze(IsLookedAt(eyes, target), deltaSeconds);
                if ((change & LookChange.Seen) != 0)
                {
                    _seenCount++;
                    _onTargetFound.Invoke(target);
                    TargetFound?.Invoke(target);
                }
            }

            RaiseAllTargetsFoundOnce();
            if (IsLooking && LookStopRules.ShouldStop(_stopRule, _seenCount, _targets.Count))
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
                var elapsed = now - lastSampleTime;
                lastSampleTime = now;
                SampleOnce(elapsed);
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

            foreach (var target in _targets)
            {
                if (target != null)
                {
                    target.ReportGaze(false, 0f);
                }
            }

            _onLookingStopped.Invoke(reason);
            LookingStopped?.Invoke(reason);
        }

        private void RaiseAllTargetsFoundOnce()
        {
            if (_allTargetsFoundRaised || _targets.Count == 0 || _seenCount < _targets.Count)
            {
                return;
            }

            _allTargetsFoundRaised = true;
            _onAllTargetsFound.Invoke();
            AllTargetsFound?.Invoke();
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

            if (distance < SamePositionDistance)
            {
                return true;
            }

            var blocked = Physics.Raycast(eyePosition, toAim / distance, out var hit, distance, _blockingLayers, _triggerInteraction);
            return !blocked || hit.collider.transform.IsChildOf(target.transform);
        }
    }
}
