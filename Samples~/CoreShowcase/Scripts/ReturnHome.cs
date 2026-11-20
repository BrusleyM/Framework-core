using UnityEngine;

namespace Framework.Samples
{
    public class ReturnHome : MonoBehaviour
    {
        private Pose _home;

        private void Awake()
        {
            _home = new Pose(transform.position, transform.rotation);
        }

        public void SendHome()
        {
            transform.SetPositionAndRotation(_home.position, _home.rotation);
        }
    }
}
