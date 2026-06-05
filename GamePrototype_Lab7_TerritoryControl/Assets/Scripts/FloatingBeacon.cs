using UnityEngine;

namespace Lab7Territory
{
    public class FloatingBeacon : MonoBehaviour
    {
        public float bobHeight = 0.2f;
        public float bobSpeed = 2f;
        public float rotationSpeed = 45f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.localPosition;
        }

        private void Update()
        {
            transform.localPosition = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
