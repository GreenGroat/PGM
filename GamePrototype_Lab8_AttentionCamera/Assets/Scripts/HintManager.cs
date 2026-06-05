using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lab8Attention
{
    public class HintManager : MonoBehaviour
    {
        public static HintManager Instance { get; private set; }

        public GameObject hintPanel;
        public Text hintText;

        private Coroutine hideRoutine;

        private void Awake()
        {
            Instance = this;
            if (hintPanel != null)
            {
                hintPanel.SetActive(false);
            }
        }

        public void ShowHint(string text, float duration)
        {
            if (hintPanel == null || hintText == null)
            {
                return;
            }

            hintText.text = text;
            hintPanel.SetActive(true);

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }
            hideRoutine = StartCoroutine(HideAfter(duration));
        }

        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (hintPanel != null)
            {
                hintPanel.SetActive(false);
            }
        }
    }
}
