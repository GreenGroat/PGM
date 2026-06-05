using UnityEngine;

namespace Lab4Narrative
{
    public class FloatAndSpin : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.12f;
        [SerializeField] private float frequency = 1.4f;
        [SerializeField] private float rotationSpeed = 45f;

        private Vector3 startPosition;

        private void Awake()
        {
            startPosition = transform.localPosition;
        }

        private void Update()
        {
            float bob = Mathf.Sin(Time.time * frequency) * amplitude;
            transform.localPosition = startPosition + Vector3.up * bob;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
