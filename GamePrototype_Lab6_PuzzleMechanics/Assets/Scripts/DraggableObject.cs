using UnityEngine;

namespace Lab6Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class DraggableObject : MonoBehaviour
    {
        public float dragPlaneHeight = 0.75f;
        public float returnSpeed = 12f;
        public Material hoverMaterial;

        private Camera mainCamera;
        private Renderer objectRenderer;
        private Material originalMaterial;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 dragOffset;
        private bool isDragging;
        private bool isLocked;
        private bool returning;

        private void Start()
        {
            mainCamera = Camera.main;
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                originalMaterial = objectRenderer.material;
            }

            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }

        private void Update()
        {
            if (isLocked)
            {
                return;
            }

            if (isDragging)
            {
                transform.position = GetMouseWorldPosition() + dragOffset;
            }
            else if (returning)
            {
                transform.position = Vector3.Lerp(transform.position, originalPosition, returnSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, returnSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, originalPosition) < 0.03f)
                {
                    transform.position = originalPosition;
                    transform.rotation = originalRotation;
                    returning = false;
                }
            }
        }

        private void OnMouseDown()
        {
            if (isLocked)
            {
                return;
            }

            isDragging = true;
            returning = false;
            dragOffset = transform.position - GetMouseWorldPosition();
            PuzzleManager.Instance?.SetObjective("Drag the item to a matching glowing slot.");
        }

        private void OnMouseUp()
        {
            if (!isDragging || isLocked)
            {
                return;
            }

            isDragging = false;
            Slot targetSlot = GetSlotUnderObject();
            if (targetSlot != null && targetSlot.CanAccept(gameObject))
            {
                targetSlot.PlaceItem(gameObject);
            }
            else
            {
                RestoreDefaultVisuals();
                returning = true;
                PuzzleManager.Instance?.SetObjective("Wrong place. Try the matching slot.");
            }
        }

        private void OnMouseEnter()
        {
            if (isLocked)
            {
                return;
            }

            transform.localScale = Vector3.one * 1.12f;
            if (objectRenderer != null && hoverMaterial != null)
            {
                objectRenderer.material = hoverMaterial;
            }
        }

        private void OnMouseExit()
        {
            if (isDragging || isLocked)
            {
                return;
            }

            transform.localScale = Vector3.one;
            if (objectRenderer != null && originalMaterial != null)
            {
                objectRenderer.material = originalMaterial;
            }
        }

        public void LockInPlace(Vector3 position, Quaternion rotation)
        {
            isLocked = true;
            isDragging = false;
            returning = false;
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = Vector3.one;
            RestoreDefaultVisuals();
        }

        private Vector3 GetMouseWorldPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, dragPlaneHeight, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return transform.position;
        }

        private Slot GetSlotUnderObject()
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 1.5f, Vector3.down, 4f);
            Slot bestSlot = null;
            float bestDistance = float.MaxValue;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                Slot slot = hit.collider.GetComponentInParent<Slot>();
                if (slot != null && hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestSlot = slot;
                }
            }

            return bestSlot;
        }

        private void RestoreDefaultVisuals()
        {
            transform.localScale = Vector3.one;
            if (objectRenderer != null && originalMaterial != null)
            {
                objectRenderer.material = originalMaterial;
            }
        }
    }
}
