using System;
using UnityEngine;

/// <summary>
/// A fast-moving projectile that travels to a specific point.
/// Applies damage to a pre-defined target upon arrival.
/// </summary>
public class ProjectileMachineGun : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    private TrailRenderer trail;
    
    private IDamageable damageable;
    private Vector3 target;
    private float damage;
    private float speed;
    private float threshold = .01f; // The distance to the target at which the projectile "hits"
    private bool isActive = true;

    [SerializeField] private GameObject onHitFx;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Initializes the projectile with a target point, damageable object, and stats.
    /// </summary>
    /// <param name="targetPosition">The exact world-space point to fly towards.</param>
    /// <param name="newDamageable">The enemy interface to damage upon arrival.</param>
    /// <param name="newDamage">The amount of damage to deal.</param>
    /// <param name="newSpeed">The projectile's travel speed.</param>
    /// <param name="newObjectPool">The object pool manager instance.</param>
    public void SetupProjectile(Vector3 targetPosition, IDamageable newDamageable, float newDamage, float newSpeed, ObjectPoolManager newObjectPool)
    {
        trail.Clear();
        objectPool = newObjectPool;
        isActive = true;

        target = targetPosition;
        damageable = newDamageable;

        damage = newDamage;
        speed = newSpeed;
    }

    private void Update()
    {
        if (isActive == false) return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Check if the projectile has reached the target position
        if ((transform.position - target).sqrMagnitude <= threshold * threshold)
        {
            isActive = false;
            damageable.TakeDamage(damage);

            objectPool.Get(onHitFx,transform.position);
            objectPool.Remove(gameObject);   
        }
    }
}