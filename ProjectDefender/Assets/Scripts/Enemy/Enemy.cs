using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour , IDamageable
{
    private NavMeshAgent agent;

    [SerializeField] private Transform centrePoint;
    public int healthPoints = 4;
    
    [Header("Movement")]
    [SerializeField] private float turnSpeed = 10;
    [SerializeField] private Transform[] waypoints;
    private int waypointIndex;
    
    private float totalDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);
    }

    private void Start()
    {
        waypoints = FindFirstObjectByType<WaypointManager>().GetWaypoints();

        CollectTotalDistance();
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

    public float DistanceToFinishLine()
    {
        return totalDistance + agent.remainingDistance;
    }
    
    private void CollectTotalDistance()
    {
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            float distance = Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            totalDistance = totalDistance + distance;
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

        // Once the enemy is past the first waypoint, calculate the distance from the previous waypoint
        if (waypointIndex > 0)
        {
            float distance = Vector3.Distance(waypoints[waypointIndex].position, waypoints[waypointIndex - 1].position);
            // Workout new total distance left to finish point
            totalDistance = totalDistance - distance;
        }
        
        waypointIndex++;

        return targetPoint;
    }

    public Vector3 CentrePoint()
    {
        return centrePoint.position;
    }

    public void TakeDamage(int damage)
    {
        healthPoints = healthPoints - damage;
        
        if (healthPoints <= 0) Destroy(gameObject);
    }
}
