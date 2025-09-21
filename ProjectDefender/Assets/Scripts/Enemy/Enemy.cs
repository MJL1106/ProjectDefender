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

    protected ObjectPoolManager objectPool;
    protected GameManager gameManager;
    protected EnemyPortal myPortal;
    protected NavMeshAgent agent;
    protected Rigidbody rb;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centrePoint;
    public float maxHp = 100;
    public float currentHp = 4;
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
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);

        visuals = GetComponent<EnemyVisuals>();
        originalLayerIndex = gameObject.layer;
        
        gameManager = FindFirstObjectByType<GameManager>();
        originalSpeed = agent.speed;
        
        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void Start()
    {
        
    }

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
        ChangeWaypoint();
    }

    protected void ResetEnemy()
    {
        gameObject.layer = originalLayerIndex;
        
        visuals.MakeTransparent(false);

        currentHp = maxHp;
        isDead = false;

        agent.speed = originalSpeed;
        agent.enabled = true;
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

    public void DisableHide(float duration)
    {
        if (disableHideCo != null) StopCoroutine(disableHideCo);

        disableHideCo = StartCoroutine(DisableHideCo(duration));
    }

    protected virtual IEnumerator DisableHideCo(float duration)
    {
        canBeHidden = false;
        yield return new WaitForSeconds(duration);
        canBeHidden = true;
    }

    public void HideEnemy(float duration)
    {
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

    protected virtual void ChangeWaypoint()
    {
        agent.SetDestination(GetNextWaypoint());
    }


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

    public virtual float DistanceToFinishLine()
    {
        return totalDistance + agent.remainingDistance;
    }
    
    private void CollectTotalDistance()
    {
        for (int i = 0; i < myWaypoints.Length - 1; i++)
        {
            float distance = Vector3.Distance(myWaypoints[i], myWaypoints[i + 1]);
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
        if (nextWaypointIndex >= myWaypoints.Length) return transform.position;
        
        Vector3 targetPoint = myWaypoints[nextWaypointIndex];

        // Once the enemy is past the first waypoint, calculate the distance from the previous waypoint
        if (nextWaypointIndex > 0)
        {
            float distance = Vector3.Distance(myWaypoints[nextWaypointIndex], myWaypoints[nextWaypointIndex - 1]);
            // Workout new total distance left to finish point
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

    public Vector3 CentrePoint()
    {
        return centrePoint.position;
    }

    public EnemyType GetEnemyType()
    {
        return enemyType;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHp = currentHp - damage;

        if (currentHp <= 0 && isDead == false)
        {
            // isDead prevents Die() from being called twice.
            isDead = true;
            Die();
        }
    }

    public virtual void Die()
    {
        gameManager.UpdateCurrency(1);
        RemoveEnemy();
    }

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
