using UnityEngine;

namespace Framework
{
    [RequireComponent(typeof(Collider))]
    public class SnapOnCollisionEnter : MonoBehaviour
    {
        [SerializeField]
        private SnapPoint _snapPoint;

        private void Awake()
        {
            if (_snapPoint == null)
            {
                _snapPoint = GetComponent<SnapPoint>();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_snapPoint != null)
            {
                _snapPoint.TryOfferFrom(collision.collider);
            }
        }
    }
}
