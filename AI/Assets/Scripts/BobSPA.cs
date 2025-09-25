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
        if (isChasingBack)
        {
            player = player.Find("PlayerFront");
            isChasingBack = false;
        }
        else
        {
            player = player.Find("PlayerBack");
            isChasingBack = true;
        }
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Ray ray = new Ray(transform.position + (Vector3.up * 0.8f), player.position - transform.position);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            LOS = hit.collider.gameObject.TryGetComponent<PlayerMovement>(out PlayerMovement _);
        }

        if (Vector3.Distance(patrolPoints[patrolIndex].position, transform.position) < distanceCheck)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
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
                Chase();
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

    private void Patrol()
    {
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }
}
