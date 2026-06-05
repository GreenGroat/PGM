using UnityEngine;
using UnityEngine.AI;

namespace Lab7Territory
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public CaptureZone[] allZones;
        public float chooseInterval = 3f;
        public float fallbackMoveSpeed = 4.5f;
        public float reachDistance = 1.4f;

        private NavMeshAgent agent;
        private CaptureZone targetZone;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (agent != null && !agent.isOnNavMesh)
            {
                agent.enabled = false;
            }

            if (allZones == null || allZones.Length == 0)
            {
                allZones = FindObjectsOfType<CaptureZone>();
            }

            InvokeRepeating(nameof(ChooseTarget), 0.2f, chooseInterval);
        }

        private void Update()
        {
            if (targetZone == null)
            {
                return;
            }

            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                return;
            }

            Vector3 destination = targetZone.transform.position;
            destination.y = transform.position.y;
            Vector3 direction = destination - transform.position;

            if (direction.magnitude > reachDistance)
            {
                transform.position += direction.normalized * fallbackMoveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
            }
        }

        private void ChooseTarget()
        {
            CaptureZone best = null;
            float bestScore = float.NegativeInfinity;

            foreach (CaptureZone zone in allZones)
            {
                if (zone == null)
                {
                    continue;
                }

                float score = 0f;
                if (zone.currentOwner == CaptureZone.Owner.Player)
                {
                    score = 4f;
                }
                else if (zone.currentOwner == CaptureZone.Owner.Neutral)
                {
                    score = 2.5f;
                }
                else
                {
                    score = 0.4f;
                }

                score -= Vector3.Distance(transform.position, zone.transform.position) * 0.035f;
                score += Random.Range(0f, 0.35f);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = zone;
                }
            }

            if (best == null)
            {
                return;
            }

            targetZone = best;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(targetZone.transform.position);
            }
        }
    }
}
