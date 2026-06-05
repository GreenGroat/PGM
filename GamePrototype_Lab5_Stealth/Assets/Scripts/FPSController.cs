using UnityEngine;

namespace Lab5Stealth
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStealth))]
    public class FPSController : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform cameraPivot;

        [Header("Movement")]
        public float walkSpeed = 4f;
        public float sprintSpeed = 7f;
        public float crouchSpeed = 2f;
        public float jumpHeight = 1.2f;
        public float gravity = -20f;

        [Header("View")]
        public float mouseSensitivity = 2.1f;
        public float minPitch = -82f;
        public float maxPitch = 82f;

        [Header("Crouch")]
        public float standHeight = 1.8f;
        public float crouchHeight = 1.05f;
        public float standCameraHeight = 0.72f;
        public float crouchCameraHeight = 0.36f;
        public float crouchLerpSpeed = 10f;

        private CharacterController controller;
        private PlayerStealth stealth;
        private float verticalVelocity;
        private float pitch;

        public bool InputLocked { get; set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stealth = GetComponent<PlayerStealth>();

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
                stealth.UpdateMotion(false, 0f, controller.isGrounded);
                return;
            }

            UpdateLook();
            UpdateMovement();
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

        private void UpdateMovement()
        {
            bool crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.C);
            bool sprinting = Input.GetKey(KeyCode.LeftShift) && !crouching;

            stealth.SetCrouching(crouching);
            stealth.SetSprinting(sprinting);

            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            Vector3 input = Vector3.ClampMagnitude(new Vector3(inputX, 0f, inputZ), 1f);
            Vector3 move = transform.right * input.x + transform.forward * input.z;

            float speed = crouching ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space) && !crouching)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            bool moving = input.sqrMagnitude > 0.01f;
            stealth.UpdateMotion(moving, speed, controller.isGrounded);
            UpdateCrouchShape(crouching);
        }

        private void UpdateCrouchShape(bool crouching)
        {
            float targetHeight = crouching ? crouchHeight : standHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, crouchLerpSpeed * Time.deltaTime);
            controller.center = Vector3.up * (controller.height * 0.5f);

            if (cameraPivot != null)
            {
                float targetCameraHeight = crouching ? crouchCameraHeight : standCameraHeight;
                Vector3 localPos = cameraPivot.localPosition;
                localPos.y = Mathf.Lerp(localPos.y, targetCameraHeight, crouchLerpSpeed * Time.deltaTime);
                cameraPivot.localPosition = localPos;
            }
        }
    }
}
