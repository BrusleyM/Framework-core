using Framework.CheckPoints;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Samples
{
    public class CheckPointsShowcase : MonoBehaviour
    {
        [SerializeField]
        private CheckPointsParent _points = new CheckPointsParent();
        [SerializeField]
        private UnityEvent _onReset = new UnityEvent();

        public CheckPointsParent Points => _points;
        public UnityEvent ResetEvent => _onReset;

        private void Awake()
        {
            foreach (var point in _points.CheckPoints)
            {
                if (point != null)
                {
                    point.Parent = _points;
                }
            }
        }

        public void ResetAll()
        {
            _points.ResetAllPoints();
            SafeEvents.Invoke(_onReset, this);
        }
    }
}
