using UnityEngine;
using UnityEngine.UI;

namespace Lab3FPS
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private Text healthText;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject deathScreen;
        [SerializeField] private FPSController fpsController;
        [SerializeField] private WeaponSwitcher weaponSwitcher;

        private int currentHealth;
        private bool dead;

        private void Awake()
        {
            currentHealth = maxHealth;

            if (fpsController == null)
            {
                fpsController = GetComponent<FPSController>();
            }

            if (weaponSwitcher == null)
            {
                weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
            }
        }

        private void Start()
        {
            if (deathScreen != null)
            {
                deathScreen.SetActive(false);
            }

            UpdateHealthUI();
        }

        public void TakeDamage(int damage)
        {
            if (dead)
            {
                return;
            }

            currentHealth = Mathf.Max(currentHealth - damage, 0);
            UpdateHealthUI();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (dead)
            {
                return;
            }

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            UpdateHealthUI();
            SetStatus($"+{amount} HP.");
        }

        private void Die()
        {
            dead = true;
            SetStatus("Player died.");

            if (deathScreen != null)
            {
                deathScreen.SetActive(true);
            }

            if (fpsController != null)
            {
                fpsController.SetControlsLocked(true);
            }

            if (weaponSwitcher != null)
            {
                weaponSwitcher.SetWeaponsEnabled(false);
            }
        }

        private void UpdateHealthUI()
        {
            if (healthText != null)
            {
                healthText.text = $"HP: {currentHealth}/{maxHealth}";
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
