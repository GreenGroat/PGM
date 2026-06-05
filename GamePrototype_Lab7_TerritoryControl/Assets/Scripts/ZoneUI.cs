using UnityEngine;
using UnityEngine.UI;

namespace Lab7Territory
{
    public class ZoneUI : MonoBehaviour
    {
        public CaptureZone zone;
        public Slider progressSlider;
        public Image fillImage;
        public Text labelText;
        public Camera mainCamera;

        public Color neutralColor = new Color(0.5f, 0.55f, 0.6f);
        public Color playerColor = new Color(0.08f, 0.78f, 1f);
        public Color enemyColor = new Color(1f, 0.12f, 0.32f);
        public Color contestedColor = new Color(1f, 0.75f, 0.1f);

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (zone == null)
            {
                return;
            }

            if (mainCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }

            if (progressSlider != null)
            {
                progressSlider.value = zone.GetProgress();
            }

            Color color = neutralColor;
            string label = "NEUTRAL";
            if (zone.IsContested)
            {
                color = contestedColor;
                label = "CONTESTED";
            }
            else if (zone.currentOwner == CaptureZone.Owner.Player)
            {
                color = playerColor;
                label = "PLAYER";
            }
            else if (zone.currentOwner == CaptureZone.Owner.Enemy)
            {
                color = enemyColor;
                label = "ENEMY";
            }

            if (fillImage != null)
            {
                fillImage.color = color;
            }

            if (labelText != null)
            {
                labelText.text = label;
                labelText.color = color;
            }
        }
    }
}
