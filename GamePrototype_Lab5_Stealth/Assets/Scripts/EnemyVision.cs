using UnityEngine;

namespace Lab5Stealth
{
    [RequireComponent(typeof(EnemyStateMachine))]
    public class EnemyVision : MonoBehaviour
    {
        public Transform eyePoint;
        public float viewRadius = 9f;
        [Range(15f, 170f)] public float viewAngle = 84f;
        public float crouchVisibilityMultiplier = 0.65f;
        public LayerMask obstacleMask;
        public LayerMask playerMask;

        public bool CanSeePlayer { get; private set; }
        public Transform Player { get; private set; }

        private EnemyStateMachine stateMachine;
        private PlayerStealth playerStealth;

        private void Awake()
        {
            stateMachine = GetComponent<EnemyStateMachine>();
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Player = playerObject.transform;
                playerStealth = playerObject.GetComponent<PlayerStealth>();
            }
        }

        private void Update()
        {
            CanSeePlayer = CheckVisibility();
            if (CanSeePlayer && Player != null)
            {
                stateMachine.SetAlert(Player.position);
            }
        }

        public bool CheckVisibility()
        {
            if (Player == null)
            {
                return false;
            }

            Vector3 eye = GetEyePosition();
            Vector3 target = Player.position + Vector3.up * 0.85f;
            Vector3 direction = target - eye;
            float distance = direction.magnitude;
            float radius = viewRadius;

            if (playerStealth != null && playerStealth.IsCrouching)
            {
                radius *= crouchVisibilityMultiplier;
            }

            if (distance > radius)
            {
                return false;
            }

            if (Vector3.Angle(transform.forward, direction.normalized) > viewAngle * 0.5f)
            {
                return false;
            }

            if (Physics.Raycast(eye, direction.normalized, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Player") || hit.transform.IsChildOf(Player))
                {
                    return true;
                }

                if (hit.collider.CompareTag("Cover"))
                {
                    return false;
                }

                return (playerMask.value & (1 << hit.collider.gameObject.layer)) != 0;
            }

            return false;
        }

        public Vector3 GetEyePosition()
        {
            return eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.35f;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 eye = GetEyePosition();
            Gizmos.color = CanSeePlayer ? new Color(1f, 0.1f, 0.25f, 0.85f) : new Color(0.1f, 0.85f, 1f, 0.45f);
            Gizmos.DrawWireSphere(eye, viewRadius);

            Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(eye, eye + left * viewRadius);
            Gizmos.DrawLine(eye, eye + right * viewRadius);
        }
    }
}
