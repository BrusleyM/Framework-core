using UnityEngine;

namespace Framework
{
    public readonly struct SnapBlend
    {
        private readonly Pose _from;
        private readonly Pose _to;
        private readonly float _durationSeconds;

        public SnapBlend(Pose from, Pose to, float durationSeconds)
        {
            _from = from;
            _to = to;
            _durationSeconds = Mathf.Max(0f, durationSeconds);
        }

        public float ProgressAt(float elapsedSeconds)
        {
            if (_durationSeconds <= 0f)
            {
                return 1f;
            }
            return Mathf.Clamp01(elapsedSeconds / _durationSeconds);
        }

        public bool IsCompleteAt(float elapsedSeconds)
        {
            return ProgressAt(elapsedSeconds) >= 1f;
        }

        public Pose PoseAtEasedProgress(float easedProgress)
        {
            return new Pose(
                Vector3.LerpUnclamped(_from.position, _to.position, easedProgress),
                Quaternion.SlerpUnclamped(_from.rotation, _to.rotation, easedProgress));
        }
    }
}
