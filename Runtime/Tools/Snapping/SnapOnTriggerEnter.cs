using UnityEngine;

namespace Framework
{
    public class SnapOnTriggerEnter : SnapOnContact
    {
        private void OnTriggerEnter(Collider other)
        {
            OfferToSnapPoint(other);
        }
    }
}
