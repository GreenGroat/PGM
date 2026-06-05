using UnityEngine;
using UnityEngine.Events;

namespace Lab7Territory
{
    public class CaptureZone : MonoBehaviour
    {
        public enum Owner
        {
            Neutral,
            Player,
            Enemy
        }

        public Owner currentOwner = Owner.Neutral;

        [Header("Capture")]
        public float captureTime = 5f;
        [Range(-1f, 1f)] public float captureProgress = 0f;
        public int pointsPerSecond = 10;

        [Header("Visuals")]
        public Renderer zoneRenderer;
        public Renderer coreRenderer;
        public Material neutralMaterial;
        public Material playerMaterial;
        public Material enemyMaterial;
        public Material contestedMaterial;

        public UnityEvent onCapturedByPlayer;
        public UnityEvent onCapturedByEnemy;

        public int PlayersInside => playersInside;
        public int EnemiesInside => enemiesInside;
        public bool IsContested => playersInside > 0 && enemiesInside > 0;

        private int playersInside;
        private int enemiesInside;
        private float nextPointTime;

        private void Start()
        {
            UpdateOwnerFromProgress(true);
            UpdateVisuals();
        }

        private void Update()
        {
            float delta = 0f;
            if (playersInside > 0 && enemiesInside == 0)
            {
                delta = Time.deltaTime / Mathf.Max(0.01f, captureTime);
            }
            else if (enemiesInside > 0 && playersInside == 0)
            {
                delta = -Time.deltaTime / Mathf.Max(0.01f, captureTime);
            }

            if (!Mathf.Approximately(delta, 0f))
            {
                captureProgress = Mathf.Clamp(captureProgress + delta, -1f, 1f);
                UpdateOwnerFromProgress(false);
                UpdateVisuals();
            }

            if (Time.time >= nextPointTime)
            {
                nextPointTime = Time.time + 1f;
                if (currentOwner == Owner.Player)
                {
                    ResourceManager.Instance?.AddPlayerPoints(pointsPerSecond);
                }
                else if (currentOwner == Owner.Enemy)
                {
                    ResourceManager.Instance?.AddEnemyPoints(pointsPerSecond);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playersInside++;
            }
            else if (other.CompareTag("Enemy"))
            {
                enemiesInside++;
            }

            UpdateVisuals();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playersInside = Mathf.Max(0, playersInside - 1);
            }
            else if (other.CompareTag("Enemy"))
            {
                enemiesInside = Mathf.Max(0, enemiesInside - 1);
            }

            UpdateVisuals();
        }

        public float GetProgress()
        {
            return Mathf.Abs(captureProgress);
        }

        public float GetSignedProgress()
        {
            return captureProgress;
        }

        private void UpdateOwnerFromProgress(bool force)
        {
            Owner previous = currentOwner;
            if (captureProgress >= 0.99f)
            {
                currentOwner = Owner.Player;
            }
            else if (captureProgress <= -0.99f)
            {
                currentOwner = Owner.Enemy;
            }
            else if (Mathf.Abs(captureProgress) < 0.05f)
            {
                currentOwner = Owner.Neutral;
            }

            if (!force && previous != currentOwner)
            {
                if (currentOwner == Owner.Player)
                {
                    onCapturedByPlayer.Invoke();
                }
                else if (currentOwner == Owner.Enemy)
                {
                    onCapturedByEnemy.Invoke();
                }
            }
        }

        private void UpdateVisuals()
        {
            Material material = neutralMaterial;
            if (IsContested && contestedMaterial != null)
            {
                material = contestedMaterial;
            }
            else if (currentOwner == Owner.Player && playerMaterial != null)
            {
                material = playerMaterial;
            }
            else if (currentOwner == Owner.Enemy && enemyMaterial != null)
            {
                material = enemyMaterial;
            }

            if (zoneRenderer != null && material != null)
            {
                zoneRenderer.material = material;
            }

            if (coreRenderer != null && material != null)
            {
                coreRenderer.material = material;
            }
        }
    }
}
