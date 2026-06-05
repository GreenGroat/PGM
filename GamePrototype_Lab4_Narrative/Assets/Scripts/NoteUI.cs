using UnityEngine;
using UnityEngine.UI;

namespace Lab4Narrative
{
    public class NoteUI : MonoBehaviour
    {
        [SerializeField] private GameObject notePanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text contentText;
        [SerializeField] private Button closeButton;

        private bool open;

        private void Start()
        {
            if (notePanel != null)
            {
                notePanel.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseNote);
            }
        }

        public void ShowNote(string title, string content)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (contentText != null)
            {
                contentText.text = content;
            }

            if (notePanel != null)
            {
                notePanel.SetActive(true);
            }

            if (!open)
            {
                open = true;
                GameUIState.PushOverlay();
            }
        }

        public void CloseNote()
        {
            if (notePanel != null)
            {
                notePanel.SetActive(false);
            }

            if (open)
            {
                open = false;
                GameUIState.PopOverlay();
            }
        }
    }
}
