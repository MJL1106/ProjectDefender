using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public enum EnemyType { Basic, Fast, Swarm, Heavy, Stealth, Flying, BossSpider, None}

/// <summary>
/// Base enemy class handling navigation, health, damage, and mechanics.
/// Uses NavMesh for pathfinding through waypoint system.
/// Supports object pooling, slowing effects, and temporary invisibility.
/// </summary>
public class Enemy : MonoBehaviour , IDamageable
{
    public EnemyVisuals visuals { get; private set; }

    protected ObjectPoolManager objectPool;
    protected GameManager gameManager;
    protected EnemyPortal myPortal;
    protected NavMeshAgent agent;
    protected Rigidbody rb;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centrePoint; // Center point for targeting
    
    [Header("Stats")]
    [SerializeField] private int reward = 10; // Currency given on death
    [SerializeField] private int castleDamage = 1; // Damage dealt to castle on reach
    public float maxHp = 100;
    protected float currentHp = 4;
    
    protected bool isDead;
    
    [Header("Movement")]
    [SerializeField] private float turnSpeed = 10;
    
    [SerializeField] protected Vector3[] myWaypoints;
    protected int nextWaypointIndex;
    protected int currentWaypointIndex;
    
    protected float totalDistance;
    protected float originalSpeed;
    
    protected bool canBeHidden = true;
    protected bool isHidden;
    private Coroutine hideCo;
    private Coroutine disableHideCo;
    private int originalLayerIndex;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10); // Faster enemies have priority

        visuals = GetComponent<EnemyVisuals>();
        originalLayerIndex = gameObject.layer;
        
        gameManager = FindFirstObjectByType<GameManager>();
        originalSpeed = agent.speed;
        
        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void Start()
    {
        
    }
    
    protected virtual void Update()
    {
        FaceTarget(agent.steeringTarget);
        
        if (ShouldChangeWaypoint()) ChangeWaypoint();
    }

    /// <summary>
    /// Initializes enemy with portal and waypoint data.
    /// Called when spawned from portal.
    /// </summary>
    public void SetupEnemy(EnemyPortal myNewPortal)
    {
        myPortal = myNewPortal;
        
        UpdateWaypoints(myPortal.currentWaypoints);
        CollectTotalDistance();
        ResetEnemy();
        BeginMovement();
    }

    private void UpdateWaypoints(Vector3[] newWaypoints)
    {
        myWaypoints = new Vector3[newWaypoints.Length];

        for (int i = 0; i < myWaypoints.Length; i++)
        {
            myWaypoints[i] = newWaypoints[i];
        }
    }

    private void BeginMovement()
    {
        currentWaypointIndex = 0;
        nextWaypointIndex = 0;
        totalDistance = 0;
        ChangeWaypoint();
    }

    /// <summary>
    /// Resets enemy state when retrieved from object pool.
    /// Restores health, speed, visibility, and NavMesh agent.
    /// </summary>
    protected void ResetEnemy()
    {
        gameObject.layer = originalLayerIndex;
        
        visuals.MakeTransparent(false);

        currentHp = maxHp;
        isDead = false;

        agent.speed = originalSpeed;
        agent.enabled = true;

        enabled = true;
    }

    /// <summary>
    /// Temporarily reduces enemy movement speed.
    /// Used by slow towers and ice effects.
    /// </summary>
    public void SlowEnemy(float slowMultiplier, float duration)
    {
        StartCoroutine(SlowEnemyCo(slowMultiplier, duration));
    }
    
    private IEnumerator SlowEnemyCo(float slowMultiplier, float duration)
    {
        agent.speed = originalSpeed;
        agent.speed = agent.speed * slowMultiplier;

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
    }

    /// <summary>
    /// Temporarily disables enemy's ability to be hidden by stealth enemies.
    /// Used by reveal towers like TowerFan.
    /// </summary>
    public void DisableHide(float duration)
    {
        if (isDead) return;
        
        if (disableHideCo != null) StopCoroutine(disableHideCo);

        disableHideCo = StartCoroutine(DisableHideCo(duration));
    }

    protected virtual IEnumerator DisableHideCo(float duration)
    {
        canBeHidden = false;
        yield return new WaitForSeconds(duration);
        canBeHidden = true;
    }

    /// <summary>
    /// Makes enemy invisible and untargetable temporarily.
    /// Applied by nearby stealth enemies.
    /// </summary>
    public void HideEnemy(float duration)
    {
        if (isDead) return;
        if (!gameObject.activeInHierarchy) return;
        if (canBeHidden == false) return;
    
        if (hideCo != null) StopCoroutine(hideCo);

        hideCo = StartCoroutine(HideEnemyCo(duration));
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
    
    public bool IsHidden() => isHidden;

    protected virtual void ChangeWaypoint()
    {
        agent.SetDestination(GetNextWaypoint());
    }

    /// <summary>
    /// Determines if enemy should advance to next waypoint.
    /// Checks both remaining distance and relative positioning.
    /// </summary>
    protected virtual bool ShouldChangeWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Length) return false;

        if (agent.remainingDistance < .5f) return true;
        
        Vector3 currentWaypoint = myWaypoints[currentWaypointIndex];
        Vector3 nextWaypoint = myWaypoints[nextWaypointIndex];

        float distanceToNextWaypoint = Vector3.Distance(transform.position, nextWaypoint);
        float distanceBetweenPoints = Vector3.Distance(currentWaypoint, nextWaypoint);

        return distanceBetweenPoints > distanceToNextWaypoint;
    }

    /// <summary>
    /// Calculates total remaining path distance to castle.
    /// Used for enemy threat prioritization in tower targeting.
    /// </summary>
    public virtual float DistanceToFinishLine()
    {
        if (myWaypoints == null || currentWaypointIndex >= myWaypoints.Length)
        {
            return 0f;
        }

        float remainingDistance = 0f;

        // Distance from current position to current waypoint
        remainingDistance += Vector3.Distance(transform.position, myWaypoints[currentWaypointIndex]);

        // Add distances between remaining waypoints
        for (int i = currentWaypointIndex; i < myWaypoints.Length - 1; i++)
        {
            remainingDistance += Vector3.Distance(myWaypoints[i], myWaypoints[i + 1]);
        }

        return remainingDistance;
    }
    
    /// <summary>
    /// Calculates total path length from start to castle.
    /// </summary>
    private void CollectTotalDistance()
    {
        for (int i = 0; i < myWaypoints.Length - 1; i++)
        {
            float distance = Vector3.Distance(myWaypoints[i], myWaypoints[i + 1]);
            totalDistance = totalDistance + distance;
        }
    }
    
    /// <summary>
    /// Smoothly rotates enemy to face movement direction.
    /// Ignores Y-axis to prevent tilting.
    /// </summary>
    private void FaceTarget(Vector3 newTarget)
    {
        Vector3 directionToTarget = newTarget - transform.position;
        if (directionToTarget.magnitude == 0) return;
        directionToTarget.y = 0;

        Quaternion newRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Advances to next waypoint and updates remaining distance tracking.
    /// </summary>
    private Vector3 GetNextWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Length) return transform.position;
        
        Vector3 targetPoint = myWaypoints[nextWaypointIndex];

        // Update total remaining distance
        if (nextWaypointIndex > 0)
        {
            float distance = Vector3.Distance(myWaypoints[nextWaypointIndex], myWaypoints[nextWaypointIndex - 1]);
            totalDistance = totalDistance - distance;
        }
        
        nextWaypointIndex++;
        currentWaypointIndex = nextWaypointIndex - 1;

        return targetPoint;
    }

    protected Vector3 GetFinalWaypoint()
    {
        if (myWaypoints.Length == 0) return transform.position;
        return myWaypoints[myWaypoints.Length - 1];
    }

    public Vector3 CentrePoint() => centrePoint.position;
    public EnemyType GetEnemyType() => enemyType;
    public int GetCastleDamage() => castleDamage;

    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0 && isDead == false)
        {
            isDead = true; // Prevents multiple death calls
            Die();
        }
    }

    public virtual void Die()
    {
        gameManager.UpdateCurrency(reward);
        gameManager.UpdateEnemiesKilled();
        RemoveEnemy();
    }

    public bool IsDead() => isDead;

    /// <summary>
    /// Returns enemy to object pool and notifies portal of removal.
    /// Creates death VFX before cleanup.
    /// </summary>
    public virtual void RemoveEnemy()
    {
        visuals.CreateOnDeathVfx();
        objectPool.Remove(gameObject);
        agent.enabled = false;
        
        if (myPortal != null) myPortal.RemoveActiveEnemy(gameObject);
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        CancelInvoke();
    }
}