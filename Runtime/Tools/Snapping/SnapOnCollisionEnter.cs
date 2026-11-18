using UnityEngine;

namespace Framework
{
    public class SnapOnCollisionEnter : SnapOnContact
    {
        private void OnCollisionEnter(Collision collision)
        {
            OfferToSnapPoint(collision.collider);
        }
    }
}
