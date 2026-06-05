using UnityEngine;

namespace Lab6Puzzle
{
    public class Door : MonoBehaviour
    {
        public Vector3 openPosition;
        public Vector3 closedPosition;
        public float speed = 2.8f;

        public bool IsOpen { get; private set; }

        private void Start()
        {
            closedPosition = transform.position;
        }

        private void Update()
        {
            Vector3 target = IsOpen ? openPosition : closedPosition;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }
    }
}
