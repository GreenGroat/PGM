using UnityEngine;

namespace Lab6Puzzle
{
    public class FloatAndSpin : MonoBehaviour
    {
        public float bobHeight = 0.12f;
        public float bobSpeed = 2f;
        public float rotationSpeed = 35f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
