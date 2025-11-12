using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Projectile used by the TowerCannon.
/// Applies area-of-effect (AOE) damage on impact.
/// </summary>
public class ProjectileCannon : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private Rigidbody rb;
    private float damage;
    
    [SerializeField] private float damageRadius; // The radius of the AOE damage
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private GameObject explosionVfx;
    
    // Cached audio settings
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

    /// <summary>
    /// Initializes the projectile with velocity, damage, and audio settings.
    /// Called from the object pool when the cannon tower fires.
    /// </summary>
    /// <param name="newVelocity">The calculated launch velocity (including gravity compensation).</param>
    /// <param name="newDamage">The damage to apply to each enemy in the AOE.</param>
    /// <param name="newPool">The ObjectPoolManager instance.</param>
    /// <param name="expSfx">The AudioClip for the explosion sound.</param>
    /// <param name="expSfxId">The unique ID for sound limiting.</param>
    /// <param name="expSfxCooldown">The cooldown for the sound.</param>
    /// <param name="maxConcurrent">The max concurrent instances of this sound.</param>
    /// <param name="limitSfx">Whether to apply sound limiting.</param>
    /// <param name="expVolume">The volume for the explosion sound.</param>
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

    /// <summary>
    /// Finds and damages all enemies within the 'damageRadius'.
    /// Skips any enemies that are currently hidden (stealthed).
    /// </summary>
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

    /// <summary>
    /// Triggers the explosion on contact with any valid object.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Projectile hit " + other.gameObject.name);
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Untargetable")) return;
        
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