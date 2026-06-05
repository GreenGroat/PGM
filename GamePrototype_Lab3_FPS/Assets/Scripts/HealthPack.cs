using UnityEngine;

namespace Lab3FPS
{
    public class HealthPack : MonoBehaviour
    {
        [SerializeField] private int healAmount = 25;
        [SerializeField] private float rotationSpeed = 90f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
