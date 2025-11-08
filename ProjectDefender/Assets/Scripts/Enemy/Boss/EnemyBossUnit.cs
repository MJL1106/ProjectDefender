using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ground unit spawned by EnemyFlyingBoss that drops from the sky.
/// Temporarily invincible during fall until landing, then navigates normally.
/// Snaps to boss position if knocked off NavMesh.
/// </summary>
public class EnemyBossUnit : Enemy
{
    private Vector3 savedDestination;
    private Vector3 lastKnownBossPosition;
    private EnemyFlyingBoss myBoss;
    
    private int originalLayer;
    private bool isInvincible = false;
    
    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private string untargetableLayerName = "Untargetable";
    
    protected override void Awake()
    {
        base.Awake();
        originalLayer = gameObject.layer;
    }

    protected override void Update()
    {
        base.Update();

        if (myBoss != null) lastKnownBossPosition = myBoss.transform.position;
    }

    /// <summary>
    /// Initializes unit after being spawned by boss.
    /// Applies temporary invincibility and physics-based falling.
    /// </summary>
    public void SetupEnemy(Vector3 destination, EnemyFlyingBoss myNewBoss, EnemyPortal myNewPortal)
    {
        ResetEnemy();
        ResetMovement();

        myBoss = myNewBoss;
        myPortal = myNewPortal;
        myPortal.GetActiveEnemies().Add(gameObject);

        savedDestination = destination;
        
        MakeInvincible();

        InvokeRepeating(nameof(SnapToBossIfNeeded), .1f, .5f);
    }

    /// <summary>
    /// Enables physics for falling behavior before landing.
    /// </summary>
    private void ResetMovement()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        agent.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Enemy") return;

        // Switch to NavMesh navigation after landing
        rb.useGravity = false;
        rb.isKinematic = true;

        agent.enabled = true;
        agent.SetDestination(savedDestination);
        
        if (isInvincible) RemoveInvincibility();
    }
    
    /// <summary>
    /// Makes unit invincible and untargetable by changing layer.
    /// Automatically times out as fallback if unit never lands properly.
    /// </summary>
    private void MakeInvincible()
    {
        isInvincible = true;
        gameObject.layer = LayerMask.NameToLayer(untargetableLayerName);
        
        Invoke(nameof(RemoveInvincibility), invincibilityDuration);
    }
    
    private void RemoveInvincibility()
    {
        if (!isInvincible) return;
        
        isInvincible = false;
        gameObject.layer = originalLayer;
        
        CancelInvoke(nameof(RemoveInvincibility));
    }

    /// <summary>
    /// Safety check to prevent units from getting stuck off NavMesh.
    /// Teleports unit back to boss if too far and not on NavMesh.
    /// </summary>
    private void SnapToBossIfNeeded()
    {
        if (agent.enabled && agent.isOnNavMesh == false)
        {
            if (Vector3.Distance(transform.position, lastKnownBossPosition) > 3f)
            {
                transform.position = lastKnownBossPosition + new Vector3(0, -1, 0);
                ResetMovement();
            }
        }
    }
    
    public override void TakeDamage(float damage)
    {
        if (isInvincible) return;
        base.TakeDamage(damage);
    }

    public override float DistanceToFinishLine()
    {
        return Vector3.Distance(transform.position, GetFinalWaypoint());
    }
    
    protected override void OnDisable()
    {
        CancelInvoke(nameof(RemoveInvincibility));
        CancelInvoke(nameof(SnapToBossIfNeeded));
        
        // Reset to original layer for object pooling
        gameObject.layer = originalLayer;
        isInvincible = false;
        
        base.OnDisable();
    }
}