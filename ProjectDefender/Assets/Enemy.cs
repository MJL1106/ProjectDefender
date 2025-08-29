using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    
    [SerializeField] private Transform[] waypoint;
    private int waypointIndex;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Check if the agent is close to current target point
        if (agent.remainingDistance < .5f)
        {
            agent.SetDestination(GetNextWaypoint());
        }
    }

    private Vector3 GetNextWaypoint()
    {
        if (waypointIndex >= waypoint.Length) return transform.position;
        
        Vector3 targetPoint = waypoint[waypointIndex].position;
        waypointIndex++;

        return targetPoint;
    }
}
