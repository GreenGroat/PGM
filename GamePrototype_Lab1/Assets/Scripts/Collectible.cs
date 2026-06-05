using UnityEngine;

public enum CollectibleKind
{
    Coin,
    SpeedBoost
}

public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleKind kind = CollectibleKind.Coin;
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private float boostMultiplier = 1.6f;
    [SerializeField] private float boostDuration = 4f;
    [SerializeField] private float rotationSpeed = 95f;

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

        UIManager ui = Object.FindFirstObjectByType<UIManager>();

        if (kind == CollectibleKind.Coin)
        {
            ui?.AddScore(scoreValue);
            Debug.Log("Coin collected.");
        }
        else
        {
            Mover mover = other.GetComponent<Mover>();
            mover?.BoostSpeed(boostMultiplier, boostDuration);
            Debug.Log("Speed boost collected.");
        }

        Destroy(gameObject);
    }
}
