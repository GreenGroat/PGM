using UnityEngine;

namespace Lab8Attention
{
    [RequireComponent(typeof(CharacterController))]
    public class NarrativePlayerController : MonoBehaviour
    {
        public Transform viewPivot;
        public CameraController cameraController;

        [Header("Movement")]
        public float moveSpeed = 4.2f;
        public float gravity = -20f;
        public float mouseSensitivity = 2f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        public bool InputLocked { get; set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            bool cameraLocked = cameraController != null && cameraController.IsInFixedMode;
            if (InputLocked || cameraLocked)
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
            if (viewPivot != null)
            {
                viewPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
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
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
