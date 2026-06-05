using UnityEngine;

namespace Lab4Narrative
{
    public class Note : MonoBehaviour
    {
        [Header("Note Data")]
        [SerializeField] private string noteTitle = "Note";
        [SerializeField] [TextArea(4, 10)] private string noteContent = "Text...";

        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = new Color(0.2f, 1f, 0.85f);
        [SerializeField] private Light highlightLight;
        [SerializeField] private string prompt = "E - read note";

        private Renderer[] renderers;
        private Color[] originalColors;
        private bool playerInRange;
        private bool collected;

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
            if (playerInRange && !collected && !GameUIState.ControlsLocked && Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || collected)
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

        private void Interact()
        {
            collected = true;
            Inventory inventory = Object.FindFirstObjectByType<Inventory>();
            inventory?.AddNote(new NoteData(noteTitle, noteContent));

            QuestManager questManager = Object.FindFirstObjectByType<QuestManager>();
            questManager?.NoteCollected();

            NoteUI noteUI = Object.FindFirstObjectByType<NoteUI>();
            noteUI?.ShowNote(noteTitle, noteContent);

            InteractionPrompt.Instance?.Hide();
            gameObject.SetActive(false);
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
