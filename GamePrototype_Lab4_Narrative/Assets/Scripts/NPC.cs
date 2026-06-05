using UnityEngine;

namespace Lab4Narrative
{
    public class NPC : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField] private string npcName = "Archivist";
        [SerializeField] [TextArea(2, 5)] private string openingLine = "The archive remembers what the city forgot.";
        [SerializeField] private string[] answers;
        [SerializeField] [TextArea(2, 5)] private string[] responses;

        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = new Color(0.65f, 0.35f, 1f);
        [SerializeField] private Light highlightLight;
        [SerializeField] private string prompt = "E - talk";

        private Renderer[] renderers;
        private Color[] originalColors;
        private bool playerInRange;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].material.color;
            }

            SetHighlighted(false);
        }

        private void Update()
        {
            if (playerInRange && !GameUIState.ControlsLocked && Input.GetKeyDown(KeyCode.E))
            {
                OpenDialogue();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = true;
            SetHighlighted(true);
            InteractionPrompt.Instance?.Show(prompt);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = false;
            SetHighlighted(false);
            InteractionPrompt.Instance?.Hide();
        }

        private void OpenDialogue()
        {
            Object.FindFirstObjectByType<DialogueUI>()?.ShowDialogue(npcName, openingLine, answers, responses);
            Object.FindFirstObjectByType<QuestManager>()?.NPCTalked();
            InteractionPrompt.Instance?.Hide();
        }

        private void SetHighlighted(bool highlighted)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].material.color = highlighted ? highlightColor : originalColors[i];
                }
            }

            if (highlightLight != null)
            {
                highlightLight.enabled = highlighted;
            }
        }
    }
}
