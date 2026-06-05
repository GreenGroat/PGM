using UnityEngine;
using UnityEngine.UI;

namespace Lab6Puzzle
{
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager Instance { get; private set; }

        public Slot gemSlot;
        public GameObject gemItem;
        public Door innerDoor;
        public Door exitDoor;
        public GameObject winPanel;
        public Text objectiveText;

        private bool gemPhaseEnabled;
        private bool puzzleCompleted;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            gemPhaseEnabled = false;
            puzzleCompleted = false;

            if (gemSlot != null)
            {
                gemSlot.SetAvailable(false);
            }

            if (gemItem != null)
            {
                gemItem.SetActive(false);
            }

            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            SetObjective("Place the cyan key cube on the first platform.");
        }

        public void NotifyItemPlaced(GameObject item)
        {
            if (item == null)
            {
                return;
            }

            if (item.CompareTag("Key"))
            {
                if (innerDoor != null)
                {
                    innerDoor.Open();
                }

                EnableGemPhase();
            }
            else if (item.CompareTag("Gem"))
            {
                if (exitDoor != null)
                {
                    exitDoor.Open();
                }

                CompletePuzzle();
            }
        }

        public void EnableGemPhase()
        {
            if (gemPhaseEnabled)
            {
                return;
            }

            gemPhaseEnabled = true;

            if (gemSlot != null)
            {
                gemSlot.SetAvailable(true);
            }

            if (gemItem != null)
            {
                gemItem.SetActive(true);
            }

            SetObjective("Good. The magenta core is active now - drag it to the second platform.");
        }

        public void CompletePuzzle()
        {
            if (puzzleCompleted)
            {
                return;
            }

            puzzleCompleted = true;

            if (exitDoor != null)
            {
                exitDoor.Open();
            }

            SetObjective("Puzzle solved. Exit mechanism unlocked.");
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
        }

        public void SetObjective(string message)
        {
            if (objectiveText != null)
            {
                objectiveText.text = message;
            }
        }
    }
}
