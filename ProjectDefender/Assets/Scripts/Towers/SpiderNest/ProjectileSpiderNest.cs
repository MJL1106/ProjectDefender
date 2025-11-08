using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A small, ground-based unit that seeks the closest enemy and explodes.
/// Spawned by the TowerSpiderNest.
/// </summary>
public class ProjectileSpiderNest : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private NavMeshAgent agent;
    
    private Enemy currentTarget;

    [Header("Base Stats")]
    [SerializeField] private float damage;
    [SerializeField] private float baseDamageRadius = .8f; // Default AOE damage radius
    [SerializeField] private float baseDetonateDistance = .5f; // Default proximity to detonate
    
    private float currentDamageRadius;
    private float currentDetonateDistance;

    [SerializeField] private GameObject explosionVfx;
    
    // Cached audio settings
    private AudioClip explosionSfx;
    private string explosionSfxId;
    private float explosionSfxCooldown;
    private int maxConcurrentExplosions;
    private bool limitExplosionSfx;
    private float explosionVolume;
    
    [Space]
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private LayerMask whatIsShield; // Layer for enemy shields (to detonate early)
    [SerializeField] private float enemyCheckRadius = 10; // How far the spider can "see" to find a new target
    [SerializeField] private float targetUpdateInterval = .5f; // How often to re-scan for the closest enemy

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        agent = GetComponent <NavMeshAgent>();
        objectPool = ObjectPoolManager.instance;
        
        currentDamageRadius = baseDamageRadius;
        currentDetonateDistance = baseDetonateDistance;
        
        InvokeRepeating(nameof(UpdateClosestTarget), .1f, targetUpdateInterval);
    }

    private void Update()
    {
        if (currentTarget == null || agent.enabled == false || agent.isOnNavMesh == false) return;
    
        agent.SetDestination(currentTarget.transform.position);

        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        
        // Detonate early if a shield is in the way
        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, currentDetonateDistance + 0.1f, whatIsShield))
        {
            Explode();
            return;
        }

        // Detonate upon reaching the target
        if (agent.hasPath && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) Explode();
    }

    /// <summary>
    /// Damages enemies, plays VFX/SFX, and returns to the pool.
    /// </summary>
    private void Explode()
    {
        DamageEnemiesAround();
    
        if (explosionSfx != null)
        {
            if (limitExplosionSfx && !string.IsNullOrEmpty(explosionSfxId))
            {
                AudioManager.instance?.PlaySFXOneShotLimited(
                    explosionSfx, 
                    transform.position, 
                    explosionSfxId, 
                    explosionSfxCooldown, 
                    maxConcurrentExplosions, 
                    true, 
                    explosionVolume
                );
            }
            else
            {
                AudioManager.instance?.PlaySFXOneShot(explosionSfx, transform.position, true, explosionVolume);
            }
        }

        objectPool.Get(explosionVfx, transform.position + new Vector3(0, .4f, 0));
    
        objectPool.Remove(gameObject);
    }

    /// <summary>
    /// Initializes the unit when it's launched from the nest.
    /// </summary>
    /// <param name="newDamage">The damage to apply on explosion.</param>
    /// <param name="expSfx">The AudioClip for the explosion sound.</param>
    /// <param name="expSfxId">The unique ID for sound limiting.</param>
    /// <param name="expSfxCooldown">The cooldown for the sound.</param>
    /// <param name="maxConcurrent">The max concurrent instances of this sound.</param>
    /// <param name="limitSfx">Whether to apply sound limiting.</param>
    /// <param name="expVolume">The volume for the explosion sound.</param>
    public void SetupSpider(float newDamage, AudioClip expSfx, string expSfxId, float expSfxCooldown, int maxConcurrent, bool limitSfx, float expVolume)
    {
        if (trail != null) trail.Clear();
    
        damage = newDamage;
    
        // Cache audio settings
        explosionSfx = expSfx;
        explosionSfxId = expSfxId;
        explosionSfxCooldown = expSfxCooldown;
        maxConcurrentExplosions = maxConcurrent;
        limitExplosionSfx = limitSfx;
        explosionVolume = expVolume;

        Collider spiderCollider = GetComponent<Collider>();
        if (spiderCollider != null) spiderCollider.enabled = true;

        agent.enabled = true;
        agent.isStopped = false;
        
        agent.stoppingDistance = currentDetonateDistance;
        
        transform.parent = null;
    }
    
    /// <summary>
    /// Applies AOE damage, skipping hidden enemies.
    /// </summary>
    private void DamageEnemiesAround()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, currentDamageRadius, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
        
            if (damageable == null)
            {
                damageable = enemy.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                Enemy enemyComponent = damageable as Enemy;
                if (enemyComponent != null && enemyComponent.IsHidden())
                {
                    continue; // Skip hidden enemies
                }
            
                damageable.TakeDamage(damage);
            }
        }
    }

    /// <summary>
    /// Finds and sets the closest non-hidden enemy as the target.
    /// </summary>
    private void UpdateClosestTarget()
    {
        Enemy newTarget = FindClosestEnemy();

        if (newTarget == currentTarget) return;

        currentTarget = newTarget;

        if (currentTarget != null)
        {
            EnemyType type = currentTarget.GetEnemyType();
            currentDetonateDistance = GetDetonateDistanceForType(type);
            currentDamageRadius = GetDamageRadiusForType(type);
        }
        else
        {
            currentDetonateDistance = baseDetonateDistance;
            currentDamageRadius = baseDamageRadius;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.stoppingDistance = currentDetonateDistance;
        }
    }

    /// <summary>
    /// Scans in a radius for the nearest non-hidden enemy.
    /// </summary>
    private Enemy FindClosestEnemy()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, enemyCheckRadius, whatIsEnemy);
        
        Enemy nearestEnemy = null; 
        float shortestDistance = float.MaxValue;

        foreach (Collider enemyCollider in enemiesAround)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy == null) enemy = enemyCollider.GetComponentInParent<Enemy>();
            
            if (enemy != null && enemy.IsHidden())
            {
                continue;
            }
            
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);

            if (distance < shortestDistance)
            {
                nearestEnemy = enemy; 
                shortestDistance = distance;
            }
        }

        return nearestEnemy;
    }

    /// <summary>
    /// Returns a larger detonate distance for large enemies.
    /// </summary>
    private float GetDetonateDistanceForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Heavy:
            case EnemyType.BossSpider:
                return 1.1f; 
        
            default:
                return baseDetonateDistance;
        }
    }
    
    /// <summary>
    /// Returns a larger damage radius for large enemies.
    /// </summary>
    private float GetDamageRadiusForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Heavy:
            case EnemyType.BossSpider:
                return 1.3f;
            
            default:
                return baseDamageRadius;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, currentDamageRadius);
    }
}