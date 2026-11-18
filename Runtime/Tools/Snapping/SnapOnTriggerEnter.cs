using UnityEngine;

namespace Framework
{
    [RequireComponent(typeof(Collider))]
    public class SnapOnTriggerEnter : MonoBehaviour
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

        private void OnTriggerEnter(Collider other)
        {
            if (_snapPoint != null)
            {
                _snapPoint.TryOfferFrom(other);
            }
        }
    }
}
