using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
   
   protected override void Attack()
   {
      base.Attack();
      
      Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

      if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
      {
         towerHead.forward = directionToEnemy;
         
         IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();
         damageable.TakeDamage(damage);
         
         visuals.CreateOnHitVFX(hitInfo.point);
         visuals.PlayAttackVFX(gunPoint.position, hitInfo.point);
         visuals.PlayerReloadVFX(attackCooldown);
         
         AudioManager.instance?.PlaySFX(attackSfx, true);
      }
   }
}
