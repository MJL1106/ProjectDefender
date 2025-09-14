using UnityEngine;

public class EnemySpider : Enemy
{
   private EnemySpiderVisuals spiderVisuals;
   
   protected override void Awake()
   {
      base.Awake();

      spiderVisuals = GetComponent<EnemySpiderVisuals>();
   }

   protected override void Start()
   {
      base.Start();
      
      spiderVisuals.BrieflySpeedUpLegs();
   }

   protected override void ChangeWaypoint()
   {
      spiderVisuals.BrieflySpeedUpLegs();
      base.ChangeWaypoint();
   }

   protected override bool ShouldChangeWaypoint()
   {
      if (nextWaypointIndex >= myWaypoints.Count) return false;

      if (agent.remainingDistance < .5f) return true;

      return false;
   }
}
