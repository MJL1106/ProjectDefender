using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBossUnit : Enemy
{
    private Vector3 savedDestination;
    private Vector3 lastKnownBossPosition;
    private EnemyFlyingBoss myBoss;
    
    // Invincibility handling
    private int originalLayer;
    private bool isInvincible = false;
    
    // You can adjust this in the inspector if needed
    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private string untargetableLayerName = "Untargetable"; // Or use layer index directly
    
    protected override void Awake()
    {
        base.Awake();
        originalLayer = gameObject.layer;
    }

    protected override void Update()
    {
        base.Update();

        if (myBoss != null)
            lastKnownBossPosition = myBoss.transform.position;
    }

    public void SetupEnemy(Vector3 destination, EnemyFlyingBoss myNewBoss, EnemyPortal myNewPortal)
    {
        ResetEnemy();
        ResetMovement();

        myBoss = myNewBoss;
        myPortal = myNewPortal;
        myPortal.GetActiveEnemies().Add(gameObject);

        savedDestination = destination;
        
        // Make unit invincible when spawned
        MakeInvincible();

        InvokeRepeating(nameof(SnapToBossIfNeeded), .1f, .5f);
    }

    private void ResetMovement()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        agent.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Enemy")
            return;

        rb.useGravity = false;
        rb.isKinematic = true;

        agent.enabled = true;
        agent.SetDestination(savedDestination);
        
        // Remove invincibility when landing (if still invincible)
        if (isInvincible)
        {
            RemoveInvincibility();
        }
    }
    
    private void MakeInvincible()
    {
        isInvincible = true;
        gameObject.layer = LayerMask.NameToLayer(untargetableLayerName);
        
        // Optional: Add visual feedback (transparency, outline, etc.)
        // You could modify the material or add a shader effect here
        
        // Fallback timer in case unit never lands properly
        Invoke(nameof(RemoveInvincibility), invincibilityDuration);
    }
    
    private void RemoveInvincibility()
    {
        if (!isInvincible) return; // Prevent double removal
        
        isInvincible = false;
        gameObject.layer = originalLayer;
        
        // Remove visual feedback if any
        
        CancelInvoke(nameof(RemoveInvincibility)); // Cancel the timer if it's still running
    }

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
    
    // Override TakeDamage if your Enemy base class has it
    public override void TakeDamage(float damage)
    {
        if (isInvincible) return;
        base.TakeDamage(damage);
    }

    public override float DistanceToFinishLine()
    {
        return Vector3.Distance(transform.position, GetFinalWaypoint());
    }
    
    // Clean up when disabled/destroyed
    protected override void OnDisable()
    {
        CancelInvoke(nameof(RemoveInvincibility));
        CancelInvoke(nameof(SnapToBossIfNeeded));
        
        // Reset to original layer in case object is pooled
        gameObject.layer = originalLayer;
        isInvincible = false;
        
        base.OnDisable();
    }
}