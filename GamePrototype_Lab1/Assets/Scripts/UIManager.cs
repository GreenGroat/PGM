using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text positionText;
    [SerializeField] private Text statusText;

    [Header("Scene References")]
    [SerializeField] private Transform player;
    [SerializeField] private int targetScore = 5;

    private int currentScore;

    private void Start()
    {
        UpdateScoreUI();
        SetStatus("Collect all coins and reach the green finish zone.");
    }

    private void Update()
    {
        if (player == null || positionText == null)
        {
            return;
        }

        Vector3 pos = player.position;
        positionText.text = $"Position: X {pos.x:F1} / Y {pos.y:F1} / Z {pos.z:F1}";
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();

        if (currentScore >= targetScore)
        {
            SetStatus("All coins collected. Go to the finish zone.");
        }
        else
        {
            SetStatus("Coin collected.");
        }
    }

    public void SetGoalReached()
    {
        if (currentScore >= targetScore)
        {
            SetStatus("Prototype complete: movement, jump, pickups and goal work.");
        }
        else
        {
            SetStatus("Finish found. Collect more coins first.");
        }
    }

    public void ShowObstacleMessage()
    {
        SetStatus("Obstacle collision detected.");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Coins: {currentScore}/{targetScore}";
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
