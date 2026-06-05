using UnityEngine;

namespace Lab4Narrative
{
    [RequireComponent(typeof(CharacterController))]
    public class NarrativePlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.8f;
        [SerializeField] private float runSpeed = 7.2f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float jumpHeight = 1.1f;

        [Header("Look")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 78f;

        private CharacterController controller;
        private Vector3 velocity;
        private float cameraPitch;

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
            GameUIState.Reset();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (GameUIState.ControlsLocked)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Look();
            Move();
        }

        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);
            cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -maxLookAngle, maxLookAngle);

            if (playerCamera != null)
            {
                playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void Move()
        {
            if (controller.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            move = Vector3.ClampMagnitude(move, 1f);

            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            controller.Move(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
