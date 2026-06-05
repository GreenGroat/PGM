using UnityEngine;
using UnityEngine.UI;

namespace Lab7Territory
{
    public class ZoneStatusHUD : MonoBehaviour
    {
        public CaptureZone[] zones;
        public Image[] radialFills;
        public Image[] rings;
        public Text[] labels;

        public Color neutralColor = new Color(0.55f, 0.58f, 0.64f);
        public Color playerColor = new Color(0.05f, 0.78f, 1f);
        public Color enemyColor = new Color(1f, 0.12f, 0.28f);
        public Color contestedColor = new Color(1f, 0.72f, 0.1f);

        private void Update()
        {
            if (zones == null)
            {
                return;
            }

            for (int i = 0; i < zones.Length; i++)
            {
                CaptureZone zone = zones[i];
                if (zone == null)
                {
                    continue;
                }

                Color color = neutralColor;
                if (zone.IsContested)
                {
                    color = contestedColor;
                }
                else if (zone.currentOwner == CaptureZone.Owner.Player)
                {
                    color = playerColor;
                }
                else if (zone.currentOwner == CaptureZone.Owner.Enemy)
                {
                    color = enemyColor;
                }

                if (i < radialFills.Length && radialFills[i] != null)
                {
                    radialFills[i].fillAmount = zone.GetProgress();
                    radialFills[i].color = color;
                }

                if (i < rings.Length && rings[i] != null)
                {
                    rings[i].color = new Color(color.r, color.g, color.b, 0.28f);
                }

                if (i < labels.Length && labels[i] != null)
                {
                    labels[i].color = color;
                }
            }
        }
    }
}
