using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TowerCrossbow : Tower
{
   private CrossbowVisuals visuals;
   
   [Header("Crossbow Details")] 
   [SerializeField] private Transform gunPoint;
   [SerializeField] private int damage;

   protected override void Awake()
   {
      base.Awake();

      visuals = GetComponent<CrossbowVisuals>();
   }
   
   protected override void Attack()
   {
      Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

      if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity))
      {
         towerHead.forward = directionToEnemy;

         Enemy enemyTarget = null;

         EnemyShield enemyShield = hitInfo.collider.GetComponent<EnemyShield>();
         IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();

         if (damageable != null && enemyShield == null)
         {
            damageable.TakeDamage(damage);
            enemyTarget = currentEnemy;
         }

         if (enemyShield)
         {
            damageable = enemyShield.GetComponent<IDamageable>();
            damageable.TakeDamage(damage);
         }

         visuals.CreateOnHitVFX(hitInfo.point);
         visuals.PlayAttackVFX(gunPoint.position, hitInfo.point, enemyTarget);
         visuals.PlayerReloadVFX(attackCooldown);

         if (AudioManager.instance == null)
         {
            Debug.Log("Audio Manager is null");
            return;
         }
         
         AudioManager.instance.PlaySFX(attackSfx, true);
      }
   }
}
