using UnityEngine;

namespace Framework.Samples
{
    public class ShowcaseCameraRig : MonoBehaviour
    {
        [SerializeField]
        private Transform _head;
        [SerializeField]
        private float _walkSpeed = 3.2f;
        [SerializeField]
        private float _runMultiplier = 1.8f;
        [SerializeField]
        private float _lookSpeed = 2.2f;
        [SerializeField]
        private float _plazaRadius = 10f;

        private float _yaw;
        private float _pitch;

        private void Start()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            _yaw = transform.eulerAngles.y;
            LockCursor(true);
#else
            Debug.LogWarning("The showcase camera rig needs the Input Manager (Active Input Handling set to Input Manager or Both).", this);
            enabled = false;
#endif
        }

        private void OnDisable()
        {
            LockCursor(false);
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LockCursor(Cursor.lockState != CursorLockMode.Locked);
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    LockCursor(true);
                }
                return;
            }

            Look();
            Walk();
        }

        private void Look()
        {
            _yaw += Input.GetAxis("Mouse X") * _lookSpeed;
            _pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * _lookSpeed, -80f, 80f);
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void Walk()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            var speed = _walkSpeed * (Input.GetKey(KeyCode.LeftShift) ? _runMultiplier : 1f);
            var next = transform.position + transform.rotation * input.normalized * speed * Time.deltaTime;
            var flat = new Vector2(next.x, next.z);
            if (flat.magnitude > _plazaRadius)
            {
                flat = flat.normalized * _plazaRadius;
            }
            transform.position = new Vector3(flat.x, transform.position.y, flat.y);
        }
#endif

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
