using UnityEngine;

namespace Lab7Territory
{
    [RequireComponent(typeof(CharacterController))]
    public class TopDownPlayerController : MonoBehaviour
    {
        public Camera playerCamera;
        public Transform cameraPivot;
        public float moveSpeed = 5.6f;
        public float gravity = -18f;
        public float mouseSensitivity = 2.1f;
        public float minPitch = -78f;
        public float maxPitch = 78f;

        private CharacterController controller;
        private float verticalVelocity;
        private float pitch;

        public bool InputLocked { get; set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (cameraPivot == null && playerCamera != null)
            {
                cameraPivot = playerCamera.transform.parent;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (InputLocked)
            {
                return;
            }

            UpdateLook();
            UpdateMove();
        }

        private void UpdateLook()
        {
            float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
            float pitchDelta = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * yaw);
            pitch = Mathf.Clamp(pitch - pitchDelta, minPitch, maxPitch);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void UpdateMove()
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            Vector3 input = Vector3.ClampMagnitude(new Vector3(inputX, 0f, inputZ), 1f);
            Vector3 move = transform.right * input.x + transform.forward * input.z;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
