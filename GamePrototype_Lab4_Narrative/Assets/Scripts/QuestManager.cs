using UnityEngine;
using UnityEngine.UI;

namespace Lab4Narrative
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private int totalNotes = 4;
        [SerializeField] private bool needTalkToNPC = true;
        [SerializeField] private Text questStatusText;
        [SerializeField] private GameObject questCompletePanel;
        [SerializeField] private ExitDoor exitDoor;

        private int notesCollected;
        private bool talkedToNPC;
        private bool completed;

        public bool IsQuestComplete => completed;

        private void Start()
        {
            if (questCompletePanel != null)
            {
                questCompletePanel.SetActive(false);
            }

            UpdateQuestUI();
        }

        public void NoteCollected()
        {
            notesCollected = Mathf.Min(notesCollected + 1, totalNotes);
            UpdateQuestUI();
            CheckCompletion();
        }

        public void NPCTalked()
        {
            talkedToNPC = true;
            UpdateQuestUI();
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (completed)
            {
                return;
            }

            bool notesDone = notesCollected >= totalNotes;
            bool talkDone = !needTalkToNPC || talkedToNPC;

            if (notesDone && talkDone)
            {
                completed = true;
                if (questCompletePanel != null)
                {
                    questCompletePanel.SetActive(true);
                }

                if (exitDoor != null)
                {
                    exitDoor.Open();
                }

                UpdateQuestUI();
            }
        }

        private void UpdateQuestUI()
        {
            if (questStatusText == null)
            {
                return;
            }

            string talkStatus = needTalkToNPC ? $"\nKeeper: {(talkedToNPC ? "spoken" : "not yet")}" : string.Empty;
            string doorStatus = completed ? "\nExit unlocked" : "\nExit sealed";
            questStatusText.text = $"Notes: {notesCollected}/{totalNotes}{talkStatus}{doorStatus}";
        }
    }
}
