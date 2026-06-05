using UnityEngine;
using UnityEngine.UI;

namespace Lab4Narrative
{
    public class InteractionPrompt : MonoBehaviour
    {
        public static InteractionPrompt Instance { get; private set; }

        [SerializeField] private Text promptText;

        private void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Show(string message)
        {
            if (promptText == null)
            {
                return;
            }

            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
}
