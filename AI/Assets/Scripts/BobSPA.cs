using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class BobSPA : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints = new Transform[4];
    public float fleeDistance = 4f;
    public float distanceCheck = 1f;

    private int health = 50;
    private float distanceToPlayer;
    private bool LOS = false;
    private int patrolIndex = 0;
    private NavMeshAgent agent;
    private bool isChasingBack;

    private Dictionary<string, float> actionScores;

    public GameObject enemyInst;

    private static BobSPA leaderEnemy;

    private void Start()
    {
        actionScores = new Dictionary<string, float>()
        {
            { "Flee", 0f },
            { "Chase", 0f },
            { "Patrol", 0f }
        };

        gameObject.TryGetComponent(out agent);
    }

    private void Update()
    {
        // SENSE
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Reset detection each frame
        LOS = false;
        enemyInst = null;

        // Detection radius
        float detectionRadius = 10f;

        // Find everything in range
        Collider[] detected = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (var col in detected)
        {
            // Detect player
            if (col.TryGetComponent<PlayerMovement>(out PlayerMovement _))
            {
                Vector3 dirToPlayer = (col.transform.position - transform.position).normalized;
                Vector3 origin = transform.position + Vector3.up * 0.8f;

                // Check LOS with a raycast
                if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, detectionRadius))
                {
                    if (hit.collider.gameObject == col.gameObject)
                    {
                        // Clear LOS to player
                        LOS = true;
                        distanceToPlayer = Vector3.Distance(transform.position, col.transform.position);
                        Debug.DrawRay(origin, dirToPlayer * detectionRadius, Color.green);
                    }
                    else if (hit.collider.TryGetComponent<BobSPA>(out BobSPA _))
                    {
                        // LOS is hitting another enemy first
                        enemyInst = hit.collider.gameObject;
                        Debug.DrawRay(origin, dirToPlayer * detectionRadius, Color.blue);
                    }
                    else
                    {
                        // Blocked by something else
                        Debug.DrawRay(origin, dirToPlayer * detectionRadius, Color.red);
                    }
                }
            }

            // Detect other enemies (proximity only, not necessarily LOS)
            else if (col.TryGetComponent<BobSPA>(out BobSPA _))
            {
                if (col.gameObject != gameObject)
                {
                    enemyInst = col.gameObject;
                }
            }
        }

        if (Vector3.Distance(patrolPoints[patrolIndex].position, transform.position) < distanceCheck)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        // SELECT LEADER
        if (LOS)
        {
            if (leaderEnemy == null) // No leader yet
                leaderEnemy = this;

            // If current leader lost LOS, allow reassignment
            if (leaderEnemy != null && leaderEnemy != this && !leaderEnemy.LOS)
                leaderEnemy = this;
        }

        // PLAN
        actionScores["Flee"] = (distanceToPlayer < fleeDistance ? 10f : 0) + (health < (health * 0.5f) ? 5f : 0) * (LOS == true ? 1 : 0);
        actionScores["Chase"] = (distanceCheck >= fleeDistance ? 8f : 0f) + (health > (health * 0.5f) ? 5f : 0) * (LOS == true ? 1 : 0);
        actionScores["Patrol"] = 3f;

        string chosenAction = actionScores.Aggregate((l,r) => l.Value > r.Value ? l : r).Key;
        switch (chosenAction)
        {
            // ACT
            case "Flee":
                Flee();
                break;

            case "Chase":
                if (leaderEnemy == this) Ambush();
                else Chase();
                break;

            case "Patrol":
                Patrol();
                break;

            default:
                break;
        }


    }

    private void Flee()
    {
        Vector3 dir = (transform.position - player.position).normalized * 2;
        Vector3 fleePos = transform.position + dir * 5f;
        agent.SetDestination(fleePos);
    }

    private void Chase()
    {
        agent.SetDestination(player.position);
    }

    private void Ambush()
    {
        Vector3 ambushPoint = (player.position + player.forward * 5f) + (player.position + player.right * 1.5f);
        agent.SetDestination(ambushPoint);
    }

    private void Patrol()
    {
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    private void OnDrawGizmosSelected()
    {
        float detectionRadius = 10f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}