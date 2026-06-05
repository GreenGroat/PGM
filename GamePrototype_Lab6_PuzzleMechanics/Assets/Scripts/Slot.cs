using UnityEngine;
using UnityEngine.Events;

namespace Lab6Puzzle
{
    public class Slot : MonoBehaviour
    {
        public string[] acceptedTags;
        public Transform snapPoint;
        public Renderer visualRenderer;
        public Material idleMaterial;
        public Material activeMaterial;
        public Material solvedMaterial;
        public UnityEvent OnItemPlaced = new UnityEvent();

        public bool IsAvailable { get; private set; } = true;
        public bool IsOccupied { get; private set; }

        private void Start()
        {
            UpdateVisuals();
        }

        public bool CanAccept(GameObject item)
        {
            if (!IsAvailable || IsOccupied || item == null)
            {
                return false;
            }

            foreach (string acceptedTag in acceptedTags)
            {
                if (item.CompareTag(acceptedTag))
                {
                    return true;
                }
            }

            return false;
        }

        public void PlaceItem(GameObject item)
        {
            if (!CanAccept(item))
            {
                return;
            }

            IsOccupied = true;
            Vector3 targetPosition = snapPoint != null ? snapPoint.position : transform.position + Vector3.up * 0.5f;
            Quaternion targetRotation = snapPoint != null ? snapPoint.rotation : transform.rotation;

            DraggableObject draggable = item.GetComponent<DraggableObject>();
            if (draggable != null)
            {
                draggable.LockInPlace(targetPosition, targetRotation);
            }
            else
            {
                item.transform.SetPositionAndRotation(targetPosition, targetRotation);
            }

            UpdateVisuals();
            PuzzleManager.Instance?.NotifyItemPlaced(item);
            OnItemPlaced.Invoke();
        }

        public void SetAvailable(bool available)
        {
            IsAvailable = available;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (visualRenderer == null)
            {
                return;
            }

            Material material = idleMaterial;
            if (IsOccupied && solvedMaterial != null)
            {
                material = solvedMaterial;
            }
            else if (IsAvailable && activeMaterial != null)
            {
                material = activeMaterial;
            }

            if (material != null)
            {
                visualRenderer.material = material;
            }
        }
    }
}
