using UnityEngine;
using UnityEngine.Events;

namespace Lab8Attention
{
    public class InteractableObject : MonoBehaviour
    {
        public UnityEvent onInteract = new UnityEvent();
        public string prompt = "Press E to interact";
        public string completeMessage = "Interaction complete";
        public bool singleUse = true;

        private bool playerInRange;
        private bool used;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                HintManager.Instance?.ShowHint(prompt, 2.2f);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }

        private void Update()
        {
            if (!playerInRange || used)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                onInteract.Invoke();
                HintManager.Instance?.ShowHint(completeMessage, 3f);
                if (singleUse)
                {
                    used = true;
                }
            }
        }
    }
}
