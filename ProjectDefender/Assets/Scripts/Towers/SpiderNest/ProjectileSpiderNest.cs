using System;
using UnityEngine;
using UnityEngine.AI;

public class ProjectileSpiderNest : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private NavMeshAgent agent;
    
    private Enemy currentTarget;

    [Header("Base Stats")]
    [SerializeField] private float damage;
    [SerializeField] private float baseDamageRadius = .8f;
    [SerializeField] private float baseDetonateDistance = .5f;
    
    private float currentDamageRadius;
    private float currentDetonateDistance;

    [SerializeField] private GameObject explosionVfx;
    
    private AudioClip explosionSfx;
    private string explosionSfxId;
    private float explosionSfxCooldown;
    private int maxConcurrentExplosions;
    private bool limitExplosionSfx;
    private float explosionVolume;
    
    [Space]
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private LayerMask whatIsShield;
    [SerializeField] private float enemyCheckRadius = 10;
    [SerializeField] private float targetUpdateInterval = .5f;

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
        
        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, currentDetonateDistance + 0.1f, whatIsShield))
        {
            Explode();
            return;
        }

        if (agent.hasPath && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Explode();
        }
    }

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

    public void SetupSpider(float newDamage, AudioClip expSfx, string expSfxId, float expSfxCooldown, int maxConcurrent, bool limitSfx, float expVolume)
    {
        if (trail != null) trail.Clear();
    
        damage = newDamage;
    
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
                    continue;
                }
            
                damageable.TakeDamage(damage);
            }
        }
    }

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