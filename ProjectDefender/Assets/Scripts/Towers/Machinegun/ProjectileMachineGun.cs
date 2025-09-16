using System;
using UnityEngine;

public class ProjectileMachineGun : MonoBehaviour
{
    private IDamageable damageable;
    private Vector3 target;
    private float damage;
    private float speed;
    private bool isActive = true;

    [SerializeField] private GameObject onHitFx;
    
    public void SetupProjectile(Vector3 targetPosition, IDamageable newDamageable, float newDamage, float newSpeed)
    {
        target = targetPosition;
        damageable = newDamageable;

        damage = newDamage;
        speed = newSpeed;
    }

    private void Update()
    {
        if (isActive == false) return;
        
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) <= .01f)
        {
            isActive = false;
            damageable.TakeDamage(Mathf.RoundToInt(damage));
            
            onHitFx.SetActive(true);
            Destroy(gameObject,1);
        }
    }
}
