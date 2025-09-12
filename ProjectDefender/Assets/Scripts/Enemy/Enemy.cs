using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public enum EnemyType { Basic, Fast, Swarm, None}

public class Enemy : MonoBehaviour , IDamageable
{
    private GameManager gameManager;
    private EnemyPortal myPortal;
    private NavMeshAgent agent;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centrePoint;
    public int healthPoints = 4;
    
    [Header("Movement")]
    [SerializeField] private float turnSpeed = 10;
    
    [SerializeField] private List<Transform> myWaypoints;
    private int nextWaypointIndex;
    private int currentWaypointIndex;
    
    private float totalDistance;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);
    }

    public void SetupEnemy(List<Waypoint> newWaypoints, EnemyPortal myNewPortal)
    {
        myWaypoints = new List<Transform>();
        foreach (var point in newWaypoints)
        {
            myWaypoints.Add(point.transform);
        }
        
        CollectTotalDistance();

        myPortal = myNewPortal;
    }

    private void Update()
    {
        FaceTarget(agent.steeringTarget);
        
        // Check if the agent is close to current target point
        if (ShouldChangeWaypoint())
        {
            agent.SetDestination(GetNextWaypoint());
        }
    }

    private bool ShouldChangeWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Count) return false;

        if (agent.remainingDistance < .5f) return true;
        
        Vector3 currentWaypoint = myWaypoints[currentWaypointIndex].position;
        Vector3 nextWaypoint = myWaypoints[nextWaypointIndex].position;

        float distanceToNextWaypoint = Vector3.Distance(transform.position, nextWaypoint);
        float distanceBetweenPoints = Vector3.Distance(currentWaypoint, nextWaypoint);

        return distanceBetweenPoints > distanceToNextWaypoint;
    }

    public float DistanceToFinishLine()
    {
        return totalDistance + agent.remainingDistance;
    }
    
    private void CollectTotalDistance()
    {
        for (int i = 0; i < myWaypoints.Count - 1; i++)
        {
            float distance = Vector3.Distance(myWaypoints[i].position, myWaypoints[i + 1].position);
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
        if (nextWaypointIndex >= myWaypoints.Count) return transform.position;
        
        Vector3 targetPoint = myWaypoints[nextWaypointIndex].position;

        // Once the enemy is past the first waypoint, calculate the distance from the previous waypoint
        if (nextWaypointIndex > 0)
        {
            float distance = Vector3.Distance(myWaypoints[nextWaypointIndex].position, myWaypoints[nextWaypointIndex - 1].position);
            // Workout new total distance left to finish point
            totalDistance = totalDistance - distance;
        }
        
        nextWaypointIndex++;
        currentWaypointIndex = nextWaypointIndex - 1;

        return targetPoint;
    }

    public Vector3 CentrePoint()
    {
        return centrePoint.position;
    }

    public EnemyType GetEnemyType()
    {
        return enemyType;
    }

    public void TakeDamage(int damage)
    {
        healthPoints = healthPoints - damage;
        
        if (healthPoints <= 0) Die();
    }

    public void Die()
    {
        myPortal.RemoveActiveEnemy(gameObject);
        gameManager.UpdateCurrency(1);
        Destroy(gameObject);
    }

    public void DestroyEnemy()
    {
        myPortal.RemoveActiveEnemy(gameObject);
        Destroy(gameObject);
    }
}
