using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public enum EnemyType { Basic, Fast, Swarm, Heavy, Stealth, Flying, BossSpider, None}

public class Enemy : MonoBehaviour , IDamageable
{
    public EnemyVisuals visuals { get; private set; }
    
    private GameManager gameManager;
    protected EnemyPortal myPortal;
    protected NavMeshAgent agent;
    protected Rigidbody rb;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centrePoint;
    public int healthPoints = 4;
    protected bool isDead;
    
    [Header("Movement")]
    [SerializeField] private float turnSpeed = 10;
    
    [SerializeField] protected List<Transform> myWaypoints;
    protected int nextWaypointIndex;
    protected int currentWaypointIndex;
    
    private float totalDistance;

    protected bool canBeHidden = true;
    protected bool isHidden;
    private Coroutine hideCo;
    private int originalLayerIndex;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);

        visuals = GetComponent<EnemyVisuals>();
        originalLayerIndex = gameObject.layer;
        
        gameManager = FindFirstObjectByType<GameManager>();
    }

    protected virtual void Start()
    {
        
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

    protected virtual void Update()
    {
        FaceTarget(agent.steeringTarget);
        
        // Check if the agent is close to current target point
        if (ShouldChangeWaypoint())
        {
            ChangeWaypoint();
        }
    }

    public void HideEnemy(float duration)
    {
        if (canBeHidden == false) return;
        
        if (hideCo != null) StopCoroutine(hideCo);

        hideCo = StartCoroutine(HideEnemyCo(duration));
    }

    protected virtual void ChangeWaypoint()
    {
        agent.SetDestination(GetNextWaypoint());
    }

    private IEnumerator HideEnemyCo(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Untargetable");
        visuals.MakeTransparent(true);
        isHidden = true;

        yield return new WaitForSeconds(duration);

        gameObject.layer = originalLayerIndex;
        visuals.MakeTransparent(false);
        isHidden = false;
    }

    protected virtual bool ShouldChangeWaypoint()
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

    protected Vector3 GetFinalWaypoint()
    {
        if (myWaypoints.Count == 0) return transform.position;

        return myWaypoints[myWaypoints.Count - 1].position;
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

        if (healthPoints <= 0 && isDead == false)
        {
            // isDead prevents Die() from being called twice.
            isDead = true;
            Die();
        }
    }

    public virtual void Die()
    {
        gameManager.UpdateCurrency(1);
        DestroyEnemy();
    }

    public void DestroyEnemy()
    {
        visuals.CreateOnDeathVfx();
        Destroy(gameObject);
        
        if (myPortal != null) myPortal.RemoveActiveEnemy(gameObject);
    }
}
