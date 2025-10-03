using System;
using UnityEngine;

public class ProjectileMachineGun : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    private TrailRenderer trail;
    
    private IDamageable damageable;
    private Vector3 target;
    private float damage;
    private float speed;
    private float threshold = .01f;
    private bool isActive = true;

    [SerializeField] private GameObject onHitFx;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

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
        if (isActive == false)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude <= threshold * threshold)
        {
            isActive = false;
            damageable.TakeDamage(damage);

            objectPool.Get(onHitFx,transform.position);
            objectPool.Remove(gameObject);   
        }
    }
}
