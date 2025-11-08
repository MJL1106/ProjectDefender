using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A tower that fires an instant-hit raycast (hitscan).
/// Uses CrossbowVisuals to display the attack.
/// </summary>
public class TowerCrossbow : Tower
{
    private CrossbowVisuals visuals;
   
    [Header("Crossbow Details")] 
    [SerializeField] private int damage;

    protected override void Awake()
    {
        base.Awake();
        visuals = GetComponent<CrossbowVisuals>();
    }
   
    /// <summary>
    /// Fires a raycast. If it hits a damageable target, applies damage
    /// and triggers all associated visuals.
    /// </summary>
    protected override void Attack()
    {
        base.Attack();
    
        Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

        if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
        {
            towerHead.forward = directionToEnemy;
        
            IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();
        
            if (damageable == null) return;
        
            damageable.TakeDamage(damage);
        
            visuals.CreateOnHitVFX(hitInfo.point);
            visuals.PlayAttackVFX(gunPoint.position, hitInfo.point);
            visuals.PlayerReloadVFX(attackCooldown);
        
            PlayTowerAttackSound();
        }
    }
}