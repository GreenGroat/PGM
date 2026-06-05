using UnityEngine;

namespace Lab4Narrative
{
    public class ExitDoor : MonoBehaviour
    {
        [SerializeField] private Transform doorVisual;
        [SerializeField] private Vector3 openedOffset = new Vector3(0f, 3.5f, 0f);
        [SerializeField] private float openSpeed = 2.5f;
        [SerializeField] private Light unlockedLight;
        [SerializeField] private string lockedPrompt = "Door sealed: finish the archive quest";

        private Vector3 closedPosition;
        private Vector3 targetPosition;
        private bool opened;

        private void Awake()
        {
            if (doorVisual == null)
            {
                doorVisual = transform;
            }

            closedPosition = doorVisual.position;
            targetPosition = closedPosition;

            if (unlockedLight != null)
            {
                unlockedLight.enabled = false;
            }
        }

        private void Update()
        {
            doorVisual.position = Vector3.Lerp(doorVisual.position, targetPosition, openSpeed * Time.deltaTime);
        }

        public void Open()
        {
            opened = true;
            targetPosition = closedPosition + openedOffset;

            if (unlockedLight != null)
            {
                unlockedLight.enabled = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            InteractionPrompt.Instance?.Show(opened ? "Exit open" : lockedPrompt);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                InteractionPrompt.Instance?.Hide();
            }
        }
    }
}
