using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (uiManager == null)
        {
            uiManager = Object.FindFirstObjectByType<UIManager>();
        }

        uiManager?.SetGoalReached();
        Debug.Log("Goal zone reached.");
    }
}
