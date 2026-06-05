using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector3 MoveDirection { get; private set; }

    private bool jumpQueued;

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        MoveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
        }
    }

    public bool ConsumeJumpPressed()
    {
        if (!jumpQueued)
        {
            return false;
        }

        jumpQueued = false;
        return true;
    }
}
