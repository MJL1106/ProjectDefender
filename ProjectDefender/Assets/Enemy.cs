using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private float turnSpeed = 10;
    [FormerlySerializedAs("waypoint")] [SerializeField] private Transform[] waypoints;
    private int waypointIndex;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);
    }

    private void Start()
    {
        waypoints = FindFirstObjectByType<WaypointManager>().GetWaypoints();
    }

    private void Update()
    {
        FaceTarget(agent.steeringTarget);
        
        // Check if the agent is close to current target point
        if (agent.remainingDistance < .5f)
        {
            agent.SetDestination(GetNextWaypoint());
        }
    }

    private void FaceTarget(Vector3 newTarget)
    {
        Vector3 directionToTarget = newTarget - transform.position;
        if (directionToTarget.magnitude == 0) return;
        directionToTarget.y = 0;

        // Create a rotation that points the forward vector to the calculated direction
        Quaternion newRotation = Quaternion.LookRotation(directionToTarget);

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, turnSpeed * Time.deltaTime);
    }

    private Vector3 GetNextWaypoint()
    {
        if (waypointIndex >= waypoints.Length) return transform.position;
        
        Vector3 targetPoint = waypoints[waypointIndex].position;
        waypointIndex++;

        return targetPoint;
    }
}
