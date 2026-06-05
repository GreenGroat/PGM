using UnityEngine;
using UnityEngine.AI;

namespace Lab5Stealth
{
    [RequireComponent(typeof(EnemyVision))]
    public class EnemyStateMachine : MonoBehaviour
    {
        public enum State
        {
            Patrol,
            Suspicion,
            Alert
        }

        [Header("State")]
        public State currentState = State.Patrol;

        [Header("Movement")]
        public Transform[] waypoints;
        public float patrolSpeed = 2.1f;
        public float suspicionSpeed = 2.6f;
        public float alertSpeed = 3.5f;
        public float waypointReachDistance = 0.35f;
        public float lookAroundTime = 2.2f;
        public float losePlayerDelay = 3f;

        [Header("Hearing")]
        public float hearingRange = 8f;

        [Header("Visual State")]
        public Renderer bodyRenderer;
        public Material patrolMaterial;
        public Material suspicionMaterial;
        public Material alertMaterial;

        public State CurrentState => currentState;

        private NavMeshAgent agent;
        private EnemyVision vision;
        private Transform player;
        private Vector3 suspicionPoint;
        private float suspicionTimer;
        private float timeSinceSeen;
        private int waypointIndex;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            vision = GetComponent<EnemyVision>();
        }

        private void OnEnable()
        {
            PlayerStealth.NoiseEmitted += OnNoiseEmitted;
        }

        private void OnDisable()
        {
            PlayerStealth.NoiseEmitted -= OnNoiseEmitted;
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            ApplyStateVisuals();
        }

        private void Update()
        {
            switch (currentState)
            {
                case State.Patrol:
                    UpdatePatrol();
                    break;
                case State.Suspicion:
                    UpdateSuspicion();
                    break;
                case State.Alert:
                    UpdateAlert();
                    break;
            }
        }

        public void SetAlert(Vector3 lastKnownPosition)
        {
            suspicionPoint = lastKnownPosition;
            timeSinceSeen = 0f;
            ChangeState(State.Alert);
        }

        public void SetSuspicion(Vector3 point)
        {
            if (currentState == State.Alert)
            {
                return;
            }

            suspicionPoint = point;
            suspicionTimer = 0f;
            ChangeState(State.Suspicion);
        }

        private void UpdatePatrol()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                return;
            }

            Transform target = waypoints[waypointIndex];
            MoveTo(target.position, patrolSpeed);

            if (Vector3.Distance(transform.position, target.position) <= waypointReachDistance)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
            }
        }

        private void UpdateSuspicion()
        {
            MoveTo(suspicionPoint, suspicionSpeed);

            if (Vector3.Distance(transform.position, suspicionPoint) <= waypointReachDistance + 0.2f)
            {
                suspicionTimer += Time.deltaTime;
                transform.Rotate(Vector3.up, Mathf.Sin(Time.time * 6f) * 80f * Time.deltaTime);

                if (suspicionTimer >= lookAroundTime)
                {
                    ChangeState(State.Patrol);
                }
            }
        }

        private void UpdateAlert()
        {
            if (player == null)
            {
                ChangeState(State.Patrol);
                return;
            }

            if (vision != null && vision.CanSeePlayer)
            {
                suspicionPoint = player.position;
                timeSinceSeen = 0f;
            }
            else
            {
                timeSinceSeen += Time.deltaTime;
            }

            MoveTo(suspicionPoint, alertSpeed);

            if (timeSinceSeen >= losePlayerDelay)
            {
                SetSuspicion(suspicionPoint);
            }
        }

        private void MoveTo(Vector3 destination, float speed)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.speed = speed;
                agent.SetDestination(destination);
                return;
            }

            Vector3 flatDestination = new Vector3(destination.x, transform.position.y, destination.z);
            transform.position = Vector3.MoveTowards(transform.position, flatDestination, speed * Time.deltaTime);

            Vector3 direction = flatDestination - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
            }
        }

        private void OnNoiseEmitted(Vector3 position, float radius)
        {
            float effectiveRange = Mathf.Max(radius, 0.1f);
            if (Vector3.Distance(transform.position, position) <= Mathf.Min(effectiveRange, hearingRange))
            {
                SetSuspicion(position);
            }
        }

        private void ChangeState(State nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;
            if (currentState == State.Suspicion)
            {
                suspicionTimer = 0f;
            }

            ApplyStateVisuals();
        }

        private void ApplyStateVisuals()
        {
            if (bodyRenderer == null)
            {
                return;
            }

            Material material = patrolMaterial;
            if (currentState == State.Suspicion)
            {
                material = suspicionMaterial;
            }
            else if (currentState == State.Alert)
            {
                material = alertMaterial;
            }

            if (material != null)
            {
                bodyRenderer.material = material;
            }
        }
    }
}
