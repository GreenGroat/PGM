using UnityEngine;

public class ObstacleReporter : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
        {
            return;
        }

        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        ui?.ShowObstacleMessage();
        Debug.Log("Player touched an obstacle.");
    }
}
