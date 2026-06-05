using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lab4Narrative
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform notesContainer;
        [SerializeField] private Button noteButtonPrefab;
        [SerializeField] private Text emptyText;

        private readonly List<NoteData> notes = new List<NoteData>();
        private bool open;

        public IReadOnlyList<NoteData> Notes => notes;

        private void Start()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            UpdateEmptyState();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
        }

        public void AddNote(NoteData note)
        {
            if (notes.Exists(existing => existing.title == note.title))
            {
                return;
            }

            notes.Add(note);
            CreateNoteButton(note);
            UpdateEmptyState();
        }

        public void ToggleInventory()
        {
            if (open)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        public void OpenInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }

            if (!open)
            {
                open = true;
                GameUIState.PushOverlay();
            }
        }

        public void CloseInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            if (open)
            {
                open = false;
                GameUIState.PopOverlay();
            }
        }

        private void CreateNoteButton(NoteData note)
        {
            if (noteButtonPrefab == null || notesContainer == null)
            {
                return;
            }

            Button button = Instantiate(noteButtonPrefab, notesContainer);
            button.gameObject.SetActive(true);
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = note.title;
            }

            button.onClick.AddListener(() =>
            {
                CloseInventory();
                Object.FindFirstObjectByType<NoteUI>()?.ShowNote(note.title, note.content);
            });
        }

        private void UpdateEmptyState()
        {
            if (emptyText != null)
            {
                emptyText.gameObject.SetActive(notes.Count == 0);
            }
        }
    }
}
