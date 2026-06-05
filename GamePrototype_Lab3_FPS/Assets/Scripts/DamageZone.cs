using UnityEngine;

namespace Lab3FPS
{
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] private int damagePerTick = 15;
        [SerializeField] private float tickInterval = 1f;

        private float nextDamageTime;

        private void OnTriggerStay(Collider other)
        {
            if (Time.time < nextDamageTime || !other.CompareTag("Player"))
            {
                return;
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.TakeDamage(damagePerTick);
            nextDamageTime = Time.time + tickInterval;
        }
    }
}
