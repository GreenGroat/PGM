using System.Collections;
using UnityEngine;

namespace Lab3FPS
{
    public class EnemyTarget : MonoBehaviour
    {
        [SerializeField] private string targetName = "Target";
        [SerializeField] private float health = 50f;
        [SerializeField] private float hitFlashTime = 0.08f;

        private Renderer[] renderers;
        private Color[] originalColors;
        private bool dead;

        public string TargetName => targetName;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        public void TakeDamage(float amount)
        {
            if (dead)
            {
                return;
            }

            health -= amount;
            StartCoroutine(HitFlashRoutine());

            if (health <= 0f)
            {
                Die();
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            SetColor(Color.white);
            yield return new WaitForSeconds(hitFlashTime);
            RestoreColors();
        }

        private void Die()
        {
            dead = true;
            Debug.Log($"{targetName} destroyed.");
            Destroy(gameObject);
        }

        private void SetColor(Color color)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = color;
            }
        }

        private void RestoreColors()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].material.color = originalColors[i];
                }
            }
        }
    }
}
