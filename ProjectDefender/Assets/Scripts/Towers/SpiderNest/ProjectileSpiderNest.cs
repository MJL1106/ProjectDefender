using System;
using UnityEngine;
using UnityEngine.AI;

public class ProjectileSpiderNest : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform currentTarget;

    [SerializeField] private float damage;
    [SerializeField] private float damageRadius = .8f;
    [SerializeField] private float detonateDistance = .5f;
    [SerializeField] private GameObject explosionVfx;
    [Space]
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private float enemyCheckRadius = 10;
    [SerializeField] private float targetUpdateInterval = .5f;

    private void Awake()
    {
        agent = GetComponent <NavMeshAgent>();
        
        InvokeRepeating(nameof(UpdateClosestTarget), .1f, targetUpdateInterval);
    }

    private void Update()
    {
        if (currentTarget == null || agent.enabled == false || agent.isOnNavMesh == false) return;
    
        agent.SetDestination(currentTarget.position);

        if (Vector3.Distance(transform.position, currentTarget.position) < detonateDistance) Explode();
    }

    private void Explode()
    {
        DamageEnemiesAround();

        explosionVfx.transform.parent = null;
        explosionVfx.SetActive(true);
        
        Destroy(gameObject);
    }

    public void SetupSpider(float newDamage)
    {
        damage = newDamage;
        agent.enabled = true;
        transform.parent = null;
    }
    
    private void DamageEnemiesAround()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, damageRadius, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
        
            // Check parent if not found on the collider itself
            if (damageable == null)
            {
                damageable = enemy.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                int newDamage = Mathf.RoundToInt(damage);
                damageable.TakeDamage(newDamage);
            }
        }
    }

    private void UpdateClosestTarget()
    {
        currentTarget = FindClosestEnemy();
    }

    private Transform FindClosestEnemy()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, enemyCheckRadius, whatIsEnemy);
        Transform nearestEnemy = null;
        float shortestDistance = float.MaxValue;

        foreach (Collider enemyCollider in enemiesAround)
        {
            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);

            if (distance < shortestDistance)
            {
                nearestEnemy = enemyCollider.transform;
                shortestDistance = distance;
            }
        }

        return nearestEnemy;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
