namespace Framework.Timing
{
    public sealed class ElapsedTimer
    {
        private readonly ITimeSource _timeSource;
        private double _accumulatedSeconds;
        private double _runningSince;

        public ElapsedTimer() : this(UnityTimeSource.Instance)
        {
        }

        public ElapsedTimer(ITimeSource timeSource)
        {
            _timeSource = timeSource;
        }

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        public double ElapsedSeconds => IsRunning ? _accumulatedSeconds + _timeSource.Seconds - _runningSince : _accumulatedSeconds;

        public void Start()
        {
            _accumulatedSeconds = 0d;
            _runningSince = _timeSource.Seconds;
            IsRunning = true;
            IsPaused = false;
        }

        public void Pause()
        {
            if (!IsRunning)
            {
                return;
            }

            _accumulatedSeconds = ElapsedSeconds;
            IsRunning = false;
            IsPaused = true;
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            _runningSince = _timeSource.Seconds;
            IsRunning = true;
            IsPaused = false;
        }

        public double Stop()
        {
            _accumulatedSeconds = ElapsedSeconds;
            IsRunning = false;
            IsPaused = false;
            return _accumulatedSeconds;
        }

        public void Reset()
        {
            _accumulatedSeconds = 0d;
            IsRunning = false;
            IsPaused = false;
        }
    }
}
