using UnityEngine;

namespace Framework
{
    [RequireComponent(typeof(Collider))]
    public class SnapOnCollisionEnter : MonoBehaviour
    {
        [SerializeField]
        private SnapPoint _snapPoint;

        private void Reset()
        {
            _snapPoint = GetComponent<SnapPoint>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            var candidate = collision.collider.GetComponentInParent<Snappable>();
            if (candidate != null)
            {
                _snapPoint.TryOffer(candidate);
            }
        }
    }
}
