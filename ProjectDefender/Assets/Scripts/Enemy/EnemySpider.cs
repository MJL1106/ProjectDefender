using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpider : Enemy
{
   private EnemySpiderVisuals spiderVisuals;

   [Header("Emp attack details")] [SerializeField]
   private GameObject empPrefab;

   [SerializeField] private LayerMask whatIsTower;
   [SerializeField] private float towerCheckRadius = 5;
   [SerializeField] private float empCooldown = 8;
   [SerializeField] private float empEffectDuration = 3;
   [SerializeField] private float empDuration = 5;
   private float empAttackTimer;
   
   protected override void Awake()
   {
      base.Awake();

      spiderVisuals = GetComponent<EnemySpiderVisuals>();
   }

   protected override void Start()
   {
      base.Start();
      
      spiderVisuals.BrieflySpeedUpLegs();
      empAttackTimer = empCooldown;
   }

   protected override void Update()
   {
      base.Update();

      empAttackTimer -= Time.deltaTime;
      
      if (empAttackTimer < 0) AttemptToEmp();
   }

   private void AttemptToEmp()
   {
      Transform target = FindRandomTower();

      if (target == null) return;

      empAttackTimer = empCooldown;

      GameObject newEmp = Instantiate(empPrefab, transform.position + new Vector3(0, .15f, 0), Quaternion.identity);
      newEmp.GetComponent<EnemySpiderEMP>().SetupEMP(empEffectDuration, target.position, empDuration);
   }

   private Transform FindRandomTower()
   {
      Collider[] towers = Physics.OverlapSphere(transform.position, towerCheckRadius, whatIsTower);

      if (towers.Length > 0) return towers[Random.Range(0, towers.Length)].transform.root;

      return null;
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

   private void OnDrawGizmos()
   {
      Gizmos.DrawWireSphere(transform.position, towerCheckRadius);
   }
}
