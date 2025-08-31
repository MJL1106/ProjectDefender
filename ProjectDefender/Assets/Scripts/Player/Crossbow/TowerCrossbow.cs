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
         
         visuals.PlayAttackVFX(gunPoint.position, hitInfo.point);
         visuals.PlayerReloadFX(attackCooldown);

         IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();

         if (damageable != null) damageable.TakeDamage(damage);
      }
   }
}
