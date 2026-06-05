using UnityEngine;
using UnityEngine.UI;

namespace Lab7Territory
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        public int playerPoints;
        public int enemyPoints;
        public Text scoreText;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            UpdateUI();
        }

        public void AddPlayerPoints(int amount)
        {
            playerPoints += amount;
            UpdateUI();
        }

        public void AddEnemyPoints(int amount)
        {
            enemyPoints += amount;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = "Player " + playerPoints + "  |  Enemy " + enemyPoints;
            }
        }
    }
}
