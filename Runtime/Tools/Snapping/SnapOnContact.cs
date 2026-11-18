using UnityEngine;

namespace Framework
{
    [RequireComponent(typeof(Collider))]
    public abstract class SnapOnContact : MonoBehaviour
    {
        [SerializeField]
        private SnapPoint _snapPoint;

        private void Awake()
        {
            ResolveSnapPointOnThisObject();
        }

        private void Reset()
        {
            ResolveSnapPointOnThisObject();
        }

        protected void OfferToSnapPoint(Collider other)
        {
            if (_snapPoint != null)
            {
                _snapPoint.TryOfferFrom(other);
            }
        }

        private void ResolveSnapPointOnThisObject()
        {
            if (_snapPoint == null)
            {
                _snapPoint = GetComponent<SnapPoint>();
            }
        }
    }
}
