using UnityEngine;
using UnityEngine.UI;

namespace Lab5Stealth
{
    public class StealthIndicator : MonoBehaviour
    {
        public PlayerStealth playerStealth;
        public EnemyStateMachine[] enemies;
        public Image fillImage;
        public Text statusText;

        public Color safeColor = new Color(0.1f, 0.95f, 0.65f);
        public Color noiseColor = new Color(0.2f, 0.75f, 1f);
        public Color suspicionColor = new Color(1f, 0.82f, 0.18f);
        public Color alertColor = new Color(1f, 0.16f, 0.28f);

        private void Start()
        {
            if (playerStealth == null)
            {
                playerStealth = FindObjectOfType<PlayerStealth>();
            }

            if (enemies == null || enemies.Length == 0)
            {
                enemies = FindObjectsOfType<EnemyStateMachine>();
            }
        }

        private void Update()
        {
            float level = playerStealth != null ? playerStealth.CurrentNoise01 * 0.35f : 0f;
            string label = "HIDDEN";
            Color color = safeColor;

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                {
                    continue;
                }

                if (enemies[i].CurrentState == EnemyStateMachine.State.Alert)
                {
                    level = 1f;
                    label = "ALERT";
                    color = alertColor;
                    break;
                }

                if (enemies[i].CurrentState == EnemyStateMachine.State.Suspicion && level < 0.65f)
                {
                    level = 0.65f;
                    label = "SUSPICION";
                    color = suspicionColor;
                }
            }

            if (level > 0.08f && level < 0.65f)
            {
                label = "NOISE";
                color = noiseColor;
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(level);
                fillImage.color = color;
            }

            if (statusText != null)
            {
                statusText.text = label;
                statusText.color = color;
            }
        }
    }
}
