using UnityEngine;

namespace Framework
{
    public sealed class LookDwell
    {
        private readonly float _requiredSeconds;
        private float _dwellSeconds;

        public LookDwell(float requiredSeconds)
        {
            _requiredSeconds = Mathf.Max(0f, requiredSeconds);
        }

        public bool IsLooking { get; private set; }
        public bool HasBeenSeen { get; private set; }

        public float Progress
        {
            get
            {
                if (HasBeenSeen)
                {
                    return 1f;
                }
                return _requiredSeconds <= 0f ? 0f : Mathf.Clamp01(_dwellSeconds / _requiredSeconds);
            }
        }

        public LookChange Update(bool isLookedAt, float deltaSeconds)
        {
            var change = LookChange.None;
            var wasLooking = IsLooking;
            if (isLookedAt)
            {
                if (!wasLooking)
                {
                    IsLooking = true;
                    _dwellSeconds = 0f;
                    change |= LookChange.Started;
                }
                else
                {
                    _dwellSeconds += Mathf.Max(0f, deltaSeconds);
                }

                if (!HasBeenSeen && _dwellSeconds >= _requiredSeconds)
                {
                    HasBeenSeen = true;
                    change |= LookChange.Seen;
                }
            }
            else if (wasLooking)
            {
                IsLooking = false;
                _dwellSeconds = 0f;
                change |= LookChange.Ended;
            }

            return change;
        }

        public void Reset()
        {
            IsLooking = false;
            HasBeenSeen = false;
            _dwellSeconds = 0f;
        }
    }
}
