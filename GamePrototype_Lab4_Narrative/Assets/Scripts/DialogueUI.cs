using UnityEngine;
using UnityEngine.UI;

namespace Lab4Narrative
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Text nameText;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private Button closeButton;

        private bool open;

        private void Start()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseDialogue);
            }
        }

        public void ShowDialogue(string npcName, string openingLine, string[] answers, string[] responses)
        {
            if (nameText != null)
            {
                nameText.text = npcName;
            }

            if (dialogueText != null)
            {
                dialogueText.text = openingLine;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                Button button = answerButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool hasAnswer = answers != null && i < answers.Length && !string.IsNullOrWhiteSpace(answers[i]);
                button.gameObject.SetActive(hasAnswer);

                if (!hasAnswer)
                {
                    continue;
                }

                int index = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    string response = responses != null && index < responses.Length ? responses[index] : "The conversation fades.";
                    if (dialogueText != null)
                    {
                        dialogueText.text = response;
                    }

                    HideAnswers();
                });

                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = answers[i];
                }
            }

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            if (!open)
            {
                open = true;
                GameUIState.PushOverlay();
            }
        }

        public void CloseDialogue()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (open)
            {
                open = false;
                GameUIState.PopOverlay();
            }
        }

        private void HideAnswers()
        {
            foreach (Button button in answerButtons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }
}
