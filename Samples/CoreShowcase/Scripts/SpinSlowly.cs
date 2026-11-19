using UnityEngine;

namespace Framework.Samples
{
    public class SpinSlowly : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _degreesPerSecond = new Vector3(0f, 25f, 0f);
        [SerializeField]
        private float _bobHeight = 0.08f;
        [SerializeField]
        private float _bobSpeed = 1.2f;

        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            transform.Rotate(_degreesPerSecond * Time.deltaTime, Space.Self);
            transform.position = _startPosition + Vector3.up * (Mathf.Sin(Time.time * _bobSpeed) * _bobHeight);
        }
    }
}
