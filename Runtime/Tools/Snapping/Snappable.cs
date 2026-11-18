using UnityEngine;

namespace Framework
{
    public class Snappable : MonoBehaviour
    {
        [SerializeField]
        private SnapKey _key;

        private Rigidbody _body;
        private bool _bodyWasKinematic;
        private RigidbodyInterpolation _bodyInterpolation;
        private bool _bodyIsHeld;

        public SnapKey Key => _key;
        public SnapPoint CurrentPoint { get; private set; }
        public bool IsSnapped => CurrentPoint != null;

        private void OnDestroy()
        {
            if (CurrentPoint != null)
            {
                CurrentPoint.Release();
            }
        }

        internal void BeginSnapTo(SnapPoint point)
        {
            CurrentPoint = point;
            HoldBody();
        }

        internal void EndSnap()
        {
            CurrentPoint = null;
            ReleaseBody();
        }

        private void HoldBody()
        {
            _body = GetComponent<Rigidbody>();
            if (_body == null || _bodyIsHeld)
            {
                return;
            }
            _bodyWasKinematic = _body.isKinematic;
            _bodyInterpolation = _body.interpolation;
            _body.interpolation = RigidbodyInterpolation.None;
            _body.isKinematic = true;
            _bodyIsHeld = true;
        }

        private void ReleaseBody()
        {
            var bodyToRestore = _body;
            var wasHeld = _bodyIsHeld;
            _body = null;
            _bodyIsHeld = false;
            if (!wasHeld || bodyToRestore == null)
            {
                return;
            }
            bodyToRestore.isKinematic = _bodyWasKinematic;
            bodyToRestore.interpolation = _bodyInterpolation;
        }
    }
}
