using UnityEngine;
using UnityEngine.UI;

namespace Lab8Attention
{
    public class CameraController : MonoBehaviour
    {
        public Camera playerCamera;
        public Transform followTarget;
        public Vector3 followOffset = new Vector3(0f, 2.05f, -4.8f);
        public Vector3 lookAtOffset = new Vector3(0f, 1.1f, 0f);
        public float transitionSpeed = 5f;
        public float normalFov = 68f;
        public float focusFov = 42f;
        public Image focusOverlay;

        private Transform fixedCameraPoint;
        private float targetFov;

        public bool IsInFixedMode { get; private set; }

        private void Start()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            targetFov = normalFov;
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = normalFov;
            }
        }

        private void LateUpdate()
        {
            if (playerCamera == null)
            {
                return;
            }

            Transform cameraTransform = playerCamera.transform;
            if (IsInFixedMode && fixedCameraPoint != null)
            {
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, fixedCameraPoint.position, transitionSpeed * Time.deltaTime);
                cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, fixedCameraPoint.rotation, transitionSpeed * Time.deltaTime);
            }
            else if (followTarget != null)
            {
                Vector3 desiredPosition = followTarget.TransformPoint(followOffset);
                Vector3 lookTarget = followTarget.position + followTarget.TransformDirection(lookAtOffset);
                Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - desiredPosition, Vector3.up);

                cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, transitionSpeed * Time.deltaTime);
                cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, desiredRotation, transitionSpeed * Time.deltaTime);
            }

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, transitionSpeed * Time.deltaTime);
            UpdateFocusOverlay();
        }

        public void SetFixedCamera(Transform fixedPoint, float duration = 2f)
        {
            if (fixedPoint == null)
            {
                return;
            }

            fixedCameraPoint = fixedPoint;
            IsInFixedMode = true;
            targetFov = focusFov;

            CancelInvoke(nameof(ReturnToFollow));
            Invoke(nameof(ReturnToFollow), duration);
        }

        public void ReturnToFollow()
        {
            IsInFixedMode = false;
            fixedCameraPoint = null;
            targetFov = normalFov;
        }

        private void UpdateFocusOverlay()
        {
            if (focusOverlay == null)
            {
                return;
            }

            Color color = focusOverlay.color;
            float targetAlpha = IsInFixedMode ? 0.22f : 0f;
            color.a = Mathf.Lerp(color.a, targetAlpha, transitionSpeed * Time.deltaTime);
            focusOverlay.color = color;
        }
    }
}
