using UnityEngine;

namespace Lab3FPS
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -20f;

        [Header("Mouse Look")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float maxLookAngle = 82f;

        [Header("Recoil")]
        [SerializeField] private float recoilRecoverySpeed = 9f;

        private CharacterController controller;
        private Vector3 velocity;
        private float cameraPitch;
        private float recoilPitch;
        private bool controlsLocked;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (playerCamera == null && Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        private void Start()
        {
            SetCursorLock(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLock(false);
            }

            if (Input.GetMouseButtonDown(0) && !controlsLocked)
            {
                SetCursorLock(true);
            }

            if (controlsLocked)
            {
                return;
            }

            HandleMouseLook();
            HandleMovement();
        }

        public void AddRecoil(float amount)
        {
            recoilPitch += amount;
        }

        public void SetControlsLocked(bool locked)
        {
            controlsLocked = locked;
            SetCursorLock(!locked);
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            recoilPitch = Mathf.Lerp(recoilPitch, 0f, recoilRecoverySpeed * Time.deltaTime);
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

            if (playerCamera != null)
            {
                float finalPitch = Mathf.Clamp(cameraPitch - recoilPitch, -maxLookAngle, maxLookAngle);
                playerCamera.localRotation = Quaternion.Euler(finalPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            if (controller.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            move = Vector3.ClampMagnitude(move, 1f);

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            controller.Move(move * currentSpeed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private static void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
