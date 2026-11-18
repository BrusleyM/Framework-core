using UnityEngine;

namespace Framework
{
    [RequireComponent(typeof(Collider))]
    public class SnapOnTriggerEnter : MonoBehaviour
    {
        [SerializeField]
        private SnapPoint _snapPoint;

        private void Reset()
        {
            _snapPoint = GetComponent<SnapPoint>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var candidate = other.GetComponentInParent<Snappable>();
            if (candidate != null)
            {
                _snapPoint.TryOffer(candidate);
            }
        }
    }
}
