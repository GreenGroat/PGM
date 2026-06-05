using UnityEngine;

namespace Lab4Narrative
{
    public static class GameUIState
    {
        private static int overlayCount;

        public static bool ControlsLocked => overlayCount > 0;

        public static void PushOverlay()
        {
            overlayCount++;
            ApplyState();
        }

        public static void PopOverlay()
        {
            overlayCount = Mathf.Max(overlayCount - 1, 0);
            ApplyState();
        }

        public static void Reset()
        {
            overlayCount = 0;
            ApplyState();
        }

        private static void ApplyState()
        {
            bool locked = overlayCount > 0;
            Time.timeScale = locked ? 0f : 1f;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }
    }
}
