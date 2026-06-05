using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class Mover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float turnSpeed = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.28f;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private Coroutine speedBoostRoutine;
    private float speedMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (groundCheck == null)
        {
            groundCheck = transform;
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = playerInput.MoveDirection;
        Vector3 movement = moveDirection * (moveSpeed * speedMultiplier * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + movement);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            Quaternion nextRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(nextRotation);
        }

        if (playerInput.ConsumeJumpPressed() && IsGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void BoostSpeed(float multiplier, float duration)
    {
        if (speedBoostRoutine != null)
        {
            StopCoroutine(speedBoostRoutine);
        }

        speedBoostRoutine = StartCoroutine(BoostSpeedRoutine(multiplier, duration));
    }

    private IEnumerator BoostSpeedRoutine(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
        speedBoostRoutine = null;
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore);
    }
}
