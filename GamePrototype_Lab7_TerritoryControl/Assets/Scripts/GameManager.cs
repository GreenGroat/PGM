using UnityEngine;
using UnityEngine.UI;

namespace Lab7Territory
{
    public class GameManager : MonoBehaviour
    {
        public CaptureZone[] zones;
        public ResourceManager resourceManager;
        public int targetPoints = 180;
        public GameObject winPanel;
        public Text winText;

        private bool gameEnded;

        private void Start()
        {
            Time.timeScale = 1f;

            if (zones == null || zones.Length == 0)
            {
                zones = FindObjectsOfType<CaptureZone>();
            }

            if (resourceManager == null)
            {
                resourceManager = FindObjectOfType<ResourceManager>();
            }

            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (gameEnded)
            {
                return;
            }

            bool allPlayer = zones.Length > 0;
            bool allEnemy = zones.Length > 0;

            foreach (CaptureZone zone in zones)
            {
                if (zone == null)
                {
                    continue;
                }

                if (zone.currentOwner != CaptureZone.Owner.Player)
                {
                    allPlayer = false;
                }

                if (zone.currentOwner != CaptureZone.Owner.Enemy)
                {
                    allEnemy = false;
                }
            }

            if (allPlayer || (resourceManager != null && resourceManager.playerPoints >= targetPoints))
            {
                GameOver("PLAYER CONTROLS THE GRID");
            }
            else if (allEnemy || (resourceManager != null && resourceManager.enemyPoints >= targetPoints))
            {
                GameOver("ENEMY CONTROLS THE GRID");
            }
        }

        private void GameOver(string message)
        {
            gameEnded = true;
            if (winText != null)
            {
                winText.text = message;
            }

            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }

            Time.timeScale = 0f;
        }
    }
}
