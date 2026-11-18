using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class EyeHeightPlacer : MonoBehaviour
    {
        [SerializeField]
        private Transform _eyes;
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private float _distanceInFront = 1f;
        [SerializeField]
        private float _heightOffset;
        [SerializeField]
        private bool _alignWithViewDirection = true;
        [SerializeField]
        private bool _placeOnEnable;
        [SerializeField]
        private UnityEvent _onPlaced = new UnityEvent();

        public UnityEvent OnPlaced => _onPlaced;

        public void Place()
        {
            TryPlace();
        }

        public bool TryPlace()
        {
            var eyes = ResolveEyes();
            if (eyes == null)
            {
                return false;
            }

            var target = _target != null ? _target : transform;
            var pose = EyeHeightPlacement.InFrontOfEyes(
                eyes.position,
                eyes.forward,
                _distanceInFront,
                _heightOffset,
                _alignWithViewDirection,
                target.rotation);
            target.SetPositionAndRotation(pose.position, pose.rotation);
            _onPlaced.Invoke();
            return true;
        }

        private void OnEnable()
        {
            if (_placeOnEnable)
            {
                TryPlace();
            }
        }

        private Transform ResolveEyes()
        {
            if (_eyes != null)
            {
                return _eyes;
            }

            var mainCamera = Camera.main;
            return mainCamera != null ? mainCamera.transform : null;
        }
    }
}
