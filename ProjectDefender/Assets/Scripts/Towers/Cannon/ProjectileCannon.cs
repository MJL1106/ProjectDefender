using System;
using System.Linq;
using UnityEngine;

public class ProjectileCannon : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private Rigidbody rb;
    private float damage;
    
    [SerializeField] private float damageRadius;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private GameObject explosionVfx;
    
    private AudioClip explosionSfx;
    private string explosionSfxId;
    private float explosionSfxCooldown;
    private int maxConcurrentExplosions;
    private bool limitExplosionSfx;
    private float explosionVolume;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    public void SetupProjectile(Vector3 newVelocity, float newDamage, ObjectPoolManager newPool,
        AudioClip expSfx, string expSfxId, float expSfxCooldown, int maxConcurrent, bool limitSfx, float expVolume)
    {
        trail.Clear();
        objectPool = newPool;
        rb.linearVelocity = newVelocity;
        damage = newDamage;
        
        // Store audio settings
        explosionSfx = expSfx;
        explosionSfxId = expSfxId;
        explosionSfxCooldown = expSfxCooldown;
        maxConcurrentExplosions = maxConcurrent;
        limitExplosionSfx = limitSfx;
        explosionVolume = expVolume;
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
                // Check if it's an Enemy and if it's hidden
                Enemy enemyComponent = damageable as Enemy;
                if (enemyComponent != null && enemyComponent.IsHidden())
                {
                    continue; // Skip hidden enemies
                }
                
                damageable.TakeDamage(damage);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Untargetable"))
        {
            return;
        }
        
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

        objectPool.Get(explosionVfx, transform.position + new Vector3(0, .5f, 0));
        objectPool.Remove(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
